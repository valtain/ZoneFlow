namespace ZoneFlow.Battle
{
    /// <summary>
    /// 전투 전용 결정론 난수 생성기 인터페이스.
    /// 구현체는 주입된 시드만을 기반으로 동작해야 하며,
    /// <see cref="UnityEngine.Random"/> 이나 <see cref="System.Random"/> 에 의존하지 않는다.
    /// </summary>
    public interface IBattleRng
    {
        /// <summary>
        /// [minInclusive, maxExclusive) 범위의 정수를 결정론적으로 반환한다.
        /// </summary>
        /// <param name="minInclusive">최솟값(포함).</param>
        /// <param name="maxExclusive">최댓값(미포함). minInclusive보다 커야 한다.</param>
        int Next(int minInclusive, int maxExclusive);

        /// <summary>
        /// [0, maxExclusive) 범위의 정수를 결정론적으로 반환한다.
        /// </summary>
        /// <param name="maxExclusive">최댓값(미포함). 1 이상이어야 한다.</param>
        int Next(int maxExclusive);
    }
}
