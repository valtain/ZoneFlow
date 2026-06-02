using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>에디터 개발 진입점. DevBootstrap 씬에 배치되며 빌드에 포함하지 않는다.</summary>
    [DefaultExecutionOrder(-2000)]
    public class DevBootstrap : MonoBehaviour
    {
        /// <summary>게임 시작 시 실행할 내비게이션 설정.</summary>
        [field: SerializeField]
        public NavigationConfig StartNavigation { get; private set; } = default;

        private async UniTaskVoid Start()
        {
            await GamePlayDirector.BootstrapAsync(gameObject.scene.name, StartNavigation);
        }
    }
}
