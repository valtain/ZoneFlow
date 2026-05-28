using UnityEngine;

namespace ZoneFlow.Player
{
    /// <summary>
    /// 이동 상태. 방향 이동·회전·애니메이션을 매 프레임 처리한다.
    /// 입력이 사라지면 <see cref="IdleState"/>로 전환한다.
    /// </summary>
    public sealed class MoveState : IPlayerState
    {
        private readonly PlayerContext _ctx;

        /// <summary>컨텍스트를 주입받아 MoveState를 초기화한다.</summary>
        public MoveState(PlayerContext ctx) => _ctx = ctx;

        /// <inheritdoc/>
        public void Enter() { }

        /// <inheritdoc/>
        public void Update(float deltaTime)
        {
            var worldDir = _ctx.Resolver.MoveDirection;

            if (worldDir.sqrMagnitude < 0.001f)
            {
                _ctx.StateMachine.ChangeState(new IdleState(_ctx));
                return;
            }

            _ctx.Movement.Move(worldDir, deltaTime);

            var lookDir = _ctx.Resolver.IsBackward ? -worldDir : worldDir;
            _ctx.Rotation.RotateTowards(lookDir, deltaTime);

            var localVelocity = Quaternion.Inverse(_ctx.Transform.rotation) * _ctx.Movement.CurrentVelocityXz;
            _ctx.Animator.SetMovement(localVelocity, deltaTime);
        }

        /// <inheritdoc/>
        public void Exit() { }
    }
}
