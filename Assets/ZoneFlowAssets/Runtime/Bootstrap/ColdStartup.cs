using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace ZoneFlow
{
    /// <summary>
    /// 콜드 스타트업 처리: Zone 씬 로드 시 서비스 씬을 먼저 로드한 후 Zone 씬을 재로드한다.
    /// 서비스 씬이 이미 로드되어 있으면 아무 작업도 수행하지 않는다.
    /// </summary>
    [DefaultExecutionOrder(-2000)]
    public class ColdStartup : MonoBehaviour
    {
        /// <summary>이 ColdStartup이 배치된 Zone 씬의 타입.</summary>
        [field: SerializeField]
        public SceneType SceneType { get; set; }

        /// <summary>
        /// 씬 로드 시 서비스 준비 상태를 확인하고, 필요시 서비스 씬을 로드한 후 현재 Zone 씬을 재로드한다.
        /// </summary>
        protected virtual void Awake()
        {
            if (SceneService.IsReady)
            {
                return;
            }

            // 비활성화 작업을 위해 async void 내에서 UniTask 실행
            _ = InitializeServiceSceneAsync();
        }

        /// <summary>서비스 씬 초기화 및 Zone 씬 재로드를 처리하는 비동기 작업.</summary>
        private async UniTaskVoid InitializeServiceSceneAsync()
        {
            Scene currentScene = gameObject.scene;
            string originalSceneName = currentScene.name;

            // 자신을 제외한 다른 루트 GameObject 비활성화
            DisableOtherRootGameObjects(currentScene);

            // 서비스 씬 부트스트랩
            await SceneService.Instance.BootstrapAsync(SceneType);

            // 현재 Zone 씬 언로드
            await SceneManager.UnloadSceneAsync(currentScene);

            // Zone 씬 재로드
            await SceneService.Instance.LoadSceneAdditiveAsync(originalSceneName);
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
