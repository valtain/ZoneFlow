using UnityEngine;

namespace ZoneFlow
{
    /// <summary>ZoneId(에셋명)로 ZoneAsset을 조회하는 레지스트리 ScriptableObject.</summary>
    [CreateAssetMenu(fileName = "ZoneAssetRegistry", menuName = "ZoneFlow/GamePlay/ZoneAssetRegistry")]
    public class ZoneAssetRegistry : ScriptableObject
    {
        [SerializeField] private ZoneAsset[] _zones = default;

        /// <summary>주어진 ZoneId에 해당하는 ZoneAsset을 반환한다. 없으면 false를 반환한다.</summary>
        public bool TryGetZone(string zoneId, out ZoneAsset zone)
        {
            if (_zones != null)
            {
                foreach (var z in _zones)
                {
                    if (z != null && z.ZoneId == zoneId)
                    {
                        zone = z;
                        return true;
                    }
                }
            }
            zone = null;
            return false;
        }
    }
}
