using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ZoneFlow.Editor
{
    /// <summary>demo-mvp(Village / Dungeon) 씬에 Zone·포털·스폰포인트를 추가하고
    /// MenuPanel·HUD 프리팹을 만드는 에디터 도구.</summary>
    public static class SceneSetupTool
    {
        private const string PrefabDir = "Assets/ZoneFlowAssets/Prefabs";

        // ──────────────────────────────────────────────────────────────────
        // Menu Item
        // ──────────────────────────────────────────────────────────────────

        [MenuItem("ZoneFlow/Setup/Create MenuPanel Prefab")]
        public static void CreateMenuPrefab() => BuildMenuPanelPrefab();

        [MenuItem("ZoneFlow/Setup/Create ExplorationHudPanel Prefab")]
        public static void CreateHudPrefab() => BuildHudPanelPrefab();

        [MenuItem("ZoneFlow/Setup/Create StoryHudPanel Prefab")]
        public static void CreateStoryHudPrefab() => BuildStoryHudPanelPrefab();

        /// <summary>Demo MVP: village (Village 씬) + dungeon (Dungeon 씬) zones with cross-portal navigation.</summary>
        [MenuItem("ZoneFlow/Setup/Setup Demo MVP")]
        public static void SetupDemoMvp()
        {
            AddZoneToScene("Village", zoneId: "village", offset: new Vector3(0, 0, 80),
                portalId: "portal_to_dungeon", portalTargetUri: "gameplay://exploration/dungeon_0?id=dungeon_0_entrance");
            AddZoneToScene("Dungeon", zoneId: "dungeon_0", offset: new Vector3(0, 0, 80),
                portalId: "portal_to_village", portalTargetUri: "gameplay://exploration/village?id=village_entrance");
        }

        // ──────────────────────────────────────────────────────────────────
        // Interaction: 기존 Portal 마이그레이션
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// 현재 열려 있는 모든 씬의 Portal을 interaction-prompt 방식으로 정리한다.
        /// 옛 월드 라벨(Label 자식) 제거 · 콜라이더 Trigger 보장 · DisplayLabel이 비면 PortalId로 시드한다.
        /// 변경된 씬은 dirty로 표시만 하므로 직접 저장(Ctrl+S)해야 한다.
        /// </summary>
        [MenuItem("ZoneFlow/Interaction/Migrate Portals (Open Scenes)")]
        public static void MigratePortalsInOpenScenes()
        {
            int processed = 0, labelsRemoved = 0, labelsSeeded = 0, triggersFixed = 0;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var portal in root.GetComponentsInChildren<Portal>(includeInactive: true))
                    {
                        processed++;

                        // 1) 옛 월드 라벨 제거 (스크린 프롬프트로 대체됨)
                        var label = portal.transform.Find("Label");
                        if (label != null)
                        {
                            Undo.DestroyObjectImmediate(label.gameObject);
                            labelsRemoved++;
                        }

                        // 2) 콜라이더 Trigger 보장 (디텍터 OverlapSphere 감지 조건)
                        if (portal.TryGetComponent<Collider>(out var col) && !col.isTrigger)
                        {
                            Undo.RecordObject(col, "Migrate Portal Collider");
                            col.isTrigger = true;
                            triggersFixed++;
                        }

                        // 3) DisplayLabel이 비면 PortalId로 시드 (이후 친화 명칭으로 수동 편집)
                        if (string.IsNullOrWhiteSpace(portal.DisplayLabel))
                        {
                            var so = new SerializedObject(portal);
                            so.FindProperty("<DisplayLabel>k__BackingField").stringValue = portal.PortalId;
                            so.ApplyModifiedProperties();
                            labelsSeeded++;
                        }

                        EditorUtility.SetDirty(portal);
                    }
                }

                EditorSceneManager.MarkSceneDirty(scene);
            }

            if (processed == 0)
                Debug.LogWarning("[SceneSetupTool] 열린 씬에서 Portal을 찾지 못했습니다.");
            else
                Debug.Log($"[SceneSetupTool] Portal 마이그레이션 완료: {processed}개 처리 " +
                    $"(월드라벨 {labelsRemoved} 제거, Trigger {triggersFixed} 보정, DisplayLabel {labelsSeeded} 시드). " +
                    "씬을 저장(Ctrl+S)하세요.");
        }

        // ──────────────────────────────────────────────────────────────────
        // Helper: Primitives
        // ──────────────────────────────────────────────────────────────────

        private static GameObject CreatePortalObject(string portalId, string navUri, Vector3 pos,
            string displayLabel = null)
        {
            // 시각적 표시용 Cylinder
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = $"Portal_{portalId}";
            visual.transform.position = pos;
            visual.transform.localScale = new Vector3(2, 3, 2);
            SetColor(visual, new Color(0.2f, 0.6f, 1.0f, 0.7f));

            // 기존 Capsule Collider를 Trigger로 설정
            var col = visual.GetComponent<CapsuleCollider>();
            if (col != null) col.isTrigger = true;

            // Portal 컴포넌트
            var portal = visual.AddComponent<Portal>();
            var so = new SerializedObject(portal);
            so.FindProperty("<NavigationUri>k__BackingField").stringValue = navUri;
            so.FindProperty("<PortalId>k__BackingField").stringValue = portalId;
            so.FindProperty("<DisplayLabel>k__BackingField").stringValue =
                string.IsNullOrEmpty(displayLabel) ? portalId : displayLabel;
            so.ApplyModifiedProperties();

            // 월드 라벨(TextMeshPro)은 더 이상 생성하지 않는다.
            // interaction-prompt의 스크린 공간 프롬프트(InteractionPromptPanel)가 DisplayLabel을 표시한다. (#61)

            return visual;
        }

        private static GameObject CreateSpawnPointMarker(string spawnId, bool isDefault, Vector3 pos, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = $"SpawnPoint_{spawnId}";
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.4f;
            SetColor(go, color);

            // Collider 제거 (걸리지 않도록)
            var col = go.GetComponent<SphereCollider>();
            if (col != null) Object.DestroyImmediate(col);

            var sp = go.AddComponent<SpawnPoint>();
            var so = new SerializedObject(sp);
            so.FindProperty("<SpawnPointId>k__BackingField").stringValue = spawnId;
            so.FindProperty("<IsDefault>k__BackingField").boolValue = isDefault;
            so.ApplyModifiedProperties();

            return go;
        }

        private static void SetColor(GameObject go, Color color)
        {
            var mr = go.GetComponent<MeshRenderer>();
            if (mr == null) return;

            // URP 프로젝트: Universal Render Pipeline/Lit 셰이더 사용 (Standard는 핑크로 표시됨)
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null) shader = Shader.Find("Standard");

            var mat = new Material(shader) { color = color };

            // URP Lit: _BaseColor 프로퍼티도 함께 설정
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);

            mr.sharedMaterial = mat;
        }

        // ──────────────────────────────────────────────────────────────────
        // Add Zone to existing scene
        // ──────────────────────────────────────────────────────────────────

        private static void AddZoneToScene(string sceneName, string zoneId, Vector3 offset,
            string portalId, string portalTargetUri)
        {
            var scenePath = $"Assets/ZoneFlowAssets/Scenes/{sceneName}.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            EditorSceneManager.SetActiveScene(scene);

            // 동일 ZoneId가 이미 있으면 스킵
            foreach (var root in scene.GetRootGameObjects())
            {
                var existing = root.GetComponent<Zone>();
                if (existing != null && existing.ZoneId == zoneId)
                {
                    Debug.Log($"[SceneSetupTool] ZoneId '{zoneId}'가 이미 존재합니다. 스킵.");
                    EditorSceneManager.CloseScene(scene, false);
                    return;
                }
            }

            // ── Zone Root ────────────────────────────────────────────────
            var zoneRoot = new GameObject($"Zone_{zoneId}");
            var zone = zoneRoot.AddComponent<Zone>();
            var so = new SerializedObject(zone);
            so.FindProperty("<ZoneId>k__BackingField").stringValue = zoneId;
            so.ApplyModifiedProperties();

            // ── Ground (Zone 루트 하위 — Zone Disable 시 함께 숨겨짐) ────
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = offset;
            ground.transform.localScale = new Vector3(3, 1, 3);
            SetColor(ground, new Color(0.3f, 0.25f, 0.35f));
            ground.transform.SetParent(zoneRoot.transform);

            // ── Obstacles ────────────────────────────────────────────────
            CreateObstacleAt($"Obs_A", offset + new Vector3(-4, 1, -3), new Vector3(2, 2, 2),   new Color(0.5f, 0.3f, 0.2f)).transform.SetParent(zoneRoot.transform);
            CreateObstacleAt($"Obs_B", offset + new Vector3( 4, 1,  4), new Vector3(1.5f, 3, 1.5f), new Color(0.4f, 0.2f, 0.4f)).transform.SetParent(zoneRoot.transform);

            // ── Default SpawnPoint ────────────────────────────────────────
            var spawnDefault = CreateSpawnPointMarker($"{zoneId}_default", isDefault: true,
                pos: offset + new Vector3(0, 0.1f, -8), color: new Color(1f, 0.6f, 0f));
            spawnDefault.transform.SetParent(zoneRoot.transform);

            // ── Entrance SpawnPoint (포털 도착지) ────────────────────────
            var spawnEntrance = CreateSpawnPointMarker($"{zoneId}_entrance", isDefault: false,
                pos: offset + new Vector3(6, 0.1f, 8), color: new Color(0f, 0.8f, 1f));
            spawnEntrance.transform.SetParent(zoneRoot.transform);

            // ── Portal (Zone 루트 하위 배치 — CatalogBaker 등록 조건) ────
            var portal = CreatePortalObject(portalId, portalTargetUri,
                pos: offset + new Vector3(6, 1.5f, 10));
            portal.transform.SetParent(zoneRoot.transform);

            EditorSceneManager.SaveScene(scene);
            EditorSceneManager.CloseScene(scene, false);
            Debug.Log($"[SceneSetupTool] '{sceneName}'에 Zone '{zoneId}' 추가 완료. Bake Catalogs를 실행하세요.");
        }

        private static GameObject CreateObstacleAt(string name, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            SetColor(go, color);
            return go;
        }

        // ──────────────────────────────────────────────────────────────────
        // ExplorationHudPanel Prefab 생성
        // ──────────────────────────────────────────────────────────────────

        private static void BuildHudPanelPrefab()
        {
            if (!AssetDatabase.IsValidFolder(PrefabDir))
            {
                AssetDatabase.CreateFolder("Assets/ZoneFlowAssets", "Prefabs");
                AssetDatabase.Refresh();
            }

            var prefabPath = $"{PrefabDir}/ExplorationHudPanel.prefab";

            // ── Root ──────────────────────────────────────────────────────
            var root = new GameObject("ExplorationHudPanel");
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;
            var hudPanel = root.AddComponent<ExplorationHudPanel>();

            // ── HealthBarContainer (좌하단) ────────────────────────────────
            var healthBarContainer = new GameObject("HealthBarContainer");
            healthBarContainer.transform.SetParent(root.transform, false);
            var healthBarRect = healthBarContainer.AddComponent<RectTransform>();
            healthBarRect.anchorMin = healthBarRect.anchorMax = healthBarRect.pivot = Vector2.zero;
            healthBarRect.anchoredPosition = new Vector2(40f, 40f);
            healthBarRect.sizeDelta = new Vector2(300f, 40f);

            var healthBg = new GameObject("HealthBg");
            healthBg.transform.SetParent(healthBarContainer.transform, false);
            var healthBgRect = healthBg.AddComponent<RectTransform>();
            healthBgRect.anchorMin = Vector2.zero;
            healthBgRect.anchorMax = Vector2.one;
            healthBgRect.offsetMin = healthBgRect.offsetMax = Vector2.zero;
            healthBg.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.75f);

            var healthFillGo = new GameObject("HealthFill");
            healthFillGo.transform.SetParent(healthBarContainer.transform, false);
            var healthFillRect = healthFillGo.AddComponent<RectTransform>();
            healthFillRect.anchorMin = Vector2.zero;
            healthFillRect.anchorMax = Vector2.one;
            healthFillRect.offsetMin = healthFillRect.offsetMax = Vector2.zero;
            var healthFill = healthFillGo.AddComponent<Image>();
            healthFill.color = new Color(0.15f, 0.75f, 0.2f, 1f);
            healthFill.type = Image.Type.Filled;
            healthFill.fillMethod = Image.FillMethod.Horizontal;
            healthFill.fillAmount = 1f;

            // ── ZoneInfoContainer (우상단) ─────────────────────────────────
            var zoneInfoContainer = new GameObject("ZoneInfoContainer");
            zoneInfoContainer.transform.SetParent(root.transform, false);
            var zoneInfoRect = zoneInfoContainer.AddComponent<RectTransform>();
            zoneInfoRect.anchorMin = zoneInfoRect.anchorMax = zoneInfoRect.pivot = Vector2.one;
            zoneInfoRect.anchoredPosition = new Vector2(-40f, -40f);
            zoneInfoRect.sizeDelta = new Vector2(240f, 40f);

            var zoneLabelGo = new GameObject("ZoneNameLabel");
            zoneLabelGo.transform.SetParent(zoneInfoContainer.transform, false);
            var zoneLabelRect = zoneLabelGo.AddComponent<RectTransform>();
            zoneLabelRect.anchorMin = Vector2.zero;
            zoneLabelRect.anchorMax = Vector2.one;
            zoneLabelRect.offsetMin = zoneLabelRect.offsetMax = Vector2.zero;
            var zoneTmp = zoneLabelGo.AddComponent<TextMeshProUGUI>();
            zoneTmp.text = "Zone";
            zoneTmp.fontSize = 24;
            zoneTmp.alignment = TextAlignmentOptions.Right;
            zoneTmp.color = Color.white;

            // ── SerializeField 연결 ───────────────────────────────────────
            var so = new SerializedObject(hudPanel);
            so.FindProperty("_healthBarContainer").objectReferenceValue = healthBarRect;
            so.FindProperty("_healthFill").objectReferenceValue = healthFill;
            so.FindProperty("_zoneInfoContainer").objectReferenceValue = zoneInfoRect;
            so.FindProperty("_zoneNameLabel").objectReferenceValue = zoneTmp;
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            Debug.Log($"[SceneSetupTool] ExplorationHudPanel 프리팹 생성: {prefabPath}");
            Selection.activeObject = prefab;
        }

        // ──────────────────────────────────────────────────────────────────
        // StoryHudPanel Prefab 생성
        // ──────────────────────────────────────────────────────────────────

        private static void BuildStoryHudPanelPrefab()
        {
            if (!AssetDatabase.IsValidFolder(PrefabDir))
            {
                AssetDatabase.CreateFolder("Assets/ZoneFlowAssets", "Prefabs");
                AssetDatabase.Refresh();
            }

            var prefabPath = $"{PrefabDir}/StoryHudPanel.prefab";

            // ── Root ──────────────────────────────────────────────────────
            var root     = new GameObject("StoryHudPanel");
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;
            var hudPanel = root.AddComponent<StoryHudPanel>();

            // ── BannerContainer (상단 full-width strip with background) ───
            var bannerGo   = new GameObject("BannerContainer");
            bannerGo.transform.SetParent(root.transform, false);
            var bannerRect = bannerGo.AddComponent<RectTransform>();
            bannerRect.anchorMin        = new Vector2(0f, 1f);
            bannerRect.anchorMax        = new Vector2(1f, 1f);
            bannerRect.pivot            = new Vector2(0.5f, 1f);
            bannerRect.anchoredPosition = Vector2.zero;
            bannerRect.sizeDelta        = new Vector2(0f, 70f);
            var bannerBg   = bannerGo.AddComponent<Image>();
            bannerBg.color = new Color(0.06f, 0.04f, 0.12f, 0.88f);

            // ── ModeLabel ─────────────────────────────────────────────────
            var modeLabelGo   = new GameObject("ModeLabel");
            modeLabelGo.transform.SetParent(bannerGo.transform, false);
            var modeLabelRect = modeLabelGo.AddComponent<RectTransform>();
            modeLabelRect.anchorMin = new Vector2(0f,   0f);
            modeLabelRect.anchorMax = new Vector2(0.5f, 1f);
            modeLabelRect.offsetMin = modeLabelRect.offsetMax = Vector2.zero;
            var modeTmp   = modeLabelGo.AddComponent<TextMeshProUGUI>();
            modeTmp.text      = "◆ STORY";
            modeTmp.fontSize  = 28;
            modeTmp.alignment = TextAlignmentOptions.Left;
            modeTmp.color     = new Color(0.7f, 0.5f, 1f);

            // ── ZoneNameLabel ─────────────────────────────────────────────
            var zoneLabelGo   = new GameObject("ZoneNameLabel");
            zoneLabelGo.transform.SetParent(bannerGo.transform, false);
            var zoneLabelRect = zoneLabelGo.AddComponent<RectTransform>();
            zoneLabelRect.anchorMin = new Vector2(0.5f, 0f);
            zoneLabelRect.anchorMax = new Vector2(1f,   1f);
            zoneLabelRect.offsetMin = zoneLabelRect.offsetMax = Vector2.zero;
            var zoneTmp   = zoneLabelGo.AddComponent<TextMeshProUGUI>();
            zoneTmp.text      = "zone@Scene";
            zoneTmp.fontSize  = 24;
            zoneTmp.alignment = TextAlignmentOptions.Right;
            zoneTmp.color     = new Color(0.85f, 0.75f, 1f);

            // ── SerializedField 연결 ──────────────────────────────────────
            var so = new SerializedObject(hudPanel);
            so.FindProperty("_bannerContainer").objectReferenceValue = bannerRect;
            so.FindProperty("_modeLabel").objectReferenceValue       = modeTmp;
            so.FindProperty("_zoneNameLabel").objectReferenceValue   = zoneTmp;
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);

            Debug.Log($"[SceneSetupTool] StoryHudPanel 프리팹 생성: {prefabPath}");
            Selection.activeObject = prefab;
        }

        // ──────────────────────────────────────────────────────────────────
        // MenuPanel Prefab 생성
        // ──────────────────────────────────────────────────────────────────

        private static void BuildMenuPanelPrefab()
        {
            if (!AssetDatabase.IsValidFolder(PrefabDir))
            {
                AssetDatabase.CreateFolder("Assets/ZoneFlowAssets", "Prefabs");
                AssetDatabase.Refresh();
            }

            var prefabPath = $"{PrefabDir}/MenuPanel.prefab";

            // 임시 씬에서 생성
            var go = new GameObject("MenuPanel");
            go.AddComponent<MenuPanel>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);

            Debug.Log($"[SceneSetupTool] MenuPanel 프리팹 생성: {prefabPath}");
            Selection.activeObject = prefab;
        }
    }
}
