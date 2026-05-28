using UnityEngine;
using UnityEngine.InputSystem;

namespace ZoneFlow.Player
{
    /// <summary>
    /// 플레이어 입력의 단일 창구.
    /// <see cref="InputSystem_Actions"/> 래퍼를 소유하고 활성화·비활성화를 관리한다.
    /// </summary>
    /// <remarks>
    /// 사전 조건: InputSystem_Actions.inputactions 파일이 이 asmdef 스코프 안에 위치해야 한다.
    /// Assets/ZoneFlowAssets/Runtime/Player/Input/ 경로로 이동 후 C# 클래스 생성을 활성화할 것.
    /// </remarks>
    [DefaultExecutionOrder(-1000)]
    public sealed class PlayerInputHandler : MonoBehaviour
    {
        private InputSystem_Actions _input;

        /// <summary>WASD/스틱 이동 입력 (-1~1 벡터, 정규화 안 됨).</summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>스프린트 버튼 입력.</summary>
        public bool SprintInput { get; private set; }

        private void Awake()
        {
            _input = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            _input.Player.Enable();
            _input.Player.Move.performed    += OnMove;
            _input.Player.Move.canceled     += OnMove;
            _input.Player.Sprint.performed  += OnSprint;
            _input.Player.Sprint.canceled   += OnSprint;
        }

        private void OnDisable()
        {
            _input.Player.Move.performed    -= OnMove;
            _input.Player.Move.canceled     -= OnMove;
            _input.Player.Sprint.performed  -= OnSprint;
            _input.Player.Sprint.canceled   -= OnSprint;
            _input.Player.Disable();
        }

        private void OnDestroy()
        {
            _input?.Dispose();
        }

        private void OnMove(InputAction.CallbackContext ctx)
            => MoveInput = ctx.ReadValue<Vector2>();

        private void OnSprint(InputAction.CallbackContext ctx)
            => SprintInput = ctx.ReadValueAsButton();
    }
}
