using Cysharp.Threading.Tasks;

namespace ZoneFlow
{
    /// <summary>씬 관리 서비스. 씬 로드·언로드·부트스트랩을 담당한다. 실제 구현은 scene_service feature에서 진행.</summary>
    public class SceneService : MonoService<SceneService>
    {
        /// <summary>주어진 씬 타입으로 서비스를 초기화하고 필요한 씬들을 로드한다.</summary>
        public UniTask BootstrapAsync(SceneType sceneType)
        {
            // scene_service feature에서 구현
            return UniTask.CompletedTask;
        }

        /// <summary>지정된 이름의 씬을 추가로 로드한다.</summary>
        public UniTask LoadSceneAdditiveAsync(string sceneName)
        {
            // scene_service feature에서 구현
            return UniTask.CompletedTask;
        }

        /// <summary>씬을 언로드한다. 실제 구현은 scene_service feature에서 진행.</summary>
        public UniTask UnloadSceneAsync(string sceneName) => UniTask.CompletedTask;
    }
}
