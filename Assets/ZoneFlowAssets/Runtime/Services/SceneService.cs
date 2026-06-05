using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace ZoneFlow
{
    /// <summary>씬 관리 서비스. 씬 로드·언로드를 담당한다. 외부에서는 GamePlayDirector를 통해 간접 호출한다.</summary>
    public class SceneService : MonoService<SceneService>
    {
        private const string CoreServicesSceneName = "CoreServices";

        /// <summary>CoreServices 씬이 로드되어 있지 않으면 Additive로 로드한다. Instance 없이도 호출 가능한 static 진입점.</summary>
        public static async UniTask EnsureCoreServicesLoaded()
        {
            if (!SceneManager.GetSceneByName(CoreServicesSceneName).isLoaded)
                await SceneManager.LoadSceneAsync(CoreServicesSceneName, LoadSceneMode.Additive).ToUniTask();
        }

        /// <summary>지정된 씬을 Additive 모드로 로드한다.</summary>
        internal UniTask LoadSceneAdditiveAsync(string sceneName, CancellationToken ct) =>
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).ToUniTask(cancellationToken: ct);

        /// <summary>지정된 씬을 언로드한다.</summary>
        internal UniTask UnloadSceneAsync(string sceneName, CancellationToken ct) =>
            SceneManager.UnloadSceneAsync(sceneName).ToUniTask(cancellationToken: ct);
    }
}
