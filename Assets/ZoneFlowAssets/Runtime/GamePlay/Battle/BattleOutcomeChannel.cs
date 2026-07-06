namespace ZoneFlow.Battle
{
    /// <summary>
    /// 전투 종료 결과를 모드 간에 전달하는 순수 POCO 채널.
    /// ADR-0002: BattleMode가 <see cref="SetOutcome"/>으로 기록하고,
    /// 직전 모드의 <c>OnResumedAsync</c>가 <see cref="ConsumeOutcome"/>으로 1회 pull·소비한다.
    /// MonoBehaviour·GameObject 무참조.
    /// </summary>
    public sealed class BattleOutcomeChannel
    {
        private BattleOutcome _pending;

        /// <summary>아직 소비되지 않은 결과가 있으면 true.</summary>
        public bool HasOutcome => _pending != null;

        /// <summary>
        /// 전투 결과를 채널에 기록한다. 이전 미소비 결과는 덮어쓴다.
        /// </summary>
        /// <param name="outcome">기록할 전투 결과.</param>
        public void SetOutcome(BattleOutcome outcome)
        {
            _pending = outcome;
        }

        /// <summary>
        /// 채널에서 전투 결과를 1회 pull하고 내부 상태를 클리어한다.
        /// 결과가 없으면 null을 반환한다. 2회째 호출은 항상 null이다.
        /// </summary>
        /// <returns>저장된 <see cref="BattleOutcome"/>. 없으면 null.</returns>
        public BattleOutcome ConsumeOutcome()
        {
            var result = _pending;
            _pending   = null;
            return result;
        }
    }
}
