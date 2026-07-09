using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;
using ZoneFlow.Battle;

namespace ZoneFlow.BattleView
{
    /// <summary>
    /// 전투 아레나 연출 오케스트레이터. BossRoom 등 전투 Zone에 저작되며 <c>BattleMode</c>가 구동한다.
    /// 결정론 엔진(<see cref="ActionResult"/>/<see cref="Combatant"/>)만 소비하고, 엔진은 이 타입을 모른다.
    /// </summary>
    public sealed class BattleStage : MonoBehaviour
    {
        [SerializeField] private Transform[] _allyAnchors;
        [SerializeField] private Transform[] _enemyAnchors;
        [SerializeField] private CinemachineCamera _battleCamera;
        [SerializeField] private BattleActorView _actorViewPrefab;
        [SerializeField] private Material _allyMaterial;
        [SerializeField] private Material _enemyMaterial;
        [SerializeField] private BattleDamageNumber _damageNumberPrefab;
        [SerializeField] private int _activePriority = 100;
        [SerializeField] private int _inactivePriority = -10;

        private const float DamageNumberHeight = 1.6f;
        private const float ShakeDuration = 0.3f;
        private static readonly Vector3 ShakeStrength = new(0.15f, 0.1f, 0f);

        private readonly Dictionary<int, BattleActorView> _views = new();

        private void Awake()
        {
            Debug.Assert(_allyAnchors != null && _allyAnchors.Length > 0, "[BattleStage] _allyAnchors가 비었다.");
            Debug.Assert(_enemyAnchors != null && _enemyAnchors.Length > 0, "[BattleStage] _enemyAnchors가 비었다.");
            Debug.Assert(_battleCamera != null, "[BattleStage] _battleCamera가 할당되지 않았다.");
            Debug.Assert(_actorViewPrefab != null, "[BattleStage] _actorViewPrefab이 할당되지 않았다.");
        }

        /// <summary>setup 기준으로 아군/적 뷰를 앵커에 스폰하고 side 머티리얼·HUD 이름/HP를 적용한다.</summary>
        /// <param name="setup">전투 초기화 데이터.</param>
        /// <param name="names">전투원 Id → 표시명 맵(HUD 이름 라벨용).</param>
        public void SetupViews(BattleSetup setup, IReadOnlyDictionary<int, string> names)
        {
            Debug.Assert(setup != null, "[BattleStage] SetupViews: setup이 null이다.");
            if (setup == null) return;

            ClearViews();
            SpawnSide(setup.Party, _allyAnchors, _allyMaterial, names);
            SpawnSide(setup.Enemies, _enemyAnchors, _enemyMaterial, names);
        }

        /// <summary>
        /// 지정한 타겟 후보들의 3D 캡슐을 클릭 가능하게 하고 어포던스 틴트를 적용한다.
        /// <see cref="IDisposable.Dispose"/> 시 원복해 타겟 선택 단계 종료를 표현한다.
        /// </summary>
        /// <param name="ids">선택 가능한 전투원 Id 목록.</param>
        /// <param name="onPicked">클릭된 전투원 Id를 통지하는 콜백.</param>
        public IDisposable BeginTargetPicking(IReadOnlyList<int> ids, Action<int> onPicked)
        {
            var activated = new List<BattleActorView>();
            if (ids != null)
            {
                foreach (var id in ids)
                {
                    var view = GetView(id);
                    if (view == null) continue;

                    view.SetPickable(true, onPicked);
                    activated.Add(view);
                }
            }
            return new TargetPickingScope(activated);
        }

        /// <summary>전투 카메라 우선도를 올려 CinemachineBrain이 전투캠으로 블렌드하게 한다.</summary>
        public void ActivateBattleCamera()
        {
            if (_battleCamera == null) return;
            _battleCamera.Priority = _activePriority;
        }

        /// <summary>전투 카메라 우선도를 낮춰 원래 카메라로 블렌드 복귀시킨다.</summary>
        public void ReleaseBattleCamera()
        {
            if (_battleCamera == null) return;
            _battleCamera.Priority = _inactivePriority;
        }

