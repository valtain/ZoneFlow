using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ZoneFlow.BattleView
{
    /// <summary>
    /// 전투원 1명의 3D 스탠드인 뷰. 결정론 엔진(<c>ZoneFlow.Battle</c>)은 이 타입을 모른다.
    /// 프리미티브(캡슐 + side 머티리얼) 기본 구현이며, <c>_renderer</c>를 비우면 실모델 서브클래스가
    /// 자체 렌더링을 담당하도록 no-op 처리한다(교체 seam).
    /// 캡슐 위 월드 HUD(이름·HP 바)와 <see cref="IPointerClickHandler"/>를 통한 3D 타겟 클릭도 담당한다.
    /// </summary>
    public class BattleActorView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Transform _root;
        [SerializeField] private Renderer _renderer;

        [Header("HUD")]
        [SerializeField] private Transform _hudBillboard;
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private RectTransform _hpFillRect;
        [SerializeField] private TextMeshProUGUI _hpText;

        private const float LungeDistanceRatio = 0.8f;
        private const float LungeOutDuration = 0.15f;
        private const float LungeBackDuration = 0.15f;

        private const float HitFlashDuration = 0.1f;
        private const float HitPunchDuration = 0.12f;
        private const float HitPunchStrength = 0.25f;

        private const float DieDuration = 0.5f;
        private const float DieSinkDistance = 0.5f;

        private const float HpFillTweenDuration = 0.35f;
        private static readonly Color PickableTintColor = new(1f, 0.95f, 0.35f);

        private Color _baseColor = Color.white;
        private int _combatantId;
        private bool _pickable;
        private Action<int> _onPicked;

        /// <summary>이 전투원 뷰의 월드 위치(연출 대상 좌표로 사용).</summary>
        public Vector3 Position => _root != null ? _root.position : transform.position;

        /// <summary>
        /// 진영 머티리얼을 적용한다. 실모델 서브클래스는 <c>_renderer</c>를 비워 no-op으로 만들 수 있다.
        /// </summary>
        /// <param name="sideMaterial">적용할 진영 머티리얼(아군/적).</param>
        public void ApplyMaterial(Material sideMaterial)
        {
            if (_renderer == null || sideMaterial == null) return;

            _renderer.material = sideMaterial;
            _baseColor = _renderer.material.color;
        }

        /// <summary>3D 클릭 타겟 선택 통지에 쓰일 전투원 Id를 설정한다.</summary>
        /// <param name="combatantId">스폰 시 대응시킨 전투원 Id.</param>
        public void SetCombatantId(int combatantId) => _combatantId = combatantId;

        /// <summary>HUD 이름 라벨을 갱신한다.</summary>
        /// <param name="displayName">표시할 이름.</param>
        public void SetName(string displayName)
        {
            if (_nameLabel != null) _nameLabel.text = displayName;
        }

        /// <summary>
        /// HUD HP 바와 수치 텍스트를 갱신한다. fill은 sprite/fillAmount 대신 <c>anchorMax.x</c> 트윈으로
        /// 리사이즈해 늘어남 없는 사각 형태를 유지한다.
        /// </summary>
        /// <param name="current">현재 HP.</param>
        /// <param name="max">최대 HP.</param>
        public void SetHp(int current, int max)
        {
            if (_hpText != null) _hpText.text = $"{Mathf.Max(0, current)}/{max}";
            if (_hpFillRect == null || max <= 0) return;

            var ratio = Mathf.Clamp01((float)current / max);
            var startRatio = _hpFillRect.anchorMax.x;
            Tween.Custom(startRatio, ratio, HpFillTweenDuration,
                v => _hpFillRect.anchorMax = new Vector2(v, _hpFillRect.anchorMax.y));
        }

        /// <summary>
        /// 3D 타겟 클릭 가능 여부를 토글한다. 가능하면 어포던스 틴트를 적용하고 클릭 콜백을 등록,
        /// 아니면 원래 진영 색으로 원복하고 콜백을 해제한다.
        /// </summary>
        /// <param name="pickable">클릭 가능 여부.</param>
        /// <param name="onPicked">클릭 시 전투원 Id를 통지하는 콜백(가능할 때만 사용).</param>
        public void SetPickable(bool pickable, Action<int> onPicked)
        {
            _pickable = pickable;
            _onPicked = pickable ? onPicked : null;

            if (_renderer == null) return;
            _renderer.material.color = pickable ? PickableTintColor : _baseColor;
        }

        /// <summary>EventSystem(PhysicsRaycaster)을 통한 3D 클릭. 피킹 가능 상태에서만 콜백을 통지한다.</summary>
        /// <param name="eventData">클릭 이벤트 데이터(미사용).</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_pickable) return;
            _onPicked?.Invoke(_combatantId);
        }

        /// <summary>대상 쪽으로 80% 전진 후 복귀하는 공격 런지 연출.</summary>
        /// <param name="targetPos">런지 목표 월드 위치.</param>
        /// <param name="ct">취소 토큰.</param>
        public async UniTask LungeAsync(Vector3 targetPos, CancellationToken ct)
        {
            if (_root == null) return;

            var origin = _root.position;
            var lungePos = Vector3.Lerp(origin, targetPos, LungeDistanceRatio);

            await Tween.Position(_root, lungePos, LungeOutDuration, Ease.OutQuad)
                .ToUniTask(cancellationToken: ct);
            await Tween.Position(_root, origin, LungeBackDuration, Ease.InQuad)
                .ToUniTask(cancellationToken: ct);
        }

        /// <summary>화이트 플래시(머티리얼 컬러 요요)와 공격자 반대 방향 펀치를 동시에 재생하는 피격 연출.</summary>
        /// <param name="fromAttackerPos">공격자 월드 위치(펀치 방향 계산용).</param>
        /// <param name="ct">취소 토큰.</param>
        public async UniTask HitReactAsync(Vector3 fromAttackerPos, CancellationToken ct)
        {
            if (_root == null) return;

            var punchDir = _root.position - fromAttackerPos;
            punchDir.y = 0f;
            punchDir = punchDir.sqrMagnitude > 0.0001f ? punchDir.normalized : _root.forward;

            var flashTask = _renderer != null
                ? Tween.MaterialColor(_renderer.material, _baseColor, Color.white, HitFlashDuration,
                    cycles: 2, cycleMode: CycleMode.Yoyo).ToUniTask(cancellationToken: ct)
                : UniTask.CompletedTask;

            var punchTask = Tween.PunchLocalPosition(_root, punchDir * HitPunchStrength, HitPunchDuration)
                .ToUniTask(cancellationToken: ct);

            await UniTask.WhenAll(flashTask, punchTask);
        }

        /// <summary>기본 사망 연출: 스케일을 0으로 줄이며 살짝 침강시킨다(URP-Lit opaque는 alpha 페이드 불가).</summary>
        /// <param name="ct">취소 토큰.</param>
        public async UniTask DieAsync(CancellationToken ct)
        {
            if (_root == null) return;

            var sinkPos = _root.position + Vector3.down * DieSinkDistance;

            var scaleTask = Tween.Scale(_root, 0f, DieDuration, Ease.InQuad).ToUniTask(cancellationToken: ct);
            var sinkTask = Tween.Position(_root, sinkPos, DieDuration, Ease.InQuad).ToUniTask(cancellationToken: ct);

            await UniTask.WhenAll(scaleTask, sinkTask);
        }

        /// <summary>HUD 캔버스를 매 프레임 메인 카메라 회전으로 정렬해 빌보드처럼 보이게 한다.</summary>
        private void LateUpdate()
        {
            if (_hudBillboard == null || !CameraService.IsReady) return;
            var cam = CameraService.Instance.MainCamera;
            if (cam == null) return;

            _hudBillboard.rotation = cam.transform.rotation;
        }
    }
}
