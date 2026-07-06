namespace ZoneFlow.Battle
{
    /// <summary>전투에서 수행할 수 있는 액션 종류.</summary>
    public enum BattleActionKind
    {
        /// <summary>기본 공격. 스킬 파워 없이 공격자의 Attack 스탯으로만 계산.</summary>
        Attack,

        /// <summary>스킬 사용. skillPower가 데미지 계산에 추가된다.</summary>
        Skill,
    }

    /// <summary>
    /// 한 턴에 제출되는 전투 액션 값 객체.
    /// <para>
    /// <see cref="BattleEngine.SubmitAction"/> 에 전달하면 결정론적으로 처리된다.
    /// </para>
    /// </summary>
    public readonly struct BattleAction
    {
        /// <summary>액션 종류(기본공격·스킬).</summary>
        public BattleActionKind Kind { get; }

        /// <summary>행동자 전투원의 Id.</summary>
        public int ActorId { get; }

        /// <summary>대상 전투원의 Id.</summary>
        public int TargetId { get; }

        /// <summary>
        /// 스킬 파워. <see cref="BattleActionKind.Skill"/>일 때만 의미 있으며,
        /// null이면 0으로 처리한다.
        /// </summary>
        public int? SkillPower { get; }

        /// <summary>
        /// 전투 액션을 생성한다.
        /// </summary>
        /// <param name="kind">액션 종류.</param>
        /// <param name="actorId">행동자 Id.</param>
        /// <param name="targetId">대상 Id.</param>
        /// <param name="skillPower">스킬 파워(null이면 기본값 0).</param>
        public BattleAction(BattleActionKind kind, int actorId, int targetId, int? skillPower)
        {
            Kind       = kind;
            ActorId    = actorId;
            TargetId   = targetId;
            SkillPower = skillPower;
        }
    }
}
