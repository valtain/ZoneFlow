using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ZoneFlow.Editor
{
    /// <summary>Zone 씬과 패널 프리팹을 스캔하여 ZoneAssetCatalog, SpawnPointCatalog, InteractableCatalog, PanelCatalog를 자동으로 갱신한다.</summary>
    public static class CatalogBaker
    {
        private const string ZoneCatalogPath         = "Assets/ZoneFlowAssets/Runtime/Data/ZoneAssetCatalog.asset";
        private const string SpawnCatalogPath        = "Assets/ZoneFlowAssets/Runtime/Data/SpawnPointCatalog.asset";
        private const string PanelCatalogPath        = "Assets/ZoneFlowAssets/Runtime/Data/PanelCatalog.asset";
        private const string InteractableCatalogPath = "Assets/ZoneFlowAssets/Runtime/Data/InteractableCatalog.asset";

        [MenuItem("ZoneFlow/Bake Catalogs")]
        public static void BakeAll()
        {
            BakeZoneAssetCatalog();
            BakeSpawnPointCatalog();
            BakeInteractableCatalog();
            BakePanelCatalog();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CatalogBaker] 카탈로그 Bake 완료.");
        }

        // ──────────────────────────────────────────────────────────────────
        // ZoneAssetCatalog Bake
        // ──────────────────────────────────────────────────────────────────

        private static void BakeZoneAssetCatalog()
        {
            var catalog = LoadOrCreate<ZoneAssetCatalog>(ZoneCatalogPath);

            var so = new SerializedObject(catalog);
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
                elem.FindPropertyRelative("<ZoneId>k__BackingField").stringValue    = zoneId;
                elem.FindPropertyRelative("<SceneName>k__BackingField").stringValue = sceneName;
                elem.FindPropertyRelative("<ZonePrefab>k__BackingField").objectReferenceValue = null;
                idx++;
            }

            so.ApplyModifiedProperties();
            Debug.Log($"[CatalogBaker] ZoneAssetCatalog: {idx}개 항목 등록.");
        }

        // ──────────────────────────────────────────────────────────────────
        // SpawnPointCatalog Bake
        // ──────────────────────────────────────────────────────────────────

        private static void BakeSpawnPointCatalog()
        {
            var catalog = LoadOrCreate<SpawnPointCatalog>(SpawnCatalogPath);

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
                            if (!string.IsNullOrEmpty(sp.SpawnPointId))
                                entries.Add((sp.SpawnPointId, zone.ZoneId));
                    }
                }
                finally { EditorSceneManager.CloseScene(scene, false); }
            }

            var so = new SerializedObject(catalog);
            var entriesArray = so.FindProperty("_entries");
            entriesArray.ClearArray();

            for (int i = 0; i < entries.Count; i++)
            {
                entriesArray.InsertArrayElementAtIndex(i);
                var elem = entriesArray.GetArrayElementAtIndex(i);
                elem.FindPropertyRelative("spawnPointId").stringValue = entries[i].spawnId;
                elem.FindPropertyRelative("zoneId").stringValue       = entries[i].zoneId;
            }

            so.ApplyModifiedProperties();
            Debug.Log($"[CatalogBaker] SpawnPointCatalog: {entries.Count}개 항목 등록.");
        }

        // ──────────────────────────────────────────────────────────────────
        // InteractableCatalog Bake
        // Zone 자식에 배치된 IInteractable만 등록한다.
        // ──────────────────────────────────────────────────────────────────

        private static void BakeInteractableCatalog()
        {
            var catalog = LoadOrCreate<InteractableCatalog>(InteractableCatalogPath);

            var entries = new List<InteractableCatalog.Entry>();

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
                            foreach (var stray in root.GetComponentsInChildren<IInteractable>(true))
                                if (!string.IsNullOrEmpty(stray.InteractableId))
                                    Debug.LogWarning($"[CatalogBaker] '{stray.InteractableId}' 는 Zone 자식이 아닙니다. 등록 건너뜀. ({scenePath})");
                            continue;
                        }

                        foreach (var interactable in root.GetComponentsInChildren<IInteractable>(true))
                        {
                            if (string.IsNullOrEmpty(interactable.InteractableId)) continue;
                            var navUri = interactable is Portal portal ? portal.NavigationUri : string.Empty;
                            entries.Add(new InteractableCatalog.Entry
                            {
                                InteractableId = interactable.InteractableId,
                                ZoneId         = zone.ZoneId,
                                NavigationUri  = navUri,
                            });
                        }
                    }
                }
                finally { EditorSceneManager.CloseScene(scene, false); }
            }

            catalog.SetEntries(entries.ToArray());
            Debug.Log($"[CatalogBaker] InteractableCatalog: {entries.Count}개 항목 등록.");
        }

        // ──────────────────────────────────────────────────────────────────
        // PanelCatalog Bake
        // ──────────────────────────────────────────────────────────────────

        private static void BakePanelCatalog()
        {
            var catalog = LoadOrCreate<PanelCatalog>(PanelCatalogPath);

            var entries = new List<PanelCatalog.Entry>();

            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/ZoneFlowAssets" }))
            {
                var path   = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                if (prefab.TryGetComponent<MenuPanel>(out var menuPanel))
                    entries.Add(new PanelCatalog.Entry { PanelId = MenuPanel.PanelId, Prefab = menuPanel });
                else if (prefab.TryGetComponent<HudPanel>(out var hudPanel))
                    entries.Add(new PanelCatalog.Entry { PanelId = HudPanel.PanelId, Prefab = hudPanel });
            }

            catalog.SetPanels(entries.ToArray());
            Debug.Log($"[CatalogBaker] PanelCatalog: {entries.Count}개 항목 등록.");
        }

        // ──────────────────────────────────────────────────────────────────
        // Helper
        // ──────────────────────────────────────────────────────────────────

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;

            var dir = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            Debug.Log($"[CatalogBaker] {typeof(T).Name} 에셋 생성: {path}");
            return asset;
        }

        private static IEnumerable<string> GetZoneScenePaths()
        {
            foreach (var buildScene in EditorBuildSettings.scenes)
            {
                if (!buildScene.enabled) continue;
                if (ContainsZone(buildScene.path)) yield return buildScene.path;
            }
        }

        private static bool ContainsZone(string scenePath)
        {
            var deps = AssetDatabase.GetDependencies(scenePath, recursive: false);
            const string zoneScriptGuid = "f8d6d15d06f748f4cb88a69cae869957";
            foreach (var dep in deps)
                if (AssetDatabase.AssetPathToGUID(dep) == zoneScriptGuid) return true;
            return ContainsZoneViaOpen(scenePath);
        }

        private static bool ContainsZoneViaOpen(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            bool found = false;
            foreach (var root in scene.GetRootGameObjects())
                if (root.GetComponentInChildren<Zone>(true) != null) { found = true; break; }
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
