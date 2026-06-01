using UnityEngine;

namespace ZoneFlow
{
    /// <summary>플레이어가 존에 진입할 때 사용하는 스폰 위치 마커.</summary>
    public class SpawnPoint : MonoBehaviour
    {
        /// <summary>이 스폰 포인트의 고유 ID.</summary>
        [field: SerializeField] public string SpawnPointId { get; private set; } = default;

        /// <summary>존의 기본 스폰 포인트 여부.</summary>
        [field: SerializeField] public bool IsDefault { get; private set; } = default;

        /// <summary>스폰 위치 및 방향 Transform.</summary>
        public Transform SpawnTransform => _cachedTransform;

        private Transform _cachedTransform;

        private void Awake()
        {
            _cachedTransform = transform;
        }
    }
}
