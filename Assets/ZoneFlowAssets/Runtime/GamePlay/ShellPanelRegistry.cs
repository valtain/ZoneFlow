using System.Collections.Generic;

namespace ZoneFlow
{
    /// <summary>
    /// ShellMode가 push할 UiPanel 프리팹을 PanelId로 조회하는 정적 레지스트리.
    /// PanelLibrary.Awake에서 프리팹을 등록하고, ShellMode.OnModeInAsync에서 조회한다.
    /// </summary>
    public static class ShellPanelRegistry
    {
        private static readonly Dictionary<string, UiPanel> _prefabs = new();

        /// <summary>PanelId에 대응하는 UiPanel 프리팹을 등록한다.</summary>
        public static void RegisterPrefab(string panelId, UiPanel prefab)
        {
            if (!string.IsNullOrEmpty(panelId) && prefab != null)
                _prefabs[panelId] = prefab;
        }

        /// <summary>PanelId에 대응하는 UiPanel 프리팹을 조회한다.</summary>
        public static bool TryGetPrefab(string panelId, out UiPanel prefab)
        {
            prefab = null;
            return !string.IsNullOrEmpty(panelId) && _prefabs.TryGetValue(panelId, out prefab) && prefab != null;
        }
    }
}
