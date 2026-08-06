using System.Collections.Generic;
using NUnit.Framework;
using ZoneFlow.Battle;

namespace ZoneFlow.Tests.Editor.Battle
{
    /// <summary>
    /// TC-01/02: TurnOrder Speed 정렬·Id 타이브레이크·사망자 스킵 결정론 검증.
    /// </summary>
    internal class TurnOrderTests
    {
        // ─────────────────────────────────────────────────────────────
        // TC-01: Speed 내림차순 정렬
        // ─────────────────────────────────────────────────────────────

        /// <summary>Speed가 높은 전투원이 먼저 행동한다.</summary>
        [Test]
        public void TurnOrder_SpeedDescending_FastActorGoesFirst()
        {
            var slow  = new Combatant(id: 1, side: BattleSide.Enemy,  maxHp: 100, attack: 10, speed: 5);
            var fast  = new Combatant(id: 2, side: BattleSide.Player, maxHp: 100, attack: 10, speed: 15);
            var mid   = new Combatant(id: 3, side: BattleSide.Player, maxHp: 100, attack: 10, speed: 10);

            var order = new TurnOrder(new List<Combatant> { slow, fast, mid });

            Assert.AreEqual(fast.Id,  order.Next().Id);
            Assert.AreEqual(mid.Id,   order.Next().Id);
            Assert.AreEqual(slow.Id,  order.Next().Id);
        }

        // ─────────────────────────────────────────────────────────────
        // TC-01: Speed 동률 → Id 오름차순 타이브레이크
        // ─────────────────────────────────────────────────────────────

        /// <summary>Speed가 동일하면 Id 오름차순(작은 쪽이 먼저)으로 행동한다.</summary>
        [Test]
        public void TurnOrder_SpeedTie_SmallerIdGoesFirst()
        {
            var a = new Combatant(id: 10, side: BattleSide.Player, maxHp: 100, attack: 10, speed: 8);
            var b = new Combatant(id:  3, side: BattleSide.Enemy,  maxHp: 100, attack: 10, speed: 8);
            var c = new Combatant(id:  7, side: BattleSide.Player, maxHp: 100, attack: 10, speed: 8);

            var order = new TurnOrder(new List<Combatant> { a, b, c });

            Assert.AreEqual( 3, order.Next().Id);
            Assert.AreEqual( 7, order.Next().Id);
            Assert.AreEqual(10, order.Next().Id);
        }

        /// <summary>동일 전투원 집합·동일 입력이면 순서가 완전히 재현된다(결정론).</summary>
        [Test]
        public void TurnOrder_SameInput_FullyDeterministic()
        {
            System.Func<TurnOrder> makeOrder = () =>
            {
                var list = new List<Combatant>
                {
                    new Combatant(id: 5, side: BattleSide.Enemy,  maxHp: 100, attack: 10, speed: 12),
                    new Combatant(id: 1, side: BattleSide.Player, maxHp: 100, attack: 10, speed: 12),
                    new Combatant(id: 9, side: BattleSide.Enemy,  maxHp: 100, attack: 10, speed:  7),
                };
                return new TurnOrder(list);
            };

            var first  = makeOrder();
            var second = makeOrder();

            // 두 라운드 순서 일치 확인
            for (int i = 0; i < 6; i++)
            {
                Assert.AreEqual(first.Next().Id, second.Next().Id,
                    $"인덱스 {i}에서 순서 불일치");
            }
        }

        // ─────────────────────────────────────────────────────────────
        // TC-02: 사망자 스킵
        // ─────────────────────────────────────────────────────────────

        /// <summary>HP가 0인 전투원은 Next()에서 반환되지 않는다.</summary>
        [Test]
        public void TurnOrder_DeadCombatant_IsSkipped()
        {
            var alive = new Combatant(id: 1, side: BattleSide.Player, maxHp: 100, attack: 10, speed: 10);
            var dead  = new Combatant(id: 2, side: BattleSide.Enemy,  maxHp: 100, attack: 10, speed:  5);
            dead.ApplyDamage(dead.MaxHp); // HP → 0

            var order = new TurnOrder(new List<Combatant> { alive, dead });

            // 여러 번 호출해도 살아있는 전투원만 나온다
            for (int i = 0; i < 4; i++)
            {
                var next = order.Next();
                Assert.AreEqual(alive.Id, next.Id, "죽은 전투원이 순서에 포함됨");
            }
        }

        /// <summary>라운드 도중 사망하면 이후 그 전투원의 턴이 오지 않는다.</summary>
        [Test]
        public void TurnOrder_DiesInRound_SkippedInSubsequentTurns()
        {
            var a = new Combatant(id: 1, side: BattleSide.Player, maxHp: 100, attack: 10, speed: 20);
            var b = new Combatant(id: 2, side: BattleSide.Enemy,  maxHp: 100, attack: 10, speed: 10);
            var c = new Combatant(id: 3, side: BattleSide.Enemy,  maxHp: 100, attack: 10, speed:  5);

            var order = new TurnOrder(new List<Combatant> { a, b, c });

            // 첫 순번: a
            Assert.AreEqual(a.Id, order.Next().Id);
            // b가 중간에 사망
            b.ApplyDamage(b.MaxHp);
            // c는 여전히 살아있으므로 두 번째 순번
            Assert.AreEqual(c.Id, order.Next().Id);
            // 다음 라운드: a → c (b 스킵)
            Assert.AreEqual(a.Id, order.Next().Id);
            Assert.AreEqual(c.Id, order.Next().Id);
        }

        /// <summary>모든 전투원이 사망하면 Next()는 null을 반환한다.</summary>
        [Test]
        public void TurnOrder_AllDead_ReturnsNull()
        {
            var a = new Combatant(id: 1, side: BattleSide.Player, maxHp: 50, attack: 10, speed: 10);
            a.ApplyDamage(a.MaxHp);

            var order = new TurnOrder(new List<Combatant> { a });
            Assert.IsNull(order.Next());
        }
    }
}
