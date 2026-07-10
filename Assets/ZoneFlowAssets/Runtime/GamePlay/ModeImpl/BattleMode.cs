using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZoneFlow.Battle;
using ZoneFlow.BattleView;
using ZoneFlow.Player;

namespace ZoneFlow
{
    /// <summary>
    /// 전투 모드. <see cref="BattlePanel"/>로 플레이어 턴 입력을 받아 전투를 종료까지 구동하고,
    /// 결과를 <see cref="BattleService"/>에 기록한 뒤 pop으로 복귀한다(ADR-0002).
    /// 적 턴은 결정론 auto-policy(첫 생존 상대 기본공격)로 진행한다.
    /// 3D 연출은 <see cref="BattleStage"/>(있으면)에 위임하는 병렬 프레젠테이션 레이어이며 엔진 호출에는 영향을 주지 않는다.
    /// </summary>
    public sealed class BattleMode : GamePlayMode
    {
        private const string BasicAttackLabel = "Attack";

        private BattlePanel _panel;
        private BattleService _service;
        private BattleEncounterAsset _encounter;
        private BattleSetup _setup;
        private BattleStage _stage;

        private readonly Dictionary<int, string> _names = new();
        private readonly Dictionary<int, BattleActorView> _viewsById = new();
        private readonly Dictionary<int, IReadOnlyList<BattlePanel.BattleActionOption>> _optionsById = new();

        /// <summary>ZoneAsset과 스폰 포인트 ID로 전투 모드를 생성한다.</summary>
        public BattleMode(ZoneAsset zoneAsset = null, string spawnPointId = null)
            : base(zoneAsset, spawnPointId, zoneAsset != null) { }

        /// <summary>
        /// Zone(아레나) 로드 직후 호출된다.
        /// DefaultEncounter → CombatantFactory로 setup을 구성하고, 전면 전투 패널을 숨김 상태로 생성한다.
        /// </summary>
        protected override async UniTask OnPlayedAsync(CancellationToken ct)
        {
            _service = BattleService.Instance;
            Debug.Assert(_service != null, "[BattleMode] BattleService.Instance가 null이다. CoreServices 씬에 배치됐는지 확인하라.");
            if (_service == null) return;

            _encounter = _service.DefaultEncounter;
            Debug.Assert(_encounter != null, "[BattleMode] BattleService.DefaultEncounter가 null이다. Inspector에서 할당하라.");
            if (_encounter == null) return;

            // SO → POCO 변환 + 표시명/행동 선택지 뷰모델 파생
            _setup = CombatantFactory.BuildSetup(_encounter);
            BuildViewModel();

            // 3D 연출 레이어(있으면) 확보 + 뷰 스폰(HUD 이름/HP 초기화 포함). 없으면 null 가드로 2D 패널만 진행한다.
            _stage = Zone != null ? Zone.GetComponentInChildren<BattleStage>(true) : null;
            Debug.Assert(_stage != null, "[BattleMode] BossRoom Zone 아래 BattleStage를 찾지 못했다.");
            _stage?.SetupViews(_setup, _names, _viewsById);

            if (UiService.Instance.Panels == null) return;
            if (!UiService.Instance.Panels.TryGetPanel(BattlePanel.PanelId, out var prefabRef)) return;
            if (prefabRef.asset is not BattlePanel battlePrefab) return;

            _panel = await UiService.Instance.SetMainViewAsync(battlePrefab, ct);

            var roster = new List<Combatant>(_setup.Party.Count + _setup.Enemies.Count);
            roster.AddRange(_setup.Party);
            roster.AddRange(_setup.Enemies);
            _panel.Initialize(roster, _names);
        }

        /// <summary>
        /// 전투 진입 연출 직후 호출된다. 패널을 슬라이드인하고, 전투 루프는 전환이 끝난 뒤 구동한다.
        /// </summary>
        protected override async UniTask OnModeInAsync(CancellationToken ct)
        {
            // OnPlayedAsync에서 서비스/인카운터/패널 준비가 실패했으면 안전하게 pop.
            if (_service == null || _encounter == null || _setup == null || _panel == null)
            {
                DeferredPopAsync(ct).Forget();
                return;
            }

            await UiService.Instance.ShowMainViewAsync(ct);

            // 전투 카메라로 블렌드 + 앵커와 겹치지 않도록 플레이어 메시를 숨긴다(GameObject는 활성 유지 — vcam 블렌드-from 소스).
            _stage?.ActivateBattleCamera();
            SetPlayerMeshVisible(false);

            // 전투 루프는 ModeIn 전환이 끝난 뒤(Active 상태) 구동한다.
            // OnModeInAsync 내부에서 종료 pop을 호출하면 GamePlayDirector의 전환 재진입 가드(한 번에 하나의 전환)에
            // 막혀 드롭되므로, 루프를 fire-and-forget으로 분리해 진행 중인 ModeIn 전환을 먼저 완료시킨다.
            RunBattleAsync(ct).Forget();
        }

