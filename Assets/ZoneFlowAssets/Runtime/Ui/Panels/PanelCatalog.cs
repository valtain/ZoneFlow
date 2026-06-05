using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>PanelId로 UiPanel 프리팹을 조회하는 카탈로그 ScriptableObject. CatalogBaker로 자동 갱신한다.</summary>
    [CreateAssetMenu(fileName = "PanelCatalog", menuName = "ZoneFlow/UI/PanelCatalog")]
    public class PanelCatalog : ScriptableObject, ISerializationCallbackReceiver
    {
        [Serializable]
        public struct Entry
        {
            public string  PanelId;
            public UiPanel Prefab;
        }

        [SerializeField] private Entry[] _panels = Array.Empty<Entry>();
        private Dictionary<string, UiPanel> _lookup = new();

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _lookup = new(_panels?.Length ?? 0);
            if (_panels == null) return;
            foreach (var e in _panels)
                if (!string.IsNullOrEmpty(e.PanelId) && e.Prefab != null)
                    _lookup[e.PanelId] = e.Prefab;
        }

        /// <summary>PanelId에 해당하는 UiPanel 프리팹을 반환한다. 없으면 false를 반환한다.</summary>
        public bool TryGetPanel(string panelId, out UiPanel prefab)
            => _lookup.TryGetValue(panelId, out prefab);

#if UNITY_EDITOR
        public void SetPanels(Entry[] panels)
        {
            _panels = panels;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
