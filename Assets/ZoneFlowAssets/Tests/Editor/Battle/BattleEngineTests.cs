using System.Collections.Generic;
using NUnit.Framework;
using ZoneFlow.Battle;

namespace ZoneFlow.Tests.Editor.Battle
{
    /// <summary>
    /// TC-04/05/06: BattleEngine 기본공격·스킬 HP 감소, 승패 판정, 결정론 트랜스크립트 검증.
    /// </summary>
    internal class BattleEngineTests
    {
        // ─────────────────────────────────────────────────────────────
        // 헬퍼
        // ─────────────────────────────────────────────────────────────

        private static BattleSetup MakeSetup(int seed = 0,
            int partyHp = 200, int partyAttack = 30, int partySpeed = 10,
            int enemyHp =  50, int enemyAttack = 10, int enemySpeed =  5)
        {
            return new BattleSetup(
                party: new List<Combatant>
                {
                    new Combatant(id: 1, side: BattleSide.Player,
                        maxHp: partyHp, attack: partyAttack, speed: partySpeed),
                },
                enemies: new List<Combatant>
                {
                    new Combatant(id: 2, side: BattleSide.Enemy,
                        maxHp: enemyHp, attack: enemyAttack, speed: enemySpeed),
                },
                seed: seed);
        }

        // ─────────────────────────────────────────────────────────────
        // TC-04: 기본공격 HP 감소
        // ─────────────────────────────────────────────────────────────

        /// <summary>기본공격 SubmitAction이 대상 HP를 양수량만큼 감소시킨다.</summary>
        [Test]
        public void SubmitAction_BasicAttack_ReducesTargetHp()
        {
            var setup  = MakeSetup(seed: 1);
            var engine = new BattleEngine(setup);

            // Current = 파티원(id:1), 대상 = 적(id:2)
            var actor   = engine.Current;
            var enemy   = setup.Enemies[0];
            int hpBefore = enemy.Hp;

            var action = new BattleAction(
                kind:     BattleActionKind.Attack,
                actorId:  actor.Id,
                targetId: enemy.Id,
                skillPower: null);

            var result = engine.SubmitAction(action);

            Assert.Greater(result.DamageDealt, 0, "데미지가 0 이하였다");
            Assert.Less(enemy.Hp, hpBefore, "적 HP가 감소하지 않았다");
        }

        /// <summary>Skill 액션이 기본공격보다 Power에 비례한 더 큰 데미지를 줄 수 있다.</summary>
        [Test]
        public void SubmitAction_SkillWithHighPower_DealsDamage()
        {
            var setup  = MakeSetup(seed: 2);
            var engine = new BattleEngine(setup);

            var actor   = engine.Current;
            var enemy   = setup.Enemies[0];
            int hpBefore = enemy.Hp;

            var action = new BattleAction(
                kind:       BattleActionKind.Skill,
                actorId:    actor.Id,
                targetId:   enemy.Id,
                skillPower: 50);

            var result = engine.SubmitAction(action);

            Assert.Greater(result.DamageDealt, 0);
            Assert.Less(enemy.Hp, hpBefore);
        }

        // ─────────────────────────────────────────────────────────────
        // TC-04: HP 0 이탈 (IsKilled 플래그)
        // ─────────────────────────────────────────────────────────────

        /// <summary>단타에 HP가 0이 되면 ActionResult.IsKilled == true.</summary>
        [Test]
        public void SubmitAction_KillsEnemy_ActionResultIsKilledTrue()
        {
            // 극단적 공격력으로 1격 사살 보장
            var setup = new BattleSetup(
                party: new List<Combatant>
                {
                    new Combatant(id: 1, side: BattleSide.Player, maxHp: 500, attack: 9999, speed: 10),
                },
                enemies: new List<Combatant>
                {
                    new Combatant(id: 2, side: BattleSide.Enemy,  maxHp:  10, attack:   1, speed:  1),
                },
                seed: 0);

            var engine = new BattleEngine(setup);
            var action = new BattleAction(
                kind:     BattleActionKind.Attack,
                actorId:  1,
                targetId: 2,
                skillPower: null);

            var result = engine.SubmitAction(action);
            Assert.IsTrue(result.IsKilled, "적이 죽었는데 IsKilled가 false");
        }

        // ─────────────────────────────────────────────────────────────
        // TC-05: 승패 판정
        // ─────────────────────────────────────────────────────────────

        /// <summary>적 팀 전멸 시 State == PlayerWon, Current == null.</summary>
        [Test]
        public void AutoResolve_PartyWins_StateIsPlayerWon()
        {
            // 파티가 압도적으로 강한 세팅
            var setup = MakeSetup(seed: 0,
                partyHp: 1000, partyAttack: 9999, partySpeed: 20,
                enemyHp:    1, enemyAttack:    1, enemySpeed:  1);

            var engine = new BattleEngine(setup);
            AutoResolve(engine);

            Assert.AreEqual(BattleState.PlayerWon, engine.State);
            Assert.IsNull(engine.Current);
        }

        /// <summary>파티 전멸 시 State == PlayerLost, Current == null.</summary>
        [Test]
        public void AutoResolve_EnemyWins_StateIsPlayerLost()
        {
            var setup = MakeSetup(seed: 0,
                partyHp:    1, partyAttack:    1, partySpeed:  1,
                enemyHp: 1000, enemyAttack: 9999, enemySpeed: 20);

            var engine = new BattleEngine(setup);
            AutoResolve(engine);

            Assert.AreEqual(BattleState.PlayerLost, engine.State);
            Assert.IsNull(engine.Current);
        }

