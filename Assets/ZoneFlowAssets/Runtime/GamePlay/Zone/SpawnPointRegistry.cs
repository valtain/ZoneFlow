using System;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>SpawnPointId로 소속 ZoneId를 역조회하는 레지스트리 ScriptableObject.</summary>
    [CreateAssetMenu(fileName = "SpawnPointRegistry", menuName = "ZoneFlow/GamePlay/SpawnPointRegistry")]
    public class SpawnPointRegistry : ScriptableObject
    {
        /// <summary>SpawnPointId와 소속 ZoneId를 연결하는 엔트리.</summary>
        [Serializable]
        public struct Entry
        {
            /// <summary>스폰 포인트 ID.</summary>
            public string spawnPointId;
            /// <summary>스폰 포인트가 속한 Zone의 ID. ZoneAssetRegistry의 ZoneId와 일치해야 한다.</summary>
            public string zoneId;
        }

        [SerializeField] private Entry[] _entries = default;

        /// <summary>SpawnPointId에 해당하는 ZoneId를 반환한다. 없으면 false를 반환한다.</summary>
        public bool TryGetZoneId(string spawnPointId, out string zoneId)
        {
            if (_entries != null)
            {
                foreach (var entry in _entries)
                {
                    if (entry.spawnPointId == spawnPointId)
                    {
                        zoneId = entry.zoneId;
                        return true;
                    }
                }
            }
            zoneId = null;
            return false;
        }
    }
}
