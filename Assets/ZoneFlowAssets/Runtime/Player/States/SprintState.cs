namespace ZoneFlow.Player
{
    /// <summary>
    /// 스프린트 상태. Phase 2에서 구현 예정.
    /// </summary>
    public sealed class SprintState : IPlayerState
    {
        private readonly PlayerContext _ctx;

        /// <summary>컨텍스트를 주입받아 SprintState를 초기화한다.</summary>
        public SprintState(PlayerContext ctx) => _ctx = ctx;

        /// <inheritdoc/>
        public void Enter() { }

        /// <inheritdoc/>
        public void Update(float deltaTime) { }

        /// <inheritdoc/>
        public void Exit() { }
    }
}
