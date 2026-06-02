using System;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>
    /// ShellMode에서 사용할 패널 프리팹 목록을 보관하는 ScriptableObject 레지스트리.
    /// PanelLibrary가 이 에셋을 참조하여 런타임에 ShellPanelRegistry에 프리팹을 등록한다.
    /// RegistryBaker로 자동 갱신 가능.
    /// </summary>
    [CreateAssetMenu(fileName = "PanelRegistry", menuName = "ZoneFlow/UI/PanelRegistry")]
    public class PanelRegistry : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string     PanelId;
            public GameObject Prefab;
        }

        [SerializeField] private Entry[] _panels = Array.Empty<Entry>();

        public Entry[] Panels => _panels;

#if UNITY_EDITOR
        public void SetPanels(Entry[] panels)
        {
            _panels = panels;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
