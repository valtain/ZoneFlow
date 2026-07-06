using System.Collections.Generic;
using UnityEngine;

namespace ZoneFlow.Battle
{
    /// <summary>전투 진행 상태.</summary>
    public enum BattleState
    {
        /// <summary>전투 진행 중.</summary>
        Ongoing,

        /// <summary>플레이어 파티 승리.</summary>
        PlayerWon,

        /// <summary>플레이어 파티 패배.</summary>
        PlayerLost,
    }

    /// <summary>
    /// 헤드리스 결정론 전투 엔진.
    /// <para>
    /// MonoBehaviour·씬·코루틴에 의존하지 않으며, EditMode에서 씬 없이 검증 가능하다.
    /// 공개 표면: <see cref="State"/>·<see cref="Current"/>·<see cref="SubmitAction"/>.
    /// 호출자가 액션을 밀어넣으면 엔진이 결정론적으로 적용·사망 판정·다음 행동자 전진을 수행한다.
    /// </para>
    /// </summary>
    public sealed class BattleEngine
    {
        private readonly List<Combatant> _all;
        private readonly TurnOrder _turnOrder;
        private readonly BattleRng _rng;

        private Combatant _current;

        /// <summary>현재 전투 진행 상태.</summary>
        public BattleState State { get; private set; }

        /// <summary>
        /// 현재 행동 순번의 전투원.
        /// 전투가 종료되었거나 생존자가 없으면 null.
        /// </summary>
        public Combatant Current => State == BattleState.Ongoing ? _current : null;

        /// <summary>
        /// 전투에 참여하는 모든 전투원(파티 + 적).
        /// 사망자를 포함한 전체 목록을 반환한다.
        /// 후속 UI 레이어나 테스트에서 대상 검색에 사용된다.
        /// </summary>
        public IReadOnlyList<Combatant> AllCombatants => _all;

        /// <summary>
        /// 전투 엔진을 초기화한다.
        /// </summary>
        /// <param name="setup">전투 설정(파티·적·시드).</param>
        public BattleEngine(BattleSetup setup)
        {
            Debug.Assert(setup != null, "BattleEngine: setup이 null이다.");

            _rng = new BattleRng(setup.Seed);

            // 전체 전투원 리스트 구성
            _all = new List<Combatant>(setup.Party.Count + setup.Enemies.Count);
            _all.AddRange(setup.Party);
            _all.AddRange(setup.Enemies);

            _turnOrder = new TurnOrder(_all);
            State      = BattleState.Ongoing;
            _current   = _turnOrder.Next();
        }

        /// <summary>
        /// 현재 행동자의 액션을 결정론적으로 처리한다.
        /// <para>
        /// 처리 순서: 데미지 적용 → 사망 판정 → 승패 체크 → 다음 행동자 전진.
        /// </para>
        /// </summary>
        /// <param name="action">제출할 액션.</param>
        /// <returns>데미지·사망 정보를 담은 결과.</returns>
        public ActionResult SubmitAction(BattleAction action)
        {
            Debug.Assert(State == BattleState.Ongoing,
                "BattleEngine.SubmitAction: 이미 종료된 전투에 액션을 제출했다.");
            Debug.Assert(_current != null,
                "BattleEngine.SubmitAction: Current가 null인 상태에서 SubmitAction이 호출됐다.");

            var target = FindById(action.TargetId);
            Debug.Assert(target != null,
                $"BattleEngine.SubmitAction: TargetId({action.TargetId})에 해당하는 전투원이 없다.");
            Debug.Assert(target.IsAlive,
                $"BattleEngine.SubmitAction: 대상(id={action.TargetId})이 이미 사망 상태다.");

            int power  = action.SkillPower ?? 0;
            int damage = DamageCalculator.Compute(_current, target, power, _rng);
            target.ApplyDamage(damage);

            bool killed = !target.IsAlive;

            // 승패 판정
            if (killed)
                EvaluateVictory();

            // 전투가 아직 진행 중이면 다음 행동자 전진
            if (State == BattleState.Ongoing)
                _current = _turnOrder.Next();

            return new ActionResult(damageDealt: damage, isKilled: killed);
        }

        /// <summary>
        /// 현재 엔진 상태를 <see cref="BattleOutcome"/> 으로 변환한다.
        /// 전투가 종료되지 않았으면 null을 반환한다.
        /// </summary>
        public BattleOutcome ToOutcome()
        {
            return State switch
            {
                BattleState.PlayerWon  => new BattleOutcome(BattleResult.Win),
                BattleState.PlayerLost => new BattleOutcome(BattleResult.Lose),
                _                      => null,
            };
        }

        // ─────────────────────────────────────────────────────────────
        // 내부 헬퍼
        // ─────────────────────────────────────────────────────────────

        private Combatant FindById(int id)
        {
            foreach (var c in _all)
                if (c.Id == id) return c;
            return null;
        }

        private void EvaluateVictory()
        {
            bool allEnemiesDead = true;
            bool allPartyDead   = true;

            foreach (var c in _all)
            {
                if (c.Side == BattleSide.Enemy  && c.IsAlive) allEnemiesDead = false;
                if (c.Side == BattleSide.Player && c.IsAlive) allPartyDead   = false;
            }

            if (allEnemiesDead)
                State = BattleState.PlayerWon;
            else if (allPartyDead)
                State = BattleState.PlayerLost;
        }
    }
}
