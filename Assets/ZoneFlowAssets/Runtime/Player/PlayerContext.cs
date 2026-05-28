using UnityEngine;

namespace ZoneFlow.Player
{
    /// <summary>
    /// 플레이어 시스템 전체에서 공유되는 컴포넌트 참조 컨테이너.
    /// <see cref="PlayerController"/>가 Awake에서 생성하며, State와 StateMachine이
    /// MonoBehaviour를 직접 참조하지 않고 이 컨텍스트를 통해서만 접근한다.
    /// </summary>
    public sealed class PlayerContext
    {
        /// <summary>플레이어 입력 핸들러.</summary>
        public PlayerInputHandler Input { get; }

        /// <summary>카메라 기준 월드 방향 계산기.</summary>
        public PlayerDirectionResolver Resolver { get; }

        /// <summary>CharacterController 이동 처리기.</summary>
        public PlayerMovement Movement { get; }

        /// <summary>회전 처리기.</summary>
        public PlayerRotation Rotation { get; }

        /// <summary>Animator 파라미터 전달기.</summary>
        public PlayerAnimator Animator { get; }

        /// <summary>플레이어 루트의 Transform.</summary>
        public Transform Transform { get; }

        /// <summary>씬의 메인 카메라. 방향 계산 등 공유 참조에 사용한다.</summary>
        public Camera MainCamera { get; }

        /// <summary>
        /// 상태 머신. <see cref="PlayerController.Awake"/>에서 생성 후 사후 주입된다.
        /// </summary>
        public PlayerStateMachine StateMachine { get; internal set; }

        /// <summary>
        /// 플레이어 컨텍스트를 생성하고 모든 컴포넌트 참조를 주입한다.
        /// </summary>
        public PlayerContext(
            PlayerInputHandler input,
            PlayerDirectionResolver resolver,
            PlayerMovement movement,
            PlayerRotation rotation,
            PlayerAnimator animator,
            Transform transform,
            Camera mainCamera)
        {
            Input      = input;
            Resolver   = resolver;
            Movement   = movement;
            Rotation   = rotation;
            Animator   = animator;
            Transform  = transform;
            MainCamera = mainCamera;
        }
    }
}
