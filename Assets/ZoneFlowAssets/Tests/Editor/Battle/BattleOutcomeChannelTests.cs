using NUnit.Framework;
using ZoneFlow.Battle;

namespace ZoneFlow.Tests.Editor.Battle
{
    /// <summary>
    /// TC-07: BattleOutcomeChannel SetOutcome→ConsumeOutcome 1회 pull 후 클리어 검증.
    /// ADR-0002 계약: ConsumeOutcome은 1회만 값을 반환하고 이후 null.
    /// </summary>
    internal class BattleOutcomeChannelTests
    {
        // ─────────────────────────────────────────────────────────────
        // TC-07a: SetOutcome 후 HasOutcome == true
        // ─────────────────────────────────────────────────────────────

        /// <summary>SetOutcome 호출 후 HasOutcome이 true가 된다.</summary>
        [Test]
        public void HasOutcome_AfterSetOutcome_IsTrue()
        {
            var channel = new BattleOutcomeChannel();
            channel.SetOutcome(new BattleOutcome(BattleResult.Win));

            Assert.IsTrue(channel.HasOutcome);
        }

        // ─────────────────────────────────────────────────────────────
        // TC-07b: ConsumeOutcome 1회 pull → 값 반환
        // ─────────────────────────────────────────────────────────────

        /// <summary>ConsumeOutcome 첫 번째 호출은 저장된 BattleOutcome을 반환한다.</summary>
        [Test]
        public void ConsumeOutcome_FirstCall_ReturnsStoredOutcome()
        {
            var channel = new BattleOutcomeChannel();
            var outcome = new BattleOutcome(BattleResult.Win);
            channel.SetOutcome(outcome);

            var consumed = channel.ConsumeOutcome();

            Assert.IsNotNull(consumed);
            Assert.AreEqual(BattleResult.Win, consumed.Result);
        }

        // ─────────────────────────────────────────────────────────────
        // TC-07c: ConsumeOutcome 2회째 → null (클리어 확인)
        // ─────────────────────────────────────────────────────────────

        /// <summary>ConsumeOutcome 두 번째 호출은 null을 반환한다(1회 소비 후 클리어).</summary>
        [Test]
        public void ConsumeOutcome_SecondCall_ReturnsNull()
        {
            var channel = new BattleOutcomeChannel();
            channel.SetOutcome(new BattleOutcome(BattleResult.Lose));

            channel.ConsumeOutcome(); // 1회 소비
            var second = channel.ConsumeOutcome();

            Assert.IsNull(second);
        }

        // ─────────────────────────────────────────────────────────────
        // TC-07d: ConsumeOutcome 후 HasOutcome == false
        // ─────────────────────────────────────────────────────────────

        /// <summary>ConsumeOutcome 호출 후 HasOutcome이 false가 된다.</summary>
        [Test]
        public void HasOutcome_AfterConsumeOutcome_IsFalse()
        {
            var channel = new BattleOutcomeChannel();
            channel.SetOutcome(new BattleOutcome(BattleResult.Fled));

            channel.ConsumeOutcome();

            Assert.IsFalse(channel.HasOutcome);
        }

        // ─────────────────────────────────────────────────────────────
        // TC-07e: SetOutcome 없이 ConsumeOutcome → null
        // ─────────────────────────────────────────────────────────────

        /// <summary>SetOutcome 없이 ConsumeOutcome을 호출하면 null을 반환한다.</summary>
        [Test]
        public void ConsumeOutcome_WithoutSetOutcome_ReturnsNull()
        {
            var channel = new BattleOutcomeChannel();

            var result = channel.ConsumeOutcome();

            Assert.IsNull(result);
        }

        // ─────────────────────────────────────────────────────────────
        // TC-07f: 결과 종류 파라미터화 검증
        // ─────────────────────────────────────────────────────────────

        /// <summary>Win·Lose·Fled 각 결과가 그대로 ConsumeOutcome에서 반환된다.</summary>
        [TestCase(BattleResult.Win)]
        [TestCase(BattleResult.Lose)]
        [TestCase(BattleResult.Fled)]
        public void ConsumeOutcome_ReturnsCorrectResult(BattleResult expectedResult)
        {
            var channel = new BattleOutcomeChannel();
            channel.SetOutcome(new BattleOutcome(expectedResult));

            var consumed = channel.ConsumeOutcome();

            Assert.IsNotNull(consumed);
            Assert.AreEqual(expectedResult, consumed.Result);
        }
    }
}
