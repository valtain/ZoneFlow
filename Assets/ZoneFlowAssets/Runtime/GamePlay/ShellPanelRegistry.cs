using System.Collections.Generic;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>
    /// ShellMode가 활성화될 때 표시할 패널을 PanelId로 관리하는 정적 레지스트리.
    /// PanelLibrary(MonoBehaviour)가 프리팹을 등록하면, Show() 호출 시 처음에만 Instantiate한다.
    /// </summary>
    public static class ShellPanelRegistry
    {
        private static readonly Dictionary<string, GameObject> _prefabs   = new();
        private static readonly Dictionary<string, GameObject> _instances = new();

        // ── 프리팹 등록 (PanelLibrary.Awake에서 호출) ────────────────────

        /// <summary>PanelId에 대응하는 프리팹을 등록한다.</summary>
        public static void RegisterPrefab(string panelId, GameObject prefab)
        {
            if (!string.IsNullOrEmpty(panelId) && prefab != null)
                _prefabs[panelId] = prefab;
        }

        // ── 인스턴스 직접 등록 (패널이 씬에 이미 있는 경우) ──────────────

        /// <summary>이미 인스턴스화된 패널을 등록한다. 보통 패널의 OnEnable에서 호출한다.</summary>
        public static void RegisterInstance(string panelId, GameObject instance)
        {
            if (!string.IsNullOrEmpty(panelId))
                _instances[panelId] = instance;
        }

        /// <summary>인스턴스 등록을 해제한다. 보통 패널의 OnDisable에서 호출한다.</summary>
        public static void UnregisterInstance(string panelId)
        {
            if (!string.IsNullOrEmpty(panelId))
                _instances.Remove(panelId);
        }

        // ── Show / Hide ────────────────────────────────────────────────────

        /// <summary>패널을 표시한다. 인스턴스가 없으면 등록된 프리팹으로 생성한다.</summary>
        public static void Show(string panelId)
        {
            if (string.IsNullOrEmpty(panelId)) return;

            var instance = GetOrCreate(panelId);
            if (instance != null)
                instance.SetActive(true);
        }

        /// <summary>패널을 숨긴다.</summary>
        public static void Hide(string panelId)
        {
            if (string.IsNullOrEmpty(panelId)) return;
            if (_instances.TryGetValue(panelId, out var instance) && instance != null)
                instance.SetActive(false);
        }

        // ── 내부 ──────────────────────────────────────────────────────────

        private static GameObject GetOrCreate(string panelId)
        {
            if (_instances.TryGetValue(panelId, out var existing) && existing != null)
                return existing;

            // 프리팹에서 최초 인스턴스화
            if (!_prefabs.TryGetValue(panelId, out var prefab) || prefab == null)
            {
                Debug.LogWarning($"[ShellPanelRegistry] '{panelId}' 패널 프리팹이 등록되지 않았습니다.");
                return null;
            }

            var go = Object.Instantiate(prefab);
            Object.DontDestroyOnLoad(go);
            // MenuPanel.OnEnable → RegisterInstance 자동 호출
            return go;
        }
    }
}
