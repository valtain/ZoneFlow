using UnityEngine;
using UnityEngine.InputSystem;

namespace ZoneFlow.Player
{
    /// <summary>
    /// 플레이어 입력의 단일 창구.
    /// <see cref="InputActionReference"/>로 <c>InputSystem_Actions.inputactions</c> 에셋의 액션을
    /// 직접 참조한다. 생성된 C# 래퍼 클래스를 사용하지 않으므로 asmdef 스코프 제약이 없다.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private InputActionReference _moveAction;
        [SerializeField] private InputActionReference _sprintAction;

        /// <summary>WASD/스틱 이동 입력 (-1~1 벡터, 정규화 안 됨).</summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>스프린트 버튼 입력.</summary>
        public bool SprintInput { get; private set; }

        private void OnEnable()
        {
            _moveAction.action.performed   += OnMove;
            _moveAction.action.canceled    += OnMove;
            _sprintAction.action.performed += OnSprint;
            _sprintAction.action.canceled  += OnSprint;
            _moveAction.action.Enable();
            _sprintAction.action.Enable();
        }

        private void OnDisable()
        {
            _moveAction.action.performed   -= OnMove;
            _moveAction.action.canceled    -= OnMove;
            _sprintAction.action.performed -= OnSprint;
            _sprintAction.action.canceled  -= OnSprint;
            _moveAction.action.Disable();
            _sprintAction.action.Disable();
        }

        private void OnMove(InputAction.CallbackContext ctx)
            => MoveInput = ctx.ReadValue<Vector2>();

        private void OnSprint(InputAction.CallbackContext ctx)
            => SprintInput = ctx.ReadValueAsButton();
    }
}
