using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZoneFlow
{
    /// <summary>
    /// Intro Zone 씬에 배치하는 인트로 화면. "ZoneFlow" 텍스트를 표시하고
    /// 로딩 바 애니메이션 후 자동으로 Menu로 이동한다. (id=null 이므로 UiService 미사용)
    /// </summary>
    public class IntroScreen : MonoBehaviour
    {
        private const string MenuUri = "gameplay://shell?id=menu&switch=stack";

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
            var tmp = titleGo.AddComponent<TextMeshProUGUI>();
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
