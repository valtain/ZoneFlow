using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZoneFlow.Editor
{
    /// <summary>World1 / World2 씬에 기본 3D 콘텐츠(지면, 장애물, 포털, 스폰포인트)를 생성하고
    /// MenuPanel 프리팹을 만드는 에디터 도구.</summary>
    public static class SceneSetupTool
    {
        private const string PrefabDir = "Assets/ZoneFlowAssets/Prefabs";

        // ──────────────────────────────────────────────────────────────────
        // Menu Item
        // ──────────────────────────────────────────────────────────────────

        [MenuItem("ZoneFlow/Setup/Setup World1")]
        public static void SetupWorld1() => SetupWorldScene("World1", "world1",
            portalTargetUri: "gameplay://exploration/world2?switch=replaceall&id=w2_entrance",
            portalTargetId:  "portal_w1");

        [MenuItem("ZoneFlow/Setup/Setup World2")]
        public static void SetupWorld2() => SetupWorldScene("World2", "world2",
            portalTargetUri: "gameplay://exploration/world1?switch=replaceall&id=w1_entrance",
            portalTargetId:  "portal_w2");

        [MenuItem("ZoneFlow/Setup/Create MenuPanel Prefab")]
        public static void CreateMenuPrefab() => BuildMenuPanelPrefab();

        [MenuItem("ZoneFlow/Setup/Setup All")]
        public static void SetupAll()
        {
            SetupWorld1();
            SetupWorld2();
            CreateMenuPrefab();
        }

        // ──────────────────────────────────────────────────────────────────
        // World Scene Setup
        // ──────────────────────────────────────────────────────────────────

        private static void SetupWorldScene(string sceneName, string zoneId,
            string portalTargetUri, string portalTargetId)
        {
            var scenePath = $"Assets/ZoneFlowAssets/Scenes/{sceneName}.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            // 이미 Ground가 있으면 스킵
            if (GameObject.Find("Ground") != null)
            {
                Debug.Log($"[SceneSetupTool] {sceneName}: 이미 설정되어 있습니다. 스킵.");
                EditorSceneManager.CloseScene(scene, false);
                return;
            }

            EditorSceneManager.SetActiveScene(scene);

            // ── Ground ──────────────────────────────────────────────────
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(5, 1, 5);
            SetColor(ground, new Color(0.25f, 0.35f, 0.25f));

            // ── Obstacles ───────────────────────────────────────────────
            CreateObstacle("Obstacle_A",  new Vector3(-6,  1, -4), new Vector3(2, 2, 2), new Color(0.4f, 0.3f, 0.2f));
            CreateObstacle("Obstacle_B",  new Vector3( 6,  1,  3), new Vector3(1.5f, 2, 3), new Color(0.35f, 0.3f, 0.25f));
            CreateObstacle("Obstacle_C",  new Vector3(-4,  1,  7), new Vector3(3, 2, 1.5f), new Color(0.3f, 0.35f, 0.3f));
            CreateObstacle("Obstacle_D",  new Vector3( 8, 0.75f, -7), new Vector3(2, 1.5f, 2), new Color(0.4f, 0.35f, 0.25f));

            // ── Default SpawnPoint ───────────────────────────────────────
            var spawnDefault = CreateSpawnPointMarker($"{zoneId}_default", isDefault: true,
                pos: new Vector3(0, 0.1f, -10), color: Color.green);
            spawnDefault.transform.SetParent(FindZoneRoot(scene)?.transform);

            // ── Entrance SpawnPoint (포털에서 도착) ──────────────────────
            var spawnEntrance = CreateSpawnPointMarker($"{zoneId}_entrance", isDefault: false,
                pos: new Vector3(8, 0.1f, 14), color: Color.cyan);
            spawnEntrance.transform.SetParent(FindZoneRoot(scene)?.transform);

            // ── Portal ───────────────────────────────────────────────────
            var portal = CreatePortalObject(portalTargetId, portalTargetUri, new Vector3(8, 1.5f, 16));

            // ── Zone의 SpawnPoint 리스트 갱신 (기존 Zone GO에 child로 이동) ──
            var zoneRoot = FindZoneRoot(scene);
            if (zoneRoot != null)
            {
                spawnDefault.transform.SetParent(zoneRoot.transform);
                spawnEntrance.transform.SetParent(zoneRoot.transform);
            }

            EditorSceneManager.SaveScene(scene);
            EditorSceneManager.CloseScene(scene, false);
            Debug.Log($"[SceneSetupTool] {sceneName} 설정 완료.");
        }

        // ──────────────────────────────────────────────────────────────────
        // Helper: Primitives
        // ──────────────────────────────────────────────────────────────────

        private static GameObject CreateObstacle(string name, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            SetColor(go, color);
            return go;
        }

        private static GameObject CreatePortalObject(string portalId, string navUri, Vector3 pos)
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
            so.ApplyModifiedProperties();

            // 레이블
            var label = new GameObject("Label");
            label.transform.SetParent(visual.transform, false);
            label.transform.localPosition = new Vector3(0, 1.5f, 0);
            // (SceneLabel 컴포넌트는 Runtime이므로 여기선 생략 — SceneSetupTool은 Editor전용)

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

        private static GameObject FindZoneRoot(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var zone = root.GetComponent<Zone>();
                if (zone != null) return root;
            }
            return null;
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
