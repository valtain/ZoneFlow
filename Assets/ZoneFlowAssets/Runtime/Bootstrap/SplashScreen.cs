using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ZoneFlow
{
    /// <summary>
    /// Splash 씬 전용 부트스트랩. "ZoneFlow" 텍스트를 FadeIn→Hold→FadeOut 후 첫 내비게이션을 실행한다.
    /// Bootstrap.cs 대신 Splash.unity에 배치한다.
    /// </summary>
    [DefaultExecutionOrder(-2000)]
    public class SplashScreen : MonoBehaviour
    {
        [field: SerializeField] public NavigationConfig StartNavigation { get; private set; }
        [field: SerializeField] public float FadeInDuration  { get; private set; } = 0.8f;
        [field: SerializeField] public float HoldDuration    { get; private set; } = 1.2f;
        [field: SerializeField] public float FadeOutDuration { get; private set; } = 0.8f;

        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvasGroup = GetComponentInChildren<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = BuildDefaultCanvas();
            _canvasGroup.alpha = 0f;
        }

        private async UniTaskVoid Start()
        {
            var ct = destroyCancellationToken;

            // CoreServices 로드와 FadeIn 병렬 실행
            await UniTask.WhenAll(
                SceneService.EnsureCoreServicesLoaded(),
                Tween.Alpha(_canvasGroup, endValue: 1f, duration: FadeInDuration).ToUniTask(cancellationToken: ct)
            );

            await UniTask.Delay(TimeSpan.FromSeconds(HoldDuration), cancellationToken: ct);

            await Tween.Alpha(_canvasGroup, endValue: 0f, duration: FadeOutDuration).ToUniTask(cancellationToken: ct);

            var sceneName = gameObject.scene.name;
            await SceneManager.UnloadSceneAsync(sceneName).ToUniTask();
            await GamePlayDirector.Instance.NavigateAsync(StartNavigation.BuildUri(), CancellationToken.None);
        }

        private CanvasGroup BuildDefaultCanvas()
        {
            var canvasGo = new GameObject("SplashCanvas");
            canvasGo.transform.SetParent(transform, false);

            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            var cg = canvasGo.AddComponent<CanvasGroup>();

            // Background (black)
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(canvasGo.transform, false);
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = Color.black;

            // Title text
            var textGo = new GameObject("TitleText");
            textGo.transform.SetParent(canvasGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(800, 200);
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = "ZoneFlow";
            tmp.fontSize = 96;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return cg;
        }
    }
}
