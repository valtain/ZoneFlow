using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using Polyglot;
using UnityEngine;
using UnityEngine.UI;

namespace ZoneFlow
{
    /// <summary>
    /// Intro Zone 씬에 배치하는 인트로 화면. "ZoneFlow" 텍스트를 표시하고
    /// 로딩 바 애니메이션 후 자동으로 Menu로 이동한다. (id=null 이므로 UiService 미사용)
    /// first-run(=<see cref="FontService.HasLocaleBeenChosen"/>이 false)에는 언어 피커를 먼저 표시한다.
    /// </summary>
    public class IntroScreen : MonoBehaviour
    {
        private const string MenuUri = "gameplay://panel?id=menu&switch=stack";

        // locale 코드 → 네이티브 표기 라벨. 언어 선택기이므로 영문-only 규칙의 의도적 예외.
        private static readonly (string Code, string Label)[] LocaleOptions =
        {
            ("en", "English"),
            ("ko", "한국어"),
            ("ja", "日本語"),
            ("zh-Hans", "简体中文"),
        };

        [field: SerializeField] public float TitleFadeInDuration { get; private set; } = 0.6f;
        [field: SerializeField] public float LoadingDuration     { get; private set; } = 2.0f;
        [field: SerializeField] public float HoldAfterLoad       { get; private set; } = 0.4f;

        [SerializeField] private Image _fillImage;
        private bool _navigated;

        private void Start()
        {
            IntroAsync().Forget();
        }

        private async UniTaskVoid IntroAsync()
        {
            if (_navigated) return;

            var ct = destroyCancellationToken;

            // first-run 언어 피커 게이트 — 선택 완료 후에는 스킵된다.
            Debug.Assert(FontService.IsReady, "[IntroScreen] CoreServices에 FontService가 없습니다.");
            if (!FontService.Instance.HasLocaleBeenChosen)
            {
                await ShowLanguagePickerAsync(ct);
            }

            // 타이틀 FadeIn
            var cg = GetComponentInChildren<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                await Tween.Alpha(cg, endValue: 1f, duration: TitleFadeInDuration).ToUniTask(cancellationToken: ct);
            }

            // 로딩 바 진행
            Debug.Assert(_fillImage != null);
            await Tween.Custom(
                startValue: 0f, endValue: 1f, duration: LoadingDuration,
                onValueChange: val => _fillImage.fillAmount = val
            ).ToUniTask(cancellationToken: ct);

            await UniTask.Delay(TimeSpan.FromSeconds(HoldAfterLoad), cancellationToken: ct);

            _navigated = true;
            await GamePlayDirector.Instance.NavigateAsync(MenuUri, CancellationToken.None);
        }

        /// <summary>
        /// 언어 피커를 표시하고 사용자가 버튼을 클릭할 때까지 대기한다. 선택 후 locale을 영속화하고
        /// <see cref="FontService"/>를 재부팅해 선택 locale의 깨끗한 per-locale 폰트를 적용한다.
        /// </summary>
        private async UniTask ShowLanguagePickerAsync(CancellationToken ct)
        {
            // 피커 자기 라벨(4개 네이티브 표기) 렌더용 임시 전역 fallback. 컴포넌트에 폰트를 직렬화하지 않는다 —
            // 선택 후 FontService.SelectLocaleAsync가 정규 per-locale fallback으로 덮어써 불변식을 되돌린다.
            FontService.Instance.ApplyPickerFallbacks();

            var canvasGo = BuildLanguagePickerUi();
            var tcs = new UniTaskCompletionSource<string>();

            for (var i = 0; i < LocaleOptions.Length; i++)
            {
                var option = LocaleOptions[i];
                var yOffset = 150f - i * 110f;
                CreateLanguageButton(canvasGo.transform, option.Label, yOffset, () => tcs.TrySetResult(option.Code));
            }

            var code = await tcs.Task.AttachExternalCancellation(ct);

            await FontService.Instance.SelectLocaleAsync(code);

            Destroy(canvasGo);
        }

        private GameObject BuildLanguagePickerUi()
        {
            var canvasGo = new GameObject("LanguagePickerCanvas");
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one; bgRect.sizeDelta = Vector2.zero;
            bgGo.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.92f);

            return canvasGo;
        }

        private static void CreateLanguageButton(Transform parent, string label, float yOffset, Action onClick)
        {
            var buttonGo = new GameObject($"LanguageButton_{label}");
            buttonGo.transform.SetParent(parent, false);
            var rect = buttonGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(300, 55);
            rect.anchoredPosition = new Vector2(0f, yOffset);

            // MenuPanel 버튼과 동일한 스타일(색·호버·폰트)로 통일한다.
            var img = buttonGo.AddComponent<Image>();
            img.color = new Color(0.18f, 0.18f, 0.25f);

            var button = buttonGo.AddComponent<Button>();
            button.targetGraphic = img;
            var colors = button.colors;
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.5f);
            colors.pressedColor = new Color(0.1f, 0.1f, 0.2f);
            button.colors = colors;
            button.onClick.AddListener(() => onClick());

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(buttonGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.sizeDelta = Vector2.zero;
            var tmp = textGo.AddComponent<PolyglotText>();
            tmp.text = label;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }

        [ContextMenu("Build UI")]
        private void BuildDefaultUi()
        {
            var canvasGo = new GameObject("IntroCanvas");
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();
            var cg = canvasGo.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            // Background
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero; bgRect.anchorMax = Vector2.one; bgRect.sizeDelta = Vector2.zero;
            bgGo.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f);

            // Title
            var titleGo = new GameObject("TitleText");
            titleGo.transform.SetParent(canvasGo.transform, false);
            var titleRect = titleGo.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.55f);
            titleRect.anchorMax = new Vector2(0.5f, 0.55f);
            titleRect.sizeDelta = new Vector2(800, 160);
            var tmp = titleGo.AddComponent<PolyglotText>();
            tmp.text = "ZoneFlow";
            tmp.fontSize = 80;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            // Loading bar background
            var barBgGo = new GameObject("LoadingBarBg");
            barBgGo.transform.SetParent(canvasGo.transform, false);
            var barBgRect = barBgGo.AddComponent<RectTransform>();
            barBgRect.anchorMin = new Vector2(0.5f, 0.38f);
            barBgRect.anchorMax = new Vector2(0.5f, 0.38f);
            barBgRect.sizeDelta = new Vector2(600, 12);
            var barBgImg = barBgGo.AddComponent<Image>();
            barBgImg.color = new Color(0.2f, 0.2f, 0.2f);

            // Loading bar fill
            var barGo = new GameObject("LoadingBar");
            barGo.transform.SetParent(canvasGo.transform, false);
            var barRect = barGo.AddComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0.5f, 0.38f);
            barRect.anchorMax = new Vector2(0.5f, 0.38f);
            barRect.sizeDelta = new Vector2(600, 12);
            barRect.pivot = new Vector2(0f, 0.5f);
            barRect.anchoredPosition = new Vector2(-300f, 0f);
            _fillImage = barGo.AddComponent<Image>();
            _fillImage.color = new Color(0.3f, 0.7f, 1f);
            _fillImage.type = Image.Type.Filled;
            _fillImage.fillMethod = Image.FillMethod.Horizontal;
            _fillImage.fillAmount = 0f;
        }
    }
}
