using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ZoneFlow.Editor
{
    /// <summary>ZoneId와 씬을 지정해 Zone + SpawnPoint를 빠르게 생성하는 에디터 윈도우.</summary>
    public class ZoneCreatorWindow : EditorWindow
    {
        private static readonly string[] SceneOptions = { "World1", "World2", "Intro" };

        private string  _zoneId    = "story_";
        private int     _sceneIdx  = 0;
        private Vector3 _offset    = new Vector3(-60f, 0f, 0f);
        private string  _customScene = "";
        private bool    _useCustomScene;

        [MenuItem("ZoneFlow/Create Zone...")]
        static void Open()
        {
            var w = GetWindow<ZoneCreatorWindow>("Zone 생성");
            w.minSize = new Vector2(340, 230);
        }

        void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("새 Zone 생성", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            _zoneId = EditorGUILayout.TextField("Zone ID", _zoneId);

            _useCustomScene = EditorGUILayout.Toggle("직접 입력 (씬)", _useCustomScene);
            if (_useCustomScene)
                _customScene = EditorGUILayout.TextField("씬 이름", _customScene);
            else
                _sceneIdx = EditorGUILayout.Popup("씬", _sceneIdx, SceneOptions);

            _offset = EditorGUILayout.Vector3Field("위치 오프셋", _offset);

            EditorGUILayout.Space(6);
            var sceneName = _useCustomScene ? _customScene : SceneOptions[_sceneIdx];
            var valid     = !string.IsNullOrWhiteSpace(_zoneId) && !string.IsNullOrWhiteSpace(sceneName);

            EditorGUILayout.HelpBox(
                valid
                    ? $"Zone '{_zoneId}'를 {sceneName}.unity에 생성합니다.\nGround + 기본 SpawnPoint가 포함됩니다."
                    : "Zone ID와 씬 이름을 입력하세요.",
                valid ? MessageType.Info : MessageType.Warning);

            EditorGUILayout.Space(4);
            GUI.enabled = valid;

            if (GUILayout.Button("Zone 생성"))
                DoCreate(sceneName, bakeCatalogs: false);

            if (GUILayout.Button("Zone 생성 + Bake Catalogs"))
                DoCreate(sceneName, bakeCatalogs: true);

            GUI.enabled = true;
        }

        private void DoCreate(string sceneName, bool bakeCatalogs)
        {
            CreateZoneInScene(sceneName, _zoneId, _offset);
            if (bakeCatalogs)
                CatalogBaker.BakeAll();
        }

        // ── Zone 생성 로직 ──────────────────────────────────────────────────

        private static void CreateZoneInScene(string sceneName, string zoneId, Vector3 offset)
        {
            var scenePath = $"Assets/ZoneFlowAssets/Scenes/{sceneName}.unity";

            if (!System.IO.File.Exists(scenePath))
            {
                EditorUtility.DisplayDialog("오류", $"씬 파일을 찾을 수 없습니다:\n{scenePath}", "확인");
                return;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            EditorSceneManager.SetActiveScene(scene);

            // 중복 ZoneId 체크
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.TryGetComponent<Zone>(out var existing) && existing.ZoneId == zoneId)
                {
                    EditorUtility.DisplayDialog("중복", $"ZoneId '{zoneId}'가 이미 {sceneName}에 존재합니다.", "확인");
                    EditorSceneManager.CloseScene(scene, false);
                    return;
                }
            }

            // ── Zone Root ─────────────────────────────────────────────────
            var zoneGo   = new GameObject($"Zone__{zoneId}");
            var zone     = zoneGo.AddComponent<Zone>();
            var zoneSo   = new SerializedObject(zone);
            zoneSo.FindProperty("<ZoneId>k__BackingField").stringValue = zoneId;
            zoneSo.ApplyModifiedProperties();

            // ── Ground ────────────────────────────────────────────────────
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position   = offset;
            ground.transform.localScale = new Vector3(3f, 1f, 3f);
            SetColor(ground, new Color(0.25f, 0.2f, 0.3f));
            ground.transform.SetParent(zoneGo.transform);

            // ── Default SpawnPoint ────────────────────────────────────────
            CreateSpawnPoint($"{zoneId}_default", isDefault: true,
                pos: offset + new Vector3(0f, 0.1f, -5f))
                .transform.SetParent(zoneGo.transform);

            EditorSceneManager.SaveScene(scene);
            EditorSceneManager.CloseScene(scene, false);
            Debug.Log($"[ZoneCreator] '{sceneName}'에 Zone '{zoneId}' 생성 완료. Bake Catalogs로 등록하세요.");
        }

        private static GameObject CreateSpawnPoint(string id, bool isDefault, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = $"SpawnPoint_{id}";
            go.transform.position   = pos;
            go.transform.localScale = Vector3.one * 0.4f;
            SetColor(go, isDefault ? Color.green : Color.cyan);
            Object.DestroyImmediate(go.GetComponent<SphereCollider>());

            var sp   = go.AddComponent<SpawnPoint>();
            var so   = new SerializedObject(sp);
            so.FindProperty("<SpawnPointId>k__BackingField").stringValue = id;
            so.FindProperty("<IsDefault>k__BackingField").boolValue      = isDefault;
            so.ApplyModifiedProperties();

            return go;
        }

        private static void SetColor(GameObject go, Color color)
        {
            var mr = go.GetComponent<MeshRenderer>();
            if (mr == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit")
                      ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                      ?? Shader.Find("Standard");
            var mat = new Material(shader) { color = color };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            mr.sharedMaterial = mat;
        }
    }
}