        /// <summary>종료 후 BattleOutcome이 일치하는 BattleResult를 반환한다.</summary>
        [Test]
        public void ToOutcome_AfterWin_ReturnsWinResult()
        {
            var setup = MakeSetup(seed: 0,
                partyHp: 1000, partyAttack: 9999, partySpeed: 20,
                enemyHp:    1, enemyAttack:    1, enemySpeed:  1);
            var engine = new BattleEngine(setup);
            AutoResolve(engine);

            var outcome = engine.ToOutcome();
            Assert.AreEqual(BattleResult.Win, outcome.Result);
        }

        [Test]
        public void ToOutcome_AfterLose_ReturnsLoseResult()
        {
            var setup = MakeSetup(seed: 0,
                partyHp:    1, partyAttack:    1, partySpeed:  1,
                enemyHp: 1000, enemyAttack: 9999, enemySpeed: 20);
            var engine = new BattleEngine(setup);
            AutoResolve(engine);

            var outcome = engine.ToOutcome();
            Assert.AreEqual(BattleResult.Lose, outcome.Result);
        }

        // ─────────────────────────────────────────────────────────────
        // TC-06: auto-resolve 트랜스크립트 재현 (결정론)
        // ─────────────────────────────────────────────────────────────

        /// <summary>동일 시드로 엔진을 2회 구동하면 HP·데미지 트랜스크립트가 완전히 일치한다.</summary>
        [Test]
        public void AutoResolve_SameSeed_IdenticalTranscript()
        {
            var makeSetup = () => new BattleSetup(
                party: new List<Combatant>
                {
                    new Combatant(id: 1, side: BattleSide.Player, maxHp: 150, attack: 25, speed: 12),
                    new Combatant(id: 3, side: BattleSide.Player, maxHp: 120, attack: 20, speed:  8),
                },
                enemies: new List<Combatant>
                {
                    new Combatant(id: 2, side: BattleSide.Enemy, maxHp: 80, attack: 15, speed: 10),
                    new Combatant(id: 4, side: BattleSide.Enemy, maxHp: 60, attack: 12, speed:  6),
                },
                seed: 9999);

            var transcript1 = CollectTranscript(makeSetup());
            var transcript2 = CollectTranscript(makeSetup());

            Assert.AreEqual(transcript1.Count, transcript2.Count,
                "트랜스크립트 길이가 다르다");

            for (int i = 0; i < transcript1.Count; i++)
            {
                Assert.AreEqual(transcript1[i], transcript2[i],
                    $"턴 {i}에서 데미지 불일치: {transcript1[i]} vs {transcript2[i]}");
            }
        }

        /// <summary>우세 스탯 팀이 승리한다.</summary>
        [Test]
        public void AutoResolve_StrongerSide_Wins()
        {
            var setup = MakeSetup(seed: 42,
                partyHp: 500, partyAttack: 80, partySpeed: 15,
                enemyHp:  50, enemyAttack: 10, enemySpeed:  5);

            var engine = new BattleEngine(setup);
            AutoResolve(engine);

            Assert.AreEqual(BattleState.PlayerWon, engine.State);
        }

        // ─────────────────────────────────────────────────────────────
        // 엔진 초기 상태
        // ─────────────────────────────────────────────────────────────

        /// <summary>전투 시작 직후 State == Ongoing.</summary>
        [Test]
        public void InitialState_IsOngoing()
        {
            var engine = new BattleEngine(MakeSetup());
            Assert.AreEqual(BattleState.Ongoing, engine.State);
        }

        /// <summary>전투 시작 직후 Current가 null이 아니다.</summary>
        [Test]
        public void InitialCurrent_NotNull()
        {
            var engine = new BattleEngine(MakeSetup());
            Assert.IsNotNull(engine.Current);
        }

        // ─────────────────────────────────────────────────────────────
        // 헬퍼 메서드
        // ─────────────────────────────────────────────────────────────

        /// <summary>엔진을 첫 번째 생존 적(또는 파티)을 타격하는 단순 정책으로 종료까지 구동한다.</summary>
        private static void AutoResolve(BattleEngine engine)
        {
            int guard = 0;
            while (engine.State == BattleState.Ongoing && engine.Current != null)
            {
                if (++guard > 10000) break; // 무한 루프 방지

                var actor  = engine.Current;
                int target = FindFirstLiveEnemy(engine, actor);
                if (target < 0) break;

                engine.SubmitAction(new BattleAction(
                    kind:       BattleActionKind.Attack,
                    actorId:    actor.Id,
                    targetId:   target,
                    skillPower: null));
            }
        }

        private static List<int> CollectTranscript(BattleSetup setup)
        {
            var engine    = new BattleEngine(setup);
            var damages   = new List<int>();
            int guard     = 0;

            while (engine.State == BattleState.Ongoing && engine.Current != null)
            {
                if (++guard > 10000) break;

                var actor  = engine.Current;
                int target = FindFirstLiveEnemy(engine, actor);
                if (target < 0) break;

                var result = engine.SubmitAction(new BattleAction(
                    kind:       BattleActionKind.Attack,
                    actorId:    actor.Id,
                    targetId:   target,
                    skillPower: null));

                damages.Add(result.DamageDealt);
            }
            return damages;
        }

        private static int FindFirstLiveEnemy(BattleEngine engine, Combatant actor)
        {
            foreach (var c in engine.AllCombatants)
            {
                if (c.Side != actor.Side && c.IsAlive)
                    return c.Id;
            }
            return -1;
        }
    }
}
