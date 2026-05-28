using UnityEngine;

namespace ZoneFlow.Player
{
    /// <summary>
    /// 플레이어 시스템의 진입점. Unity 생명주기를 소유하고 <see cref="PlayerContext"/>를 생성한다.
    /// 카메라 등 씬 공유 참조는 여기서 직렬화해 <see cref="PlayerContext"/>를 통해 배포한다.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerDirectionResolver))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerRotation))]
    [RequireComponent(typeof(PlayerAnimator))]
    public sealed class PlayerController : MonoBehaviour
    {
        /// <summary>씬의 메인 카메라. null이면 Awake에서 Camera.main으로 대체된다.</summary>
        [field: SerializeField] public Camera MainCamera { get; private set; }

        private PlayerContext      _ctx;
        private PlayerStateMachine _stateMachine;

        private void Awake()
        {
            if (MainCamera == null)
                MainCamera = Camera.main;

            Debug.Assert(MainCamera != null, "[PlayerController] MainCamera를 찾을 수 없습니다.");

            var inputHandler = GetComponent<PlayerInputHandler>();
            var resolver     = GetComponent<PlayerDirectionResolver>();
            var movement     = GetComponent<PlayerMovement>();
            var rotation     = GetComponent<PlayerRotation>();
            var animator     = GetComponent<PlayerAnimator>();

            Debug.Assert(inputHandler != null, "[PlayerController] PlayerInputHandler 없음");
            Debug.Assert(resolver     != null, "[PlayerController] PlayerDirectionResolver 없음");
            Debug.Assert(movement     != null, "[PlayerController] PlayerMovement 없음");
            Debug.Assert(rotation     != null, "[PlayerController] PlayerRotation 없음");
            Debug.Assert(animator     != null, "[PlayerController] PlayerAnimator 없음");

            _ctx = new PlayerContext(inputHandler, resolver, movement, rotation, animator, transform, MainCamera);

            resolver.Initialize(MainCamera);

            _stateMachine     = new PlayerStateMachine();
            _ctx.StateMachine = _stateMachine;

            _stateMachine.ChangeState(new IdleState(_ctx));
        }

        private void Update()
        {
            // Resolver를 먼저 갱신하여 StateMachine이 최신 방향을 읽을 수 있도록 한다.
            _ctx.Resolver.Tick(_ctx.Input.MoveInput, Time.deltaTime);
        }

        private void LateUpdate()
        {
            // Cinemachine이 LateUpdate에서 카메라를 갱신하므로 StateMachine은 그 이후에 실행한다.
            _stateMachine.Update(Time.deltaTime);
        }
    }
}
