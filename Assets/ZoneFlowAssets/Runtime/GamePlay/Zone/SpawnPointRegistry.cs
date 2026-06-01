using System;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>SpawnPointId로 소속 ZoneAsset을 조회하는 레지스트리 ScriptableObject.</summary>
    [CreateAssetMenu(fileName = "SpawnPointRegistry", menuName = "ZoneFlow/GamePlay/SpawnPointRegistry")]
    public class SpawnPointRegistry : ScriptableObject
    {
        /// <summary>SpawnPointId와 소속 ZoneAsset을 연결하는 엔트리.</summary>
        [Serializable]
        public struct Entry
        {
            /// <summary>스폰 포인트 ID.</summary>
            public string spawnPointId;
            /// <summary>스폰 포인트가 속한 존 에셋.</summary>
            public ZoneAsset zone;
        }

        [SerializeField] private Entry[] _entries = default;

        /// <summary>주어진 SpawnPointId에 해당하는 ZoneAsset을 반환한다. 없으면 false를 반환한다.</summary>
        public bool TryGetZone(string spawnPointId, out ZoneAsset zone)
        {
            if (_entries != null)
            {
                foreach (var entry in _entries)
                {
                    if (entry.spawnPointId == spawnPointId)
                    {
                        zone = entry.zone;
                        return true;
                    }
                }
            }
            zone = null;
            return false;
        }
    }
}
