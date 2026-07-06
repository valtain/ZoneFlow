using NUnit.Framework;
using ZoneFlow.Battle;

namespace ZoneFlow.Tests.Editor.Battle
{
    /// <summary>
    /// TC-03: DamageCalculator 결정론·시드 분산 검증.
    /// </summary>
    internal class DamageCalculatorTests
    {
        private static Combatant MakeAttacker(int attack = 20) =>
            new Combatant(id: 1, side: BattleSide.Player, maxHp: 100, attack: attack, speed: 10);

        private static Combatant MakeDefender(int maxHp = 100) =>
            new Combatant(id: 2, side: BattleSide.Enemy, maxHp: maxHp, attack: 10, speed: 5);

        // ─────────────────────────────────────────────────────────────
        // TC-03: 동일 시드 → 동일 데미지
        // ─────────────────────────────────────────────────────────────

        /// <summary>동일 시드·동일 (attacker/defender/power) 조합은 항상 동일한 데미지를 산출한다.</summary>
        [Test]
        public void Compute_SameSeed_SameDamage()
        {
            var attacker = MakeAttacker();
            var defender = MakeDefender();
            const int power = 10;
            const int seed  = 12345;

            int d1 = DamageCalculator.Compute(attacker, defender, power, new BattleRng(seed));
            int d2 = DamageCalculator.Compute(attacker, defender, power, new BattleRng(seed));

            Assert.AreEqual(d1, d2, "동일 시드에서 다른 데미지가 나왔다");
        }

        /// <summary>시드가 달라지면 데미지 값이 달라진다(분산 확인).</summary>
        [Test]
        public void Compute_DifferentSeed_ProducesDifferentDamage_OverMultipleTrials()
        {
            var attacker = MakeAttacker(attack: 50);
            var defender = MakeDefender();
            const int power = 20;

            // 여러 시드 쌍을 시도해 최소 한 번은 다른 값이 나와야 한다
            bool foundDifference = false;
            for (int i = 0; i < 20; i++)
            {
                int d1 = DamageCalculator.Compute(attacker, defender, power, new BattleRng(i));
                int d2 = DamageCalculator.Compute(attacker, defender, power, new BattleRng(i + 1000));
                if (d1 != d2) { foundDifference = true; break; }
            }
            Assert.IsTrue(foundDifference, "시드를 바꿔도 데미지가 전혀 달라지지 않았다");
        }

        // ─────────────────────────────────────────────────────────────
        // 기본 양수 보장
        // ─────────────────────────────────────────────────────────────

        /// <summary>데미지는 항상 1 이상이다(0 데미지 없음).</summary>
        [Test]
        public void Compute_AlwaysPositive()
        {
            var attacker = MakeAttacker(attack: 1);
            var defender = MakeDefender();
            const int power = 1;

            for (int seed = 0; seed < 50; seed++)
            {
                int dmg = DamageCalculator.Compute(attacker, defender, power, new BattleRng(seed));
                Assert.GreaterOrEqual(dmg, 1, $"시드 {seed}에서 데미지가 0 이하였다");
            }
        }

        /// <summary>공격력이 높을수록 데미지가 더 높다(같은 시드 기준).</summary>
        [Test]
        public void Compute_HigherAttack_ProducesHigherDamage()
        {
            var weakAttacker   = MakeAttacker(attack: 5);
            var strongAttacker = MakeAttacker(attack: 100);
            var defender = MakeDefender();
            const int power = 10;
            const int seed  = 42;

            int weak   = DamageCalculator.Compute(weakAttacker,   defender, power, new BattleRng(seed));
            int strong = DamageCalculator.Compute(strongAttacker, defender, power, new BattleRng(seed));

            Assert.Greater(strong, weak, "공격력이 높아도 데미지가 더 낮거나 같다");
        }
    }
}
