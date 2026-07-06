using UnityEngine;

namespace ZoneFlow.Battle
{
    /// <summary>
    /// 순수 static 데미지 계산기.
    /// <para>
    /// 전역 <see cref="UnityEngine.Random"/> 이나 <see cref="System.Random"/> 을 사용하지 않으며,
    /// 주입된 <see cref="IBattleRng"/> 로만 분산을 생성한다(결정론).
    /// </para>
    /// </summary>
    public static class DamageCalculator
    {
        // 분산 비율: 기본 데미지의 ±VARIANCE_PERCENT% 범위
        private const int VariancePercent = 20;

        /// <summary>
        /// 공격자·방어자·스킬 파워·주입 RNG를 기반으로 최종 데미지를 계산한다.
        /// <para>
        /// 공식: base = attacker.Attack + power, 분산 = base * VariancePercent / 100,
        /// final = Clamp(base + rng(-variance, +variance+1), 1, int.MaxValue).
        /// </para>
        /// </summary>
        /// <param name="attacker">공격자 전투원.</param>
        /// <param name="defender">방어자 전투원(이번 슬라이스에서는 방어 스탯 미사용, 확장 지점).</param>
        /// <param name="power">스킬 파워. 기본공격이면 0을 넘긴다.</param>
        /// <param name="rng">결정론 난수 생성기. 호출할 때마다 상태가 전진한다.</param>
        /// <returns>1 이상의 최종 데미지.</returns>
        public static int Compute(Combatant attacker, Combatant defender, int power, IBattleRng rng)
        {
            Debug.Assert(attacker != null, "DamageCalculator.Compute: attacker가 null이다.");
            Debug.Assert(defender != null, "DamageCalculator.Compute: defender가 null이다.");
            Debug.Assert(rng != null, "DamageCalculator.Compute: rng가 null이다.");
            Debug.Assert(power >= 0, $"DamageCalculator.Compute: power({power})는 0 이상이어야 한다.");

            int baseValue = attacker.Attack + power;
            int variance  = Mathf.Max(1, baseValue * VariancePercent / 100);

            // rng.Next(-variance, variance + 1): 분산 범위 [-variance, +variance]
            int roll      = rng.Next(-variance, variance + 1);
            int final     = Mathf.Max(1, baseValue + roll);

            return final;
        }
    }
}
