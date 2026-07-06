namespace ZoneFlow.Battle
{
    /// <summary>전투 종료 결과 종류.</summary>
    public enum BattleResult
    {
        /// <summary>플레이어 파티 승리.</summary>
        Win,

        /// <summary>플레이어 파티 패배.</summary>
        Lose,

        /// <summary>전투에서 도주.</summary>
        Fled,
    }

    /// <summary>
    /// 전투 종료 시 생성되는 결과 페이로드.
    /// <para>
    /// ADR-0002: <c>BattleService.SetOutcome(BattleOutcome)</c>을 통해 채널에 기록되며,
    /// 직전 모드의 <c>OnResumedAsync</c>에서 <c>ConsumeOutcome()</c>으로 1회 pull한다.
    /// Navigation URI에 싣지 않는다.
    /// </para>
    /// </summary>
    public sealed class BattleOutcome
    {
        /// <summary>전투 결과(승·패·도주).</summary>
        public BattleResult Result { get; }

        /// <summary>
        /// 전투 결과를 생성한다.
        /// </summary>
        /// <param name="result">결과 종류.</param>
        public BattleOutcome(BattleResult result)
        {
            Result = result;
        }
    }
}
