using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZoneFlow.Battle;

namespace ZoneFlow
{
    /// <summary>
    /// 전투 모드. 헤드리스 결정론 auto-policy로 전투를 종료까지 구동하고,
    /// 결과를 <see cref="BattleService"/>에 기록한 뒤 pop으로 복귀한다(ADR-0002).
    /// </summary>
    public sealed class BattleMode : GamePlayMode
    {
        /// <summary>ZoneAsset과 스폰 포인트 ID로 전투 모드를 생성한다.</summary>
        public BattleMode(ZoneAsset zoneAsset = null, string spawnPointId = null)
            : base(zoneAsset, spawnPointId, zoneAsset != null) { }

        /// <summary>
        /// 전투 진입 연출 직후 호출된다.
        /// DefaultEncounter → CombatantFactory → BattleEngine → auto-policy → SetOutcome → pop.
        /// </summary>
        protected override async UniTask OnModeInAsync(CancellationToken ct)
        {
            var service = BattleService.Instance;
            Debug.Assert(service != null, "[BattleMode] BattleService.Instance가 null이다. CoreServices 씬에 배치됐는지 확인하라.");
            if (service == null)
            {
                await Director.NavigateAsync("gameplay://pop", ct);
                return;
            }

            var encounter = service.DefaultEncounter;
            Debug.Assert(encounter != null, "[BattleMode] BattleService.DefaultEncounter가 null이다. Inspector에서 할당하라.");
            if (encounter == null)
            {
                await Director.NavigateAsync("gameplay://pop", ct);
                return;
            }

            // SO → POCO 변환
            var setup  = CombatantFactory.BuildSetup(encounter);
            var engine = service.StartBattle(setup);

            // 단순 결정론 auto-policy: 현재 행동자가 첫 번째 생존 적을 기본공격
            while (engine.State == BattleState.Ongoing)
            {
                var actor = engine.Current;
                Debug.Assert(actor != null, "[BattleMode] BattleState.Ongoing인데 Current가 null이다.");
                if (actor == null) break;

                var target = FindFirstAliveOpponent(engine, actor);
                Debug.Assert(target != null,
                    $"[BattleMode] {actor.Id}({actor.Side})의 상대 생존자가 없는데 Ongoing 상태다.");
                if (target == null) break;

                engine.SubmitAction(new BattleAction(
                    kind:       BattleActionKind.Attack,
                    actorId:    actor.Id,
                    targetId:   target.Id,
                    skillPower: null));

                // 무한 루프 방지: 취소 토큰 점검
                ct.ThrowIfCancellationRequested();
            }

            var outcome = engine.ToOutcome();
            Debug.Assert(outcome != null, "[BattleMode] 전투가 종료됐는데 ToOutcome()이 null이다.");

            if (outcome != null)
                service.SetOutcome(outcome);

            await Director.NavigateAsync("gameplay://pop", ct);
        }

        // ─────────────────────────────────────────────────────────────
        // 내부 헬퍼
        // ─────────────────────────────────────────────────────────────

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
    }
}
