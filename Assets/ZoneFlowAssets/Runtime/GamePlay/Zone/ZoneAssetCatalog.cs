using System.Collections.Generic;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>프로젝트의 모든 Zone 정의를 보관하는 카탈로그 ScriptableObject. CatalogBaker로 자동 갱신한다.</summary>
    [CreateAssetMenu(fileName = "ZoneAssetCatalog", menuName = "ZoneFlow/GamePlay/ZoneAssetCatalog")]
    public class ZoneAssetCatalog : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private ZoneAsset[] _zones = default;
        private Dictionary<string, ZoneAsset> _lookup = new();

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _lookup = new(_zones?.Length ?? 0);
            if (_zones == null) return;
            foreach (var z in _zones)
                if (z != null && !string.IsNullOrEmpty(z.ZoneId))
                    _lookup[z.ZoneId] = z;
        }

        /// <summary>ZoneId에 해당하는 ZoneAsset을 반환한다. 없으면 false를 반환한다.</summary>
        public bool TryGetZone(string zoneId, out ZoneAsset zone)
            => _lookup.TryGetValue(zoneId, out zone);
    }
}
