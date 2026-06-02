using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>
    /// Splash 씬 전용 부트스트랩. CoreServices를 로드하고 현재 씬을 언로드한 뒤 첫 내비게이션을 실행한다.
    /// ColdStartup과 달리 다른 GameObject를 비활성화하지 않으며, 로고 연출 등이 진행되는 동안 초기화한다.
    /// </summary>
    [DefaultExecutionOrder(-2000)]
    public class Bootstrap : MonoBehaviour
    {
        /// <summary>초기화 완료 후 실행할 내비게이션 설정.</summary>
        [field: SerializeField]
        public NavigationConfig StartNavigation { get; private set; } = default;

        private async UniTaskVoid Start()
        {
            await GamePlayDirector.BootstrapAsync(gameObject.scene.name, StartNavigation);
        }
    }
}
