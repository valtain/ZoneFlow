using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace ZoneFlow
{
    /// <summary>
    /// 콜드 스타트업 처리: Zone 씬 로드 시 서비스 씬을 먼저 로드한 후 Zone 씬을 재로드하고 내비게이션을 실행한다.
    /// 서비스 씬이 이미 로드되어 있으면 아무 작업도 수행하지 않는다.
    /// </summary>
    [DefaultExecutionOrder(-2000)]
    public class ColdStartup : MonoBehaviour
    {
        /// <summary>이 ColdStartup이 배치된 Zone 씬의 타입.</summary>
        [field: SerializeField]
        public SceneType SceneType { get; set; }

        /// <summary>씬 재로드 완료 후 실행할 내비게이션 설정.</summary>
        [field: SerializeField]
        public NavigationConfig StartNavigation { get; private set; } = default;

        /// <summary>
        /// 씬 로드 시 서비스 준비 상태를 확인하고, 필요시 서비스 씬을 로드한 후 현재 Zone 씬을 재로드한다.
        /// </summary>
        protected virtual void Awake()
        {
            if (SceneService.IsReady)
            {
                return;
            }

            _ = InitializeServiceSceneAsync();
        }

        /// <summary>서비스 씬 초기화, Zone 씬 재로드, 내비게이션 실행을 처리하는 비동기 작업.</summary>
        private async UniTaskVoid InitializeServiceSceneAsync()
        {
            Scene currentScene = gameObject.scene;
            string originalSceneName = currentScene.name;

            DisableOtherRootGameObjects(currentScene);

            await SceneService.Instance.BootstrapAsync(SceneType);
            await SceneManager.UnloadSceneAsync(currentScene);
            await SceneService.Instance.LoadSceneAdditiveAsync(originalSceneName);

            await GamePlayDirector.Instance.NavigateAsync(StartNavigation.BuildUri(), destroyCancellationToken);
        }

        /// <summary>현재 씬에서 자신을 제외한 모든 루트 GameObject를 비활성화한다.</summary>
        private void DisableOtherRootGameObjects(Scene scene)
        {
            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                if (rootGameObject != gameObject)
                {
                    rootGameObject.SetActive(false);
                }
            }
        }
    }
}
