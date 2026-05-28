using Unity.Cinemachine;
using UnityEngine;

namespace ZoneFlow.Player
{
    /// <summary>
    /// 카메라 기준 입력 좌표계와 월드 이동 방향을 매 프레임 계산한다.
    /// <para>
    /// 카메라가 기울어져 있어도 플레이어 Up 벡터에 맞게 입력 좌표계를 보정하고,
    /// 카메라가 상/하 반구를 넘나드는 경우 <see cref="BlendTime"/> 동안 Slerp 블렌딩하여
    /// 급격한 이동 방향 전환을 방지한다.
    /// </para>
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class PlayerDirectionResolver : MonoBehaviour
    {
        /// <summary>입력 기준 좌표계 모드.</summary>
        public enum ForwardMode
        {
            /// <summary>카메라 forward 기준 (TPS 기본값).</summary>
            Camera,
            /// <summary>플레이어 forward 기준.</summary>
            Player,
            /// <summary>월드 forward 기준.</summary>
            World,
        }

        /// <summary>입력을 해석하는 기준 좌표계.</summary>
        [field: SerializeField] public ForwardMode InputForward { get; private set; } = ForwardMode.Camera;

        /// <summary>반구 전환 시 블렌딩 지속 시간 (초).</summary>
        [field: SerializeField] public float BlendTime { get; private set; } = 2f;

        /// <summary>현재 프레임의 입력 기준 좌표계 (카메라 + 반구 블렌딩 적용).</summary>
        public Quaternion InputFrame { get; private set; } = Quaternion.identity;

        /// <summary>카메라 기준으로 변환된 XZ 이동 방향 (Y=0, 정규화됨).</summary>
        public Vector3 MoveDirection { get; private set; }

        /// <summary>현재 입력이 뒷걸음질 방향이면 true. InputFrame 전방과 이동 방향의 내적이 음수일 때 판정된다.</summary>
        public bool IsBackward { get; private set; }

        private static readonly Quaternion UpsideDown = Quaternion.AngleAxis(180f, Vector3.right);
        private const float AxisValidThreshold = 0.001f;

        private bool    _inTopHemisphere  = true;
        private float   _timeInHemisphere = 100f;
        private Vector3 _lastRawInput;
        private Camera  _mainCamera;

        /// <summary>
        /// <see cref="PlayerController"/>가 Awake에서 <see cref="PlayerContext"/>의 카메라를 주입한다.
        /// </summary>
        public void Initialize(Camera mainCamera)
        {
            _mainCamera = mainCamera;
        }

        /// <summary>반구 블렌딩 상태를 초기화한다.</summary>
        public void Reset()
        {
            _inTopHemisphere  = true;
            _timeInHemisphere = 100f;
        }

        /// <summary>
        /// 입력 벡터를 InputFrame 기준 월드 방향으로 변환하여
        /// <see cref="InputFrame"/>과 <see cref="MoveDirection"/>을 갱신한다.
        /// </summary>
        public void Tick(Vector2 moveInput, float deltaTime)
        {
            if (moveInput.sqrMagnitude < 0.001f)
            {
                MoveDirection = Vector3.zero;
                IsBackward    = false;
                _lastRawInput = Vector3.zero;
                return;
            }

            var rawInput         = new Vector3(moveInput.x, 0f, moveInput.y);
            bool inputDirChanged = Vector3.Dot(rawInput, _lastRawInput) < 0.8f;
            _lastRawInput        = rawInput;

            var baseFrame = GetBaseFrame();
            InputFrame    = ComputeInputFrame(baseFrame, inputDirChanged, deltaTime);

            var direction = InputFrame * rawInput;
            if (direction.sqrMagnitude > 1f) direction.Normalize();
            direction.y   = 0f;
            MoveDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector3.zero;

            var forwardDot = Vector3.Dot(InputFrame * Vector3.forward, MoveDirection);
            if      (forwardDot < -0.1f) IsBackward = true;
            else if (forwardDot >  0.1f) IsBackward = false;
        }

        private Quaternion GetBaseFrame() => InputForward switch
        {
            ForwardMode.Camera => _mainCamera.transform.rotation,
            ForwardMode.Player => transform.rotation,
            _                  => Quaternion.identity,
        };

        private Quaternion ComputeInputFrame(Quaternion frame, bool inputDirChanged, float deltaTime)
        {
            var up   = frame * Vector3.up;
            var axis = Vector3.Cross(up, transform.up);

            if (axis.sqrMagnitude < AxisValidThreshold && Vector3.Dot(up, transform.up) >= 0f)
                return frame;

            var frameTop    = ComputeFrameTop(frame, up, axis);
            var frameBottom = ComputeFrameBottom(frame, up, axis);

            UpdateHemisphere(up, inputDirChanged, deltaTime);

            return GetBlendedFrame(frameTop, frameBottom);
        }

        private Quaternion ComputeFrameTop(Quaternion frame, Vector3 up, Vector3 axis)
        {
            var angle = UnityVectorExtensions.SignedAngle(up, transform.up, axis);
            return Quaternion.AngleAxis(angle, axis) * frame;
        }

        private Quaternion ComputeFrameBottom(Quaternion frame, Vector3 up, Vector3 axis)
        {
            var frameBottom = frame * UpsideDown;
            var axisBottom  = Vector3.Cross(frameBottom * Vector3.up, transform.up);

            if (axisBottom.sqrMagnitude <= AxisValidThreshold)
                return frameBottom;

            var angle = UnityVectorExtensions.SignedAngle(up, transform.up, axis);
            return Quaternion.AngleAxis(180f - angle, axisBottom) * frameBottom;
        }

        private void UpdateHemisphere(Vector3 up, bool inputDirChanged, float deltaTime)
        {
            _timeInHemisphere += deltaTime;

            bool inTop = Vector3.Dot(up, transform.up) >= 0f;
            if (inTop != _inTopHemisphere)
            {
                _inTopHemisphere  = inTop;
                _timeInHemisphere = Mathf.Max(0f, BlendTime - _timeInHemisphere);
            }

            if (inputDirChanged)
                _timeInHemisphere = BlendTime;
        }

        private Quaternion GetBlendedFrame(Quaternion frameTop, Quaternion frameBottom)
        {
            if (_timeInHemisphere >= BlendTime)
                return _inTopHemisphere ? frameTop : frameBottom;

            float t = _timeInHemisphere / BlendTime;

            return _inTopHemisphere
                ? Quaternion.Slerp(frameBottom, frameTop,    t)
                : Quaternion.Slerp(frameTop,    frameBottom, t);
        }
    }
}
