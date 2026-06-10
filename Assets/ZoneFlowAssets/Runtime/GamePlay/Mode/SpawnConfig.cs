using UnityEngine;

namespace ZoneFlow
{
    /// <summary>플레이어를 배치할 위치와 자세를 담는 값 타입.</summary>
    public readonly struct SpawnConfig
    {
        /// <summary>배치할 월드 좌표.</summary>
        public Vector3 Position { get; }

        /// <summary>배치할 회전값.</summary>
        public Quaternion Rotation { get; }

        public SpawnConfig(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        /// <summary>위치와 자세를 지정해 SpawnConfig를 생성한다.</summary>
        public static SpawnConfig At(Vector3 position, Quaternion rotation)
            => new SpawnConfig(position, rotation);
    }
}
