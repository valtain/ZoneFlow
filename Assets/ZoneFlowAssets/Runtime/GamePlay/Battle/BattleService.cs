using UnityEngine;

namespace ZoneFlow.Battle
{
    /// <summary>
    /// 전투 결과 채널과 전투 엔진 팩토리를 제공하는 CoreServices 상주 서비스.
    /// ADR-0002: <see cref="SetOutcome"/>/<see cref="ConsumeOutcome"/>을 내부 <see cref="BattleOutcomeChannel"/>에 위임한다.
    /// 코드가 GameObject를 생성하지 않는다 — CoreServices 씬에 배치하여 등록한다.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class BattleService : MonoService<BattleService>
    {
        /// <summary>
        /// 슬라이스 임시 기본 조우 설정.
        /// BattleMode가 별도 조우를 받지 못할 때 이 값으로 전투를 시작한다.
        /// </summary>
        [field: SerializeField]
        public BattleEncounterAsset DefaultEncounter { get; private set; } = default;

        private readonly BattleOutcomeChannel _channel = new BattleOutcomeChannel();

        /// <summary>아직 소비되지 않은 전투 결과가 있으면 true.</summary>
        public bool HasOutcome => _channel.HasOutcome;

        /// <summary>
        /// 전투 결과를 채널에 기록한다.
        /// BattleMode가 전투 종료 직후 호출한다.
        /// </summary>
        /// <param name="outcome">기록할 전투 결과.</param>
        public void SetOutcome(BattleOutcome outcome)
        {
            Debug.Assert(outcome != null, "[BattleService] SetOutcome: outcome이 null이다.");
            _channel.SetOutcome(outcome);
        }

        /// <summary>
        /// 채널에서 전투 결과를 1회 pull하고 클리어한다.
        /// 직전 모드의 <c>OnResumedAsync</c>에서 호출한다(ADR-0002).
        /// 결과가 없거나 이미 소비된 경우 null을 반환한다.
        /// </summary>
        /// <returns>저장된 <see cref="BattleOutcome"/>. 없으면 null.</returns>
        public BattleOutcome ConsumeOutcome()
        {
            return _channel.ConsumeOutcome();
        }

        /// <summary>
        /// <see cref="BattleSetup"/>을 받아 새 <see cref="BattleEngine"/>을 생성하고 반환한다.
        /// 엔진 생명주기는 호출자가 관리한다.
        /// </summary>
        /// <param name="setup">전투 초기화 데이터.</param>
        /// <returns>초기화된 전투 엔진.</returns>
        public BattleEngine StartBattle(BattleSetup setup)
        {
            Debug.Assert(setup != null, "[BattleService] StartBattle: setup이 null이다.");
            return new BattleEngine(setup);
        }
    }
}