        /// <summary>모드 퇴장 시 전투 패널을 슬라이드아웃하고, 전투 카메라를 릴리즈하며 플레이어 메시를 복원한다.</summary>
        protected override UniTask OnModeOutAsync(CancellationToken ct)
        {
            _stage?.ReleaseBattleCamera();
            SetPlayerMeshVisible(true);
            return UiService.Instance.HideMainViewAsync(ct);
        }

        /// <summary>모드 종료 시 3D 연출 뷰를 정리하고 전투 패널 인스턴스를 파괴한다.</summary>
        protected override UniTask OnStoppedAsync(CancellationToken ct)
        {
            _stage?.Teardown();
            UiService.Instance.ClearMainViewIfIs(_panel);
            _panel = null;
            return UniTask.CompletedTask;
        }

        // ─────────────────────────────────────────────────────────────
        // 전투 루프 (Active 상태에서 구동 — 전환 밖)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 인터랙티브 턴 루프. 플레이어 턴은 패널 입력을 await, 적 턴은 auto-policy로 진행하고,
        /// 종료 시 결과를 기록한 뒤 pop한다. ModeIn 전환이 풀린 뒤 실행돼 재진입 가드를 피한다.
        /// </summary>
        private async UniTask RunBattleAsync(CancellationToken ct)
        {
            // 진행 중이던 ModeIn 전환이 완전히 풀린 다음 프레임에 시작한다(_isNavigating 해제 보장).
            await UniTask.Yield();

            var engine = _service.StartBattle(_setup);

            while (engine.State == BattleState.Ongoing)
            {
                var actor = engine.Current;
                Debug.Assert(actor != null, "[BattleMode] BattleState.Ongoing인데 Current가 null이다.");
                if (actor == null) break;

                BattleAction action;
                if (actor.Side == BattleSide.Player)
                {
                    var options = ResolveOptions(actor);
                    var aliveTargets = CollectAliveOpponents(engine, actor);
                    Debug.Assert(aliveTargets.Count > 0,
                        $"[BattleMode] 플레이어 {actor.Id}의 생존 타겟이 없는데 Ongoing 상태다.");
                    if (aliveTargets.Count == 0) break;

                    action = await _panel.AwaitPlayerActionAsync(actor, options, aliveTargets, ct,
                        _stage != null ? _stage.BeginTargetPicking : null);
                }
                else
                {
                    var target = FindFirstAliveOpponent(engine, actor);
                    Debug.Assert(target != null,
                        $"[BattleMode] {actor.Id}({actor.Side})의 상대 생존자가 없는데 Ongoing 상태다.");
                    if (target == null) break;

                    action = new BattleAction(
                        kind:       BattleActionKind.Attack,
                        actorId:    actor.Id,
                        targetId:   target.Id,
                        skillPower: null);
                }

                // 제출 전에 대상 전투원을 확보해 결과 연출에 전달한다.
                var targetCombatant = FindById(engine, action.TargetId);
                var result = engine.SubmitAction(action);

                var presentPanel = _panel.PresentActionAsync(result, actor, targetCombatant, ct);
                var presentStage = _stage != null
                    ? _stage.PlayActionAsync(result, actor, targetCombatant, ct)
                    : UniTask.CompletedTask;
                await UniTask.WhenAll(presentPanel, presentStage);

                ct.ThrowIfCancellationRequested();
            }

            var outcome = engine.ToOutcome();
            Debug.Assert(outcome != null, "[BattleMode] 전투가 종료됐는데 ToOutcome()이 null이다.");

            if (outcome != null)
                _service.SetOutcome(outcome);

            await Director.NavigateAsync("gameplay://pop", ct);
        }

