using UnityEngine;
using Cysharp.Threading.Tasks;

namespace ZoneFlow
{
    /// <summary>
    /// Zone 씬 cold-start 처리. CoreServices가 로드되지 않은 상태에서 씬이 직접 열렸을 때
    /// 다른 오브젝트를 비활성화한 뒤 GamePlayDirector.BootstrapAsync로 초기화를 위임한다.
    /// </summary>
    [DefaultExecutionOrder(-2000)]
    public class ColdStartup : MonoBehaviour
    {
        /// <summary>초기화 완료 후 실행할 내비게이션 설정.</summary>
        [field: SerializeField]
        public NavigationConfig StartNavigation { get; private set; } = default;

        protected virtual void Awake()
        {
            if (GamePlayDirector.IsReady)
                return;

            DisableOtherRootGameObjects(gameObject.scene);
            GamePlayDirector.BootstrapAsync(gameObject.scene.name, StartNavigation).Forget();
        }

        private void DisableOtherRootGameObjects(UnityEngine.SceneManagement.Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root != gameObject)
                    root.SetActive(false);
            }
        }
    }
}
