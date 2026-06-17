using UnityEditor;
using UnityEngine;

namespace ZoneFlow.Editor
{
    /// <summary>
    /// 플레이 중 GamePlayDirector의 Mode 스택과 로드된 Zone 상태를 조회하는 디버그 윈도우.
    /// 폴링하지 않고 GamePlayDebug.StateChanged 이벤트에 반응해서만 갱신한다.
    /// </summary>
    public class RuntimeStateWindow : EditorWindow
    {
        private Vector2 _scroll;
        private GUIStyle _cellTitle;
        private GUIStyle _cellSub;

        [MenuItem("ZoneFlow/Runtime State")]
        static void Open()
        {
            var w = GetWindow<RuntimeStateWindow>("Runtime State");
            w.minSize = new Vector2(300, 240);
        }

        private void OnEnable()
        {
            GamePlayDebug.StateChanged += Repaint;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private void OnDisable()
        {
            GamePlayDebug.StateChanged -= Repaint;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        private void OnPlayModeChanged(PlayModeStateChange _) => Repaint();

        private void EnsureStyles()
        {
            if (_cellTitle != null) return;

            // 다크/라이트 테마에 맞는 기본 라벨 색을 명시적으로 지정한다(커스텀 GUIStyle은 검정으로 떨어질 수 있음).
            var textColor = EditorStyles.label.normal.textColor;

            _cellTitle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 15 };
            _cellTitle.normal.textColor = textColor;

            _cellSub = new GUIStyle(EditorStyles.label) { fontSize = 11 };
            _cellSub.normal.textColor = textColor;
        }

        private void OnGUI()
        {
            if (!Application.isPlaying || !GamePlayDirector.IsReady)
            {
                EditorGUILayout.HelpBox(
                    "플레이 중에만 사용할 수 있습니다. Play 모드에 진입하면 Mode/Zone 상태가 표시됩니다.",
                    MessageType.Info);
                return;
            }

            EnsureStyles();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawModePanel();
            EditorGUILayout.Space(10);
            DrawZonePanel();

            EditorGUILayout.EndScrollView();
        }

        // ── Mode 패널 ──────────────────────────────────────────────────────

        private void DrawModePanel()
        {
            var stack = GamePlayDirector.Instance.ModeStack;
            EditorGUILayout.LabelField($"Mode Stack ({stack.Count})", EditorStyles.boldLabel);

            if (stack.Count == 0)
            {
                EditorGUILayout.LabelField("  활성 모드 없음");
                return;
            }

            // 스택 top(Active)부터 아래로, 큰 셀로 나열한다.
            for (int i = stack.Count - 1; i >= 0; i--)
            {
                var mode = stack[i];
                bool isActive = i == stack.Count - 1;

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    var title = mode.GetType().Name;
                    if (isActive) title = "● " + title + "   (ACTIVE)";
                    EditorGUILayout.LabelField(title, _cellTitle);

                    // PanelMode는 Zone이 없으므로 PanelId를, 그 외에는 연결된 Zone ID를 표시한다.
                    string link = mode is PanelMode panel
                        ? $"Panel: {panel.PanelId}"
                        : $"Zone: {mode.DebugLinkedZoneId ?? "-"}";
                    EditorGUILayout.LabelField($"State: {mode.State}      {link}", _cellSub);
                }
                EditorGUILayout.Space(2);
            }
        }

        // ── Zone 패널 ──────────────────────────────────────────────────────

        private static void DrawZonePanel()
        {
            var zones = Object.FindObjectsByType<Zone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            EditorGUILayout.LabelField($"Zones Loaded ({zones.Length})", EditorStyles.boldLabel);

            if (zones.Length == 0)
            {
                EditorGUILayout.LabelField("  로드된 Zone 없음");
                return;
            }

            foreach (var zone in zones)
            {
                bool active = zone.gameObject.activeInHierarchy;
                var id = string.IsNullOrEmpty(zone.ZoneId) ? "(no id)" : zone.ZoneId;
                EditorGUILayout.LabelField(
                    $"  • {id}  [{(active ? "active" : "inactive")}]   spawns: {zone.SpawnPoints.Count}   interactables: {zone.Interactables.Count}");
            }
        }
    }
}
