using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>
    /// Polyglot 다국어 데모 전용 컴포넌트. Intro → LocalizationDemo 전환을 <see cref="SceneService"/>로 수행해
    /// <see cref="TableLoadLogger"/>가 IntroStrings 해제 → MenuStrings 로드 순서를 관찰하도록 만드는 검증 드라이버다.
    /// 프로덕션 <c>IntroScreen</c> 내비게이션과는 완전히 무관하며, 기본적으로 비활성 GameObject로 씬에 둔다
    /// (검증 시에만 활성화한다). Intro 씬이 언로드된 뒤에도 이어지는 로드를 계속해야 하므로 자신의
    /// GameObject 수명에 묶이는 <c>destroyCancellationToken</c> 대신 <see cref="CancellationToken.None"/>을 사용한다.
    /// </summary>
    public class IntroToDemoDriver : MonoBehaviour
    {
        private const string IntroSceneName = "Intro";
        private const string DemoSceneName = "LocalizationDemo";

        /// <summary>전환 전 대기 시간(초). IntroStrings 로드 로그가 먼저 찍히도록 여유를 둔다.</summary>
        [field: SerializeField] public float DelaySeconds { get; private set; } = 1.5f;

        private void Awake()
        {
            // Awake는 씬의 모든 Start보다 먼저 실행되므로, 여기서 IntroScreen을 비활성화하면
            // 그쪽의 Start(로딩 바 트윈 시작)가 아예 호출되지 않는다 — Unload 중 파괴된 Image에
            // 접근하는 MissingReferenceException 경합을 원천 차단한다.
            StopProductionIntro();
        }

        private void Start()
        {
            RunAsync().Forget();
        }

        private static void StopProductionIntro()
        {
            var introScreen = FindFirstObjectByType<IntroScreen>();
            if (introScreen == null)
            {
                return;
            }

            introScreen.enabled = false;

            var introCanvas = introScreen.transform.Find("IntroCanvas");
            if (introCanvas != null)
            {
                introCanvas.gameObject.SetActive(false);
            }
        }

        private async UniTaskVoid RunAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(DelaySeconds), cancellationToken: CancellationToken.None);

            Debug.Assert(SceneService.IsReady, "[IntroToDemoDriver] SceneService가 준비되지 않았습니다.");

            // Intro를 먼저 언로드해 IntroStrings release 로그를 남긴 뒤 Demo를 로드해 MenuStrings load 로그가 뒤따르게 한다.
            await SceneService.Instance.UnloadSceneAsync(IntroSceneName, CancellationToken.None);
            await SceneService.Instance.LoadSceneAdditiveAsync(DemoSceneName, CancellationToken.None);
        }
    }
}
