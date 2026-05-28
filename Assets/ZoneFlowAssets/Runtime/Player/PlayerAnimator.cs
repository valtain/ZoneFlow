using UnityEngine;

namespace ZoneFlow.Player
{
    /// <summary>
    /// Animator 파라미터 전달을 전담한다.
    /// <para>
    /// Unity-chan <c>UnityChanLocomotions.controller</c>의 파라미터 구조를 사용한다.
    /// Speed(Float) = localVelocity.z (전후), Direction(Float) = localVelocity.x (좌우).
    /// </para>
    /// </summary>
    public sealed class PlayerAnimator : MonoBehaviour
    {
        private static readonly int SpeedHash     = Animator.StringToHash("Speed");
        private static readonly int DirectionHash = Animator.StringToHash("Direction");

        // Lock-on Strafe 구현 시 아래 파라미터를 활성화한다.
        // private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        // private static readonly int MoveZHash = Animator.StringToHash("MoveZ");

        /// <summary>Blend Tree 전환에 적용되는 댐핑 시간 (초).</summary>
        [field: SerializeField] public float DampTime { get; private set; } = 0.1f;

        /// <summary>Animator 컴포넌트. Model 자식 GameObject의 Animator를 Inspector에서 할당한다.</summary>
        [field: SerializeField] public Animator Animator { get; private set; }

        /// <summary>
        /// 로컬 속도를 Animator 파라미터로 전달한다. DampTime으로 부드러운 블렌드 전환을 적용한다.
        /// </summary>
        public void SetMovement(Vector3 localVelocity, float deltaTime)
        {
            var speed      = localVelocity.magnitude;
            var normalized = speed > 0.1f ? localVelocity / speed : Vector3.zero;
            Animator.SetFloat(SpeedHash,     normalized.z, DampTime, deltaTime);
            Animator.SetFloat(DirectionHash, normalized.x, DampTime, deltaTime);
        }

        /// <summary>
        /// 파라미터를 댐핑 없이 즉시 0으로 설정한다. IdleState 진입 시 호출한다.
        /// </summary>
        public void SetIdle()
        {
            Animator.SetFloat(SpeedHash,     0f);
            Animator.SetFloat(DirectionHash, 0f);
        }
    }
}
