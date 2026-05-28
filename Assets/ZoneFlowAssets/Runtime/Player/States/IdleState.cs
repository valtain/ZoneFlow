namespace ZoneFlow.Player
{
    /// <summary>
    /// 이동 입력이 없는 정지 상태. 중력만 적용하고 입력이 감지되면 <see cref="MoveState"/>로 전환한다.
    /// </summary>
    public sealed class IdleState : IPlayerState
    {
        private readonly PlayerContext _ctx;

        /// <summary>컨텍스트를 주입받아 IdleState를 초기화한다.</summary>
        public IdleState(PlayerContext ctx) => _ctx = ctx;

        /// <inheritdoc/>
        public void Enter()
        {
            _ctx.Animator.SetIdle();
        }

        /// <inheritdoc/>
        public void Update(float deltaTime)
        {
            if (_ctx.Input.MoveInput.sqrMagnitude > 0.001f)
            {
                _ctx.StateMachine.ChangeState(new MoveState(_ctx));
                return;
            }

            _ctx.Movement.ApplyGravityOnly(deltaTime);
        }

        /// <inheritdoc/>
        public void Exit() { }
    }
}