        /// <summary>준비 실패 시, 진행 중이던 ModeIn 전환이 풀린 뒤 안전하게 pop한다.</summary>
        private async UniTask DeferredPopAsync(CancellationToken ct)
        {
            await UniTask.Yield();
            await Director.NavigateAsync("gameplay://pop", ct);
        }

        // ─────────────────────────────────────────────────────────────
        // 뷰모델 파생
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// encounter와 setup을 인덱스 zip으로 대응시켜 표시명 맵과 플레이어 행동 선택지 맵을 파생한다.
        /// CombatantFactory가 party를 encounter.Party 순서(Id 오름차순)로, 이어서 enemies를 배치하는 규칙에 의존한다.
        /// </summary>
        private void BuildViewModel()
        {
            _names.Clear();
            _viewsById.Clear();
            _optionsById.Clear();

            for (int i = 0; i < _setup.Party.Count; i++)
            {
                var combatant = _setup.Party[i];
                var persona   = _encounter.Party[i];
                _names[combatant.Id] = persona.DisplayName;
                _viewsById[combatant.Id] = persona.BattleView;

                var options = new List<BattlePanel.BattleActionOption>
                {
                    new(BasicAttackLabel, null),
                };
                foreach (var skill in persona.Skills)
                {
                    // 현재 엔진은 Damage 스킬만 SubmitAction으로 해석한다(팩토리도 Damage만 보존).
                    if (skill != null && skill.Kind == SkillKind.Damage)
                        options.Add(new BattlePanel.BattleActionOption(skill.DisplayName, skill.Power));
                }
                _optionsById[combatant.Id] = options;
            }

            for (int i = 0; i < _setup.Enemies.Count; i++)
            {
                _names[_setup.Enemies[i].Id] = _encounter.Enemies[i].DisplayName;
                _viewsById[_setup.Enemies[i].Id] = _encounter.Enemies[i].BattleView;
            }
        }

        // ─────────────────────────────────────────────────────────────
        // 내부 헬퍼
        // ─────────────────────────────────────────────────────────────

        /// <summary>행동자의 선택지를 뷰모델에서 조회한다. 없으면 기본공격만 제공한다(방어).</summary>
        private IReadOnlyList<BattlePanel.BattleActionOption> ResolveOptions(Combatant actor)
        {
            if (_optionsById.TryGetValue(actor.Id, out var options))
                return options;

            Debug.Assert(false, $"[BattleMode] 플레이어 {actor.Id}의 행동 선택지가 없다.");
            return new[] { new BattlePanel.BattleActionOption(BasicAttackLabel, null) };
        }

        /// <summary>주어진 행동자의 반대 진영 생존 전투원 목록을 반환한다.</summary>
        private static List<Combatant> CollectAliveOpponents(BattleEngine engine, Combatant actor)
        {
            var oppositeSide = actor.Side == BattleSide.Player ? BattleSide.Enemy : BattleSide.Player;
            var result = new List<Combatant>();
            foreach (var c in engine.AllCombatants)
            {
                if (c.Side == oppositeSide && c.IsAlive)
                    result.Add(c);
            }
            return result;
        }

        /// <summary>주어진 행동자의 반대 진영 중 첫 번째 생존 전투원을 반환한다.</summary>
        private static Combatant FindFirstAliveOpponent(BattleEngine engine, Combatant actor)
        {
            var oppositeSide = actor.Side == BattleSide.Player ? BattleSide.Enemy : BattleSide.Player;
            foreach (var c in engine.AllCombatants)
            {
                if (c.Side == oppositeSide && c.IsAlive)
                    return c;
            }
            return null;
        }

        /// <summary>Id로 전투원을 조회한다(결과 연출 대상 확보용).</summary>
        private static Combatant FindById(BattleEngine engine, int id)
        {
            foreach (var c in engine.AllCombatants)
            {
                if (c.Id == id)
                    return c;
            }
            return null;
        }

        /// <summary>
        /// 플레이어 렌더러 일체를 켜거나 끈다(GameObject는 활성 유지 — vcam 블렌드-from 소스).
        /// 3D 스탠드인 앵커와 겹치지 않도록 전투 중에만 숨긴다.
        /// </summary>
        private static void SetPlayerMeshVisible(bool visible)
        {
            var player = PlayerService.IsReady ? PlayerService.Instance.Player : null;
            if (player == null) return;

            foreach (var renderer in player.GetComponentsInChildren<Renderer>(true))
                renderer.enabled = visible;
        }
    }
}
