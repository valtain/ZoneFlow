using System;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>
    /// IInteractable 객체를 InteractableId로 조회하는 ScriptableObject 레지스트리.
    /// Zone 씬이 로드되어 있지 않아도 상호작용 목적지를 조회할 수 있다.
    ///
    /// 구조 원칙: 등록되는 IInteractable은 반드시 Zone GameObject의 자식이어야 한다.
    /// Zone 밖에 배치된 IInteractable은 Zone 생명주기 관리 대상이 아니므로 등록 불가.
    ///
    /// RegistryBaker가 Zone 씬을 스캔하여 자동 갱신한다.
    /// </summary>
    [CreateAssetMenu(fileName = "InteractableRegistry",
                     menuName  = "ZoneFlow/GamePlay/InteractableRegistry")]
    public class InteractableRegistry : ScriptableObject
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

        /// <summary>InteractableId로 항목을 조회한다.</summary>
        public bool TryGetEntry(string interactableId, out Entry entry)
        {
            if (!string.IsNullOrEmpty(interactableId) && _interactables != null)
            {
                foreach (var e in _interactables)
                {
                    if (e.InteractableId == interactableId)
                    {
                        entry = e;
                        return true;
                    }
                }
            }
            entry = default;
            return false;
        }

        /// <summary>InteractableId로 NavigationUri를 조회한다 (Portal 전용 편의 메서드).</summary>
        public bool TryGetNavigationUri(string interactableId, out string navigationUri)
        {
            if (TryGetEntry(interactableId, out var entry) &&
                !string.IsNullOrEmpty(entry.NavigationUri))
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