        /// <summary>
        /// 액션 결과를 4비트로 연출한다: 런지 → 피격 → 데미지 숫자 → (처치 시) 사망+카메라 흔들림.
        /// 1~3은 인과 가독성을 위해 순차, 4는 단일 임팩트를 위해 병렬로 재생한다.
        /// </summary>
        /// <param name="result">엔진이 반환한 액션 결과.</param>
        /// <param name="actor">행동자 전투원.</param>
        /// <param name="target">대상 전투원.</param>
        /// <param name="ct">취소 토큰.</param>
        public async UniTask PlayActionAsync(ActionResult result, Combatant actor, Combatant target, CancellationToken ct)
        {
            var attackerView = GetView(actor?.Id);
            var targetView = GetView(target?.Id);
            if (attackerView == null || targetView == null) return;

            await attackerView.LungeAsync(targetView.Position, ct);
            await targetView.HitReactAsync(attackerView.Position, ct);
            targetView.SetHp(target.Hp, target.MaxHp);
            await SpawnDamageNumberAsync(targetView.Position, result.DamageDealt, ct);

            if (result.IsKilled)
                await UniTask.WhenAll(targetView.DieAsync(ct), ShakeCameraAsync(ct));
        }

        /// <summary>스폰된 뷰를 모두 파괴하고 전투 카메라를 릴리즈한다(Zone은 ref-counted 재사용이라 stale 뷰를 남기지 않는다).</summary>
        public void Teardown()
        {
            ClearViews();
            ReleaseBattleCamera();
        }

        private void SpawnSide(
            IReadOnlyList<Combatant> combatants, Transform[] anchors, Material material,
            IReadOnlyDictionary<int, string> names)
        {
            if (_actorViewPrefab == null || anchors == null || combatants == null) return;

            for (int i = 0; i < combatants.Count; i++)
            {
                if (i >= anchors.Length)
                {
                    Debug.Assert(false, $"[BattleStage] 앵커 부족: {combatants.Count}명인데 앵커가 {anchors.Length}개다.");
                    break;
                }

                var anchor = anchors[i];
                var combatant = combatants[i];
                var view = Instantiate(_actorViewPrefab, anchor.position, anchor.rotation, transform);
                view.ApplyMaterial(material);
                view.SetCombatantId(combatant.Id);
                view.SetName(ResolveName(combatant.Id, names));
                view.SetHp(combatant.Hp, combatant.MaxHp);
                _views[combatant.Id] = view;
            }
        }

        private static string ResolveName(int id, IReadOnlyDictionary<int, string> names)
            => names != null && names.TryGetValue(id, out var name) ? name : $"Combatant {id}";

        private async UniTask SpawnDamageNumberAsync(Vector3 targetPos, int amount, CancellationToken ct)
        {
            if (_damageNumberPrefab == null) return;

            var spawnPos = targetPos + Vector3.up * DamageNumberHeight;
            var instance = Instantiate(_damageNumberPrefab, spawnPos, Quaternion.identity, transform);
            await instance.PlayAsync(amount, ct);
        }

        private async UniTask ShakeCameraAsync(CancellationToken ct)
        {
            if (_battleCamera == null) return;
            await Tween.ShakeLocalPosition(_battleCamera.transform, ShakeStrength, ShakeDuration)
                .ToUniTask(cancellationToken: ct);
        }

        private BattleActorView GetView(int? combatantId)
            => combatantId.HasValue && _views.TryGetValue(combatantId.Value, out var view) ? view : null;

        private void ClearViews()
        {
            foreach (var view in _views.Values)
                if (view != null) Destroy(view.gameObject);
            _views.Clear();
        }

        /// <summary>Dispose 시 활성화됐던 뷰들의 피킹 가능 상태를 원복하는 스코프 핸들.</summary>
        private sealed class TargetPickingScope : IDisposable
        {
            private readonly List<BattleActorView> _views;

            public TargetPickingScope(List<BattleActorView> views) => _views = views;

            public void Dispose()
            {
                foreach (var view in _views)
                    if (view != null) view.SetPickable(false, null);
            }
        }
    }
}
