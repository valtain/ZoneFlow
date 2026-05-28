using Unity.Cinemachine;
using UnityEngine;

namespace ZoneFlow.Player
{
    /// <summary>
    /// 플레이어의 이동 방향을 향한 부드러운 회전을 전담한다.
    /// </summary>
    public sealed class PlayerRotation : MonoBehaviour
    {
        /// <summary>회전 댐핑 시간 (초). 0에 가까울수록 즉각 회전한다.</summary>
        [field: SerializeField] public float Damping { get; private set; } = 0.2f;

        /// <summary>
        /// 지정한 방향을 향해 Damper를 적용한 Slerp으로 부드럽게 회전한다.
        /// 뒷걸음질 시에는 호출 측에서 방향을 반전하여 전달한다.
        /// </summary>
        public void RotateTowards(Vector3 lookDirection, float deltaTime)
        {
            if (lookDirection.sqrMagnitude < 0.001f) return;

            var targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Damper.Damp(1f, Damping, deltaTime));
        }
    }
}
