using System.Collections.Generic;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>씬 또는 프리팹 기반의 게임플레이 구역을 나타내는 MonoBehaviour.</summary>
    public class Zone : MonoBehaviour
    {
        /// <summary>이 존의 고유 ID.</summary>
        [field: SerializeField] public string ZoneId { get; private set; } = default;

        /// <summary>기본 스폰 포인트. IsDefault==true인 첫 번째 SpawnPoint, 없으면 첫 번째.</summary>
        public SpawnPoint DefaultSpawnPoint { get; private set; }

        /// <summary>이 존에 포함된 모든 스폰 포인트 목록.</summary>
        public IReadOnlyList<SpawnPoint> SpawnPoints => _spawnPoints;

        /// <summary>이 존에 포함된 모든 상호작용 오브젝트 목록.</summary>
        public IReadOnlyList<IInteractable> Interactables => _interactables;

        private SpawnPoint[] _spawnPoints;
        private IInteractable[] _interactables;

        private void Awake()
        {
            _spawnPoints = GetComponentsInChildren<SpawnPoint>(true);
            _interactables = GetComponentsInChildren<IInteractable>(true);

            SpawnPoint defaultSp = null;
            foreach (var sp in _spawnPoints)
            {
                if (sp.IsDefault)
                {
                    defaultSp = sp;
                    break;
                }
            }

            if (defaultSp == null && _spawnPoints.Length > 0)
                defaultSp = _spawnPoints[0];

            Debug.Assert(defaultSp != null, $"[Zone:{ZoneId}] SpawnPoint가 하나도 없습니다.");
            DefaultSpawnPoint = defaultSp;
        }

        /// <summary>ID로 스폰 포인트를 반환한다. 없으면 DefaultSpawnPoint를 반환한다.</summary>
        public SpawnPoint GetSpawnPoint(string spawnPointId)
        {
            if (!string.IsNullOrEmpty(spawnPointId))
            {
                foreach (var sp in _spawnPoints)
                {
                    if (sp.SpawnPointId == spawnPointId)
                        return sp;
                }
            }
            return DefaultSpawnPoint;
        }
    }
}
