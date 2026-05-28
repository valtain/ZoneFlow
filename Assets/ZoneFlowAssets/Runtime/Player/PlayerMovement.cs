using Unity.Cinemachine;
using UnityEngine;

namespace ZoneFlow.Player
{
    /// <summary>
    /// <see cref="CharacterController"/>를 통한 수평 이동과 중력 적용을 전담한다.
    /// velocity 기반으로 이동하며 Damping으로 가속/감속을 부드럽게 처리한다.
    /// </summary>
    public sealed class PlayerMovement : MonoBehaviour
    {
        /// <summary>최대 수평 이동 속도 (m/s).</summary>
        [field: SerializeField] public float MoveSpeed { get; private set; } = 5f;

        /// <summary>속도 변화에 적용되는 댐핑 시간 (초). 0에 가까울수록 즉각 반응한다.</summary>
        [field: SerializeField] public float Damping { get; private set; } = 0.3f;

        /// <summary>중력 가속도 크기 (양수, m/s²).</summary>
        [field: SerializeField] public float Gravity { get; private set; } = 10f;

        /// <summary>현재 프레임의 수평 속도 벡터. 애니메이션 파라미터 계산에 사용한다.</summary>
        public Vector3 CurrentVelocityXz { get; private set; }

        private CharacterController _cc;
        private float _verticalVelocity;

        // isGrounded 유지를 위한 최소 하향 속도. 0이면 다음 프레임 isGrounded = false 버그 발생.
        private const float GroundedVelocity = -2f;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            Debug.Assert(_cc != null, "[PlayerMovement] CharacterController가 없습니다.");
        }

        /// <summary>
        /// 목표 방향으로 Damping을 적용해 수평 이동하고 중력을 처리한다. MoveState에서 매 프레임 호출한다.
        /// </summary>
        public void Move(Vector3 worldDirection, float deltaTime)
        {
            MoveInternal(deltaTime, worldDirection * MoveSpeed);
        }

        /// <summary>
        /// 수평 이동 없이 중력만 적용하고 속도를 감속시킨다. IdleState에서 매 프레임 호출한다.
        /// </summary>
        public void ApplyGravityOnly(float deltaTime)
        {
            MoveInternal(deltaTime, Vector3.zero);
        }

        private void MoveInternal(float deltaTime, Vector3 desiredVelocity)
        {
            var damp = Damper.Damp(1f, Damping, deltaTime);
            CurrentVelocityXz = UpdateVelocityXz(CurrentVelocityXz, desiredVelocity, damp);

            UpdateVerticalVelocity(deltaTime);

            _cc.Move((CurrentVelocityXz + Vector3.up * _verticalVelocity) * deltaTime);
        }

        private Vector3 UpdateVelocityXz(Vector3 current, Vector3 desired, float damp)
        {
            // Slerp은 zero 벡터를 처리 못하므로, 정지·감속 상태이거나 큰 방향 전환이면 Lerp 사용
            var isSharpTurn = current.sqrMagnitude < 0.01f
                           || desired.sqrMagnitude  < 0.01f
                           || Vector3.Angle(current, desired) > 100f;

            return isSharpTurn
                ? Vector3.Lerp(current,  desired, damp)
                : Vector3.Slerp(current, desired, damp);
        }

        private void UpdateVerticalVelocity(float deltaTime)
        {
            if (_cc.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = GroundedVelocity;
            else
                _verticalVelocity -= Gravity * deltaTime;
        }
    }
}
