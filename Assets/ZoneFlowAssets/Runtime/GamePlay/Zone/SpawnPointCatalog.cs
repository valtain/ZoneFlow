using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>SpawnPointId로 소속 ZoneId를 역조회하는 카탈로그 ScriptableObject. CatalogBaker로 자동 갱신한다.</summary>
    [CreateAssetMenu(fileName = "SpawnPointCatalog", menuName = "ZoneFlow/GamePlay/SpawnPointCatalog")]
    public class SpawnPointCatalog : ScriptableObject, ISerializationCallbackReceiver
    {
        [Serializable]
        public struct Entry
        {
            /// <summary>스폰 포인트 ID.</summary>
            public string spawnPointId;
            /// <summary>스폰 포인트가 속한 Zone의 ID. ZoneAssetCatalog의 ZoneId와 일치해야 한다.</summary>
            public string zoneId;
        }

        [SerializeField] private Entry[] _entries = default;
        private Dictionary<string, string> _lookup = new();

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _lookup = new(_entries?.Length ?? 0);
            if (_entries == null) return;
            foreach (var e in _entries)
                if (!string.IsNullOrEmpty(e.spawnPointId))
                    _lookup[e.spawnPointId] = e.zoneId;
        }

        /// <summary>SpawnPointId에 해당하는 ZoneId를 반환한다. 없으면 false를 반환한다.</summary>
        public bool TryGetZoneId(string spawnPointId, out string zoneId)
            => _lookup.TryGetValue(spawnPointId, out zoneId);
    }
}
