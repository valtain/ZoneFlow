using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZoneFlow
{
    /// <summary>
    /// 메인 메뉴 패널. ShellPanelRegistry에 "menu" ID로 등록되어 ShellMode("menu") 진입 시 표시된다.
    /// SceneSetupTool이 이 컴포넌트를 포함한 MenuPanel 프리팹을 생성한다.
    /// </summary>
    public class MenuPanel : MonoBehaviour
    {
        // RegistryBaker가 이 값을 읽어 PanelRegistry에 등록한다.
        public const string PanelId = "menu";
        private const string NewGameUri = "gameplay://exploration/world1?switch=replaceall";

        private void OnEnable()  => ShellPanelRegistry.RegisterInstance(PanelId, gameObject);
        private void OnDisable() => ShellPanelRegistry.UnregisterInstance(PanelId);

        private void Awake()
        {
            if (GetComponentInChildren<Canvas>() == null)
                BuildDefaultUi();
            gameObject.SetActive(false); // ShellPanelRegistry.Show()가 활성화
        }

        // ──────────────────────────────────────────
        // 버튼 핸들러
        // ──────────────────────────────────────────

        public void OnNewGame()
        {
            if (!GamePlayDirector.IsReady) return;
            GamePlayDirector.Instance
                .NavigateAsync(NewGameUri, CancellationToken.None)
                .Forget();
        }

        public void OnLoadGame()
        {
            // TODO: 저장 시스템 연동
            Debug.Log("[MenuPanel] Load Game — 미구현");
        }

        public void OnSettings()
        {
            Debug.Log("[MenuPanel] Settings — 미구현");
        }

        public void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void BuildDefaultUi()
        {
            var canvasGo = new GameObject("MenuCanvas");
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            // Background panel
            var panelGo = new GameObject("Panel");
            panelGo.transform.SetParent(canvasGo.transform, false);
            var panelRect = panelGo.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(400, 500);
            var panelImg = panelGo.AddComponent<Image>();
            panelImg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);

            // Title
            AddLabel(panelGo.transform, "TitleLabel", "ZoneFlow",
                new Vector2(0, 180), new Vector2(360, 60), 40);

            // Buttons
            AddMenuButton(panelGo.transform, "NewGameBtn",   "New Game",   new Vector2(0,  80), OnNewGame);
            AddMenuButton(panelGo.transform, "LoadGameBtn",  "Load Game",  new Vector2(0,   0), OnLoadGame);
            AddMenuButton(panelGo.transform, "SettingsBtn",  "Settings",   new Vector2(0, -80), OnSettings);
            AddMenuButton(panelGo.transform, "QuitBtn",      "Quit",       new Vector2(0,-160), OnQuit);
        }

        private static void AddLabel(Transform parent, string name, string text,
            Vector2 anchoredPos, Vector2 size, float fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }

        private static void AddMenuButton(Transform parent, string name, string label,
            Vector2 anchoredPos, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(300, 55);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.18f, 0.18f, 0.25f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.5f);
            colors.pressedColor = new Color(0.1f, 0.1f, 0.2f);
            btn.colors = colors;
            btn.onClick.AddListener(onClick);

            var txtGo = new GameObject("Label");
            txtGo.transform.SetParent(go.transform, false);
            var txtRect = txtGo.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one; txtRect.sizeDelta = Vector2.zero;
            var tmp = txtGo.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }
    }
}
