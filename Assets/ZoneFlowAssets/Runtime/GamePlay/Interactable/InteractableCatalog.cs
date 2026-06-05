using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>
    /// IInteractable 객체를 InteractableId로 조회하는 카탈로그 ScriptableObject.
    /// Zone 씬이 로드되어 있지 않아도 상호작용 목적지를 조회할 수 있다.
    ///
    /// 구조 원칙: 등록되는 IInteractable은 반드시 Zone GameObject의 자식이어야 한다.
    /// Zone 밖에 배치된 IInteractable은 Zone 생명주기 관리 대상이 아니므로 등록 불가.
    ///
    /// CatalogBaker가 Zone 씬을 스캔하여 자동 갱신한다.
    /// </summary>
    [CreateAssetMenu(fileName = "InteractableCatalog", menuName = "ZoneFlow/GamePlay/InteractableCatalog")]
    public class InteractableCatalog : ScriptableObject, ISerializationCallbackReceiver
    {
        [Serializable]
        public struct Entry
        {
            /// <summary>IInteractable.InteractableId 값.</summary>
            public string InteractableId;
            /// <summary>이 Interactable이 속한 Zone의 ZoneId.</summary>
            public string ZoneId;
            /// <summary>Portal 등 NavigationUri를 가진 경우 저장. 없으면 빈 문자열.</summary>
            public string NavigationUri;
        }

        [SerializeField] private Entry[] _interactables = Array.Empty<Entry>();
        private Dictionary<string, Entry> _lookup = new();

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _lookup = new(_interactables?.Length ?? 0);
            if (_interactables == null) return;
            foreach (var e in _interactables)
                if (!string.IsNullOrEmpty(e.InteractableId))
                    _lookup[e.InteractableId] = e;
        }

        /// <summary>InteractableId로 항목을 조회한다.</summary>
        public bool TryGetEntry(string interactableId, out Entry entry)
            => _lookup.TryGetValue(interactableId, out entry);

        /// <summary>InteractableId로 NavigationUri를 조회한다 (Portal 전용 편의 메서드).</summary>
        public bool TryGetNavigationUri(string interactableId, out string navigationUri)
        {
            if (TryGetEntry(interactableId, out var entry) && !string.IsNullOrEmpty(entry.NavigationUri))
            {
                navigationUri = entry.NavigationUri;
                return true;
            }
            navigationUri = null;
            return false;
        }

#if UNITY_EDITOR
        public void SetEntries(Entry[] entries)
        {
            _interactables = entries;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
