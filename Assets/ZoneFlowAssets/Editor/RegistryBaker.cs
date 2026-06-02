using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ZoneFlow.Editor
{
    /// <summary>Zone 씬과 패널 프리팹을 스캔하여 ZoneAssetRegistry, SpawnPointRegistry, PanelRegistry를 자동으로 갱신한다.</summary>
    public static class RegistryBaker
    {
        private const string ZoneRegistryPath   = "Assets/ZoneFlowAssets/Runtime/Data/ZoneAssetRegistry.asset";
        private const string SpawnRegistryPath  = "Assets/ZoneFlowAssets/Runtime/Data/SpawnPointRegistry.asset";
        private const string PanelRegistryPath  = "Assets/ZoneFlowAssets/Runtime/Data/PanelRegistry.asset";
        private const string InteractableRegistryPath = "Assets/ZoneFlowAssets/Runtime/Data/InteractableRegistry.asset";

        [MenuItem("ZoneFlow/Bake Registries")]
        public static void BakeAll()
        {
            BakeZoneAssetRegistry();
            BakeSpawnPointRegistry();
            BakeInteractableRegistry();
            BakePanelRegistry();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[RegistryBaker] 레지스트리 Bake 완료.");
        }

        // ──────────────────────────────────────────────────────────────────
        // ZoneAssetRegistry Bake
        // ──────────────────────────────────────────────────────────────────

        private static void BakeZoneAssetRegistry()
        {
            var registry = AssetDatabase.LoadAssetAtPath<ZoneAssetRegistry>(ZoneRegistryPath);
            if (registry == null)
            {
                Debug.LogError($"[RegistryBaker] ZoneAssetRegistry를 찾을 수 없습니다: {ZoneRegistryPath}");
                return;
            }

            var so = new SerializedObject(registry);
            var zonesArray = so.FindProperty("_zones");
            zonesArray.ClearArray();

            int idx = 0;
            foreach (var scenePath in GetZoneScenePaths())
            {
                var zoneId = ExtractZoneIdFromScene(scenePath);
                if (string.IsNullOrEmpty(zoneId)) continue;

                var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                zonesArray.InsertArrayElementAtIndex(idx);
                var elem = zonesArray.GetArrayElementAtIndex(idx);
                elem.FindPropertyRelative("<ZoneId>k__BackingField").stringValue   = zoneId;
                elem.FindPropertyRelative("<SceneName>k__BackingField").stringValue = sceneName;
                elem.FindPropertyRelative("<ZonePrefab>k__BackingField").objectReferenceValue = null;
                idx++;
            }

            so.ApplyModifiedProperties();
            Debug.Log($"[RegistryBaker] ZoneAssetRegistry: {idx}개 항목 등록.");
        }

        // ──────────────────────────────────────────────────────────────────
        // SpawnPointRegistry Bake
        // ──────────────────────────────────────────────────────────────────

        private static void BakeSpawnPointRegistry()
        {
            var registry = AssetDatabase.LoadAssetAtPath<SpawnPointRegistry>(SpawnRegistryPath);
            if (registry == null)
            {
                Debug.LogError($"[RegistryBaker] SpawnPointRegistry를 찾을 수 없습니다: {SpawnRegistryPath}");
                return;
            }

            var entries = new List<(string spawnId, string zoneId)>();

            foreach (var scenePath in GetZoneScenePaths())
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                try
                {
                    foreach (var root in scene.GetRootGameObjects())
                    {
                        var zone = root.GetComponent<Zone>();
                        if (zone == null) continue;

                        foreach (var sp in root.GetComponentsInChildren<SpawnPoint>(true))
                        {
                            if (!string.IsNullOrEmpty(sp.SpawnPointId))
                                entries.Add((sp.SpawnPointId, zone.ZoneId));
                        }
                    }
                }
                finally
                {
                    EditorSceneManager.CloseScene(scene, false);
                }
            }

            var so = new SerializedObject(registry);
            var entriesArray = so.FindProperty("_entries");
            entriesArray.ClearArray();

            for (int i = 0; i < entries.Count; i++)
            {
                entriesArray.InsertArrayElementAtIndex(i);
                var elem = entriesArray.GetArrayElementAtIndex(i);
                elem.FindPropertyRelative("spawnPointId").stringValue = entries[i].spawnId;
                elem.FindPropertyRelative("zoneId").stringValue        = entries[i].zoneId;
            }

            so.ApplyModifiedProperties();
            Debug.Log($"[RegistryBaker] SpawnPointRegistry: {entries.Count}개 항목 등록.");
        }

        // ──────────────────────────────────────────────────────────────────
        // InteractableRegistry Bake
        // Zone 자식에 배치된 IInteractable만 등록한다.
        // ──────────────────────────────────────────────────────────────────

        private static void BakeInteractableRegistry()
        {
            var registry = AssetDatabase.LoadAssetAtPath<InteractableRegistry>(InteractableRegistryPath);
            if (registry == null)
            {
                Debug.LogError($"[RegistryBaker] InteractableRegistry를 찾을 수 없습니다: {InteractableRegistryPath}");
                return;
            }

            var entries = new List<InteractableRegistry.Entry>();

            foreach (var scenePath in GetZoneScenePaths())
            {
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                try
                {
                    foreach (var root in scene.GetRootGameObjects())
                    {
                        var zone = root.GetComponent<Zone>();
                        if (zone == null)
                        {
                            // Zone 밖에 있는 IInteractable은 경고만 출력 (등록하지 않음)
                            foreach (var stray in root.GetComponentsInChildren<IInteractable>(true))
                            {
                                if (!string.IsNullOrEmpty(stray.InteractableId))
                                    Debug.LogWarning($"[RegistryBaker] '{stray.InteractableId}' 는 Zone 자식이 아닙니다. 등록 건너뜀. ({scenePath})");
                            }
                            continue;
                        }

                        foreach (var interactable in root.GetComponentsInChildren<IInteractable>(true))
                        {
                            if (string.IsNullOrEmpty(interactable.InteractableId)) continue;

                            var navUri = string.Empty;
                            if (interactable is Portal portal)
                                navUri = portal.NavigationUri;

                            entries.Add(new InteractableRegistry.Entry
                            {
                                InteractableId = interactable.InteractableId,
                                ZoneId         = zone.ZoneId,
                                NavigationUri  = navUri,
                            });
                        }
                    }
                }
                finally
                {
                    EditorSceneManager.CloseScene(scene, false);
                }
            }

            registry.SetEntries(entries.ToArray());
            Debug.Log($"[RegistryBaker] InteractableRegistry: {entries.Count}개 항목 등록.");
        }

        // ──────────────────────────────────────────────────────────────────
        // PanelRegistry Bake
        // ──────────────────────────────────────────────────────────────────

        private static void BakePanelRegistry()
        {
            var registry = AssetDatabase.LoadAssetAtPath<PanelRegistry>(PanelRegistryPath);
            if (registry == null)
            {
                Debug.LogError($"[RegistryBaker] PanelRegistry를 찾을 수 없습니다: {PanelRegistryPath}");
                return;
            }

            var entries = new List<PanelRegistry.Entry>();

            // Prefabs 폴더에서 MenuPanel 컴포넌트를 가진 프리팹 탐색
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/ZoneFlowAssets" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                var panel = prefab.GetComponent<MenuPanel>();
                if (panel == null) continue;

                entries.Add(new PanelRegistry.Entry
                {
                    PanelId = MenuPanel.PanelId,
                    Prefab  = prefab,
                });
            }

            registry.SetPanels(entries.ToArray());
            Debug.Log($"[RegistryBaker] PanelRegistry: {entries.Count}개 항목 등록.");
        }

        // ──────────────────────────────────────────────────────────────────
        // Helper
        // ──────────────────────────────────────────────────────────────────

        /// <summary>EditorBuildSettings에 등록된 씬 중 Zone 컴포넌트를 포함한 씬 경로만 반환한다.</summary>
        private static IEnumerable<string> GetZoneScenePaths()
        {
            foreach (var buildScene in EditorBuildSettings.scenes)
            {
                if (!buildScene.enabled) continue;
                var path = buildScene.path;
                if (ContainsZone(path)) yield return path;
            }
        }

        private static bool ContainsZone(string scenePath)
        {
            var deps = AssetDatabase.GetDependencies(scenePath, recursive: false);
            // 빠른 판단: Zone 스크립트 GUID 포함 여부
            const string zoneScriptGuid = "f8d6d15d06f748f4cb88a69cae869957";
            foreach (var dep in deps)
            {
                if (AssetDatabase.AssetPathToGUID(dep) == zoneScriptGuid)
                    return true;
            }
            // fallback: 씬을 열어서 확인
            return ContainsZoneViaOpen(scenePath);
        }

        private static bool ContainsZoneViaOpen(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            bool found = false;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.GetComponentInChildren<Zone>(true) != null) { found = true; break; }
            }
            EditorSceneManager.CloseScene(scene, false);
            return found;
        }

        private static string ExtractZoneIdFromScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            string zoneId = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                var zone = root.GetComponent<Zone>();
                if (zone != null) { zoneId = zone.ZoneId; break; }
            }
            EditorSceneManager.CloseScene(scene, false);
            return zoneId;
        }
    }
}
