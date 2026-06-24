using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace ZoneFlow
{
    /// <summary>씬 관리 서비스. 씬 로드·언로드를 담당한다. 외부에서는 GamePlayDirector를 통해 간접 호출한다.</summary>
    public class SceneService : MonoService<SceneService>
    {
        private const string CoreServicesSceneName = "CoreServices";
        private const string ContentServicesSceneName = "ContentServices";

        /// <summary>CoreServices 씬이 로드되어 있지 않으면 Additive로 로드한다. Instance 없이도 호출 가능한 static 진입점.</summary>
        public static async UniTask EnsureCoreServicesLoaded()
        {
            if (!SceneManager.GetSceneByName(CoreServicesSceneName).isLoaded)
                await SceneManager.LoadSceneAsync(CoreServicesSceneName, LoadSceneMode.Additive).ToUniTask();
        }

        /// <summary>
        /// 콘텐츠 세션 전용 ContentServices 씬을 Additive로 로드한다(미로드 시). 콘텐츠 플레이 진입 직전에 호출한다.
        /// DialogueService 등 콘텐츠 세션 수명의 서비스를 호스팅하며, 수명 경계가 곧 내러티브 진행 상태의 수명이 된다.
        /// </summary>
        public static async UniTask EnsureContentServicesLoaded()
        {
            if (!SceneManager.GetSceneByName(ContentServicesSceneName).isLoaded)
                await SceneManager.LoadSceneAsync(ContentServicesSceneName, LoadSceneMode.Additive).ToUniTask();
        }

        /// <summary>
        /// ContentServices 씬을 언로드한다(로드 상태일 때만). 메뉴 복귀 시 호출한다.
        /// 언로드되면 DialogueService.Instance 등이 null이 되어 콘텐츠 세션 상태가 소멸한다(의도된 생명주기 경계).
        /// </summary>
        public static async UniTask UnloadContentServicesAsync()
        {
            if (SceneManager.GetSceneByName(ContentServicesSceneName).isLoaded)
                await SceneManager.UnloadSceneAsync(ContentServicesSceneName).ToUniTask();
        }

        /// <summary>지정된 씬을 Additive 모드로 로드한다.</summary>
        internal UniTask LoadSceneAdditiveAsync(string sceneName, CancellationToken ct) =>
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).ToUniTask(cancellationToken: ct);

        /// <summary>지정된 씬을 언로드한다.</summary>
        internal UniTask UnloadSceneAsync(string sceneName, CancellationToken ct) =>
            SceneManager.UnloadSceneAsync(sceneName).ToUniTask(cancellationToken: ct);
    }
}
