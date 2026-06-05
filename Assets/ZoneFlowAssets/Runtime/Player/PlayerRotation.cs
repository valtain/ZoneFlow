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
