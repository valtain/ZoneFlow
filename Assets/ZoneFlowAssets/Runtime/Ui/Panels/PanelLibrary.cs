using UnityEngine;

namespace ZoneFlow
{
    /// <summary>
    /// PanelRegistry 에셋을 읽어 ShellPanelRegistry에 패널 프리팹을 등록하는 서비스.
    /// CoreServices 씬에 배치하고 Inspector에서 PanelRegistry 에셋을 연결한다.
    /// </summary>
    public class PanelLibrary : MonoBehaviour
    {
        [field: SerializeField] public PanelRegistry Registry { get; private set; }

        private void Awake()
        {
            if (Registry == null) return;
            foreach (var entry in Registry.Panels)
                if (entry.Prefab != null)
                    ShellPanelRegistry.RegisterPrefab(entry.PanelId, entry.Prefab);
        }
    }
}
