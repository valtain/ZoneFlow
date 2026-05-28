namespace ZoneFlow.Player
{
    /// <summary>
    /// 플레이어 상태 인터페이스. 모든 상태는 이 인터페이스를 구현한다.
    /// </summary>
    public interface IPlayerState
    {
        /// <summary>상태 진입 시 호출된다.</summary>
        void Enter();

        /// <summary>매 프레임 호출된다.</summary>
        void Update(float deltaTime);

        /// <summary>상태 이탈 시 호출된다.</summary>
        void Exit();
    }

    /// <summary>
    /// 플레이어 상태 머신. 상태 전환과 매 프레임 Update 전달을 담당한다.
    /// </summary>
    public sealed class PlayerStateMachine
    {
        /// <summary>현재 활성 상태.</summary>
        public IPlayerState CurrentState { get; private set; }

        /// <summary>
        /// 새로운 상태로 전환한다. Exit → Enter 순서를 보장한다.
        /// </summary>
        public void ChangeState(IPlayerState next)
        {
            CurrentState?.Exit();
            CurrentState = next;
            CurrentState?.Enter();
        }

        /// <summary>현재 상태의 Update를 호출한다.</summary>
        public void Update(float deltaTime) => CurrentState?.Update(deltaTime);
    }
}
