using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>게임플레이 부트스트랩: DevBootstrap 씬에 배치되어 Zone 씬을 초기화하고 시작 내비게이션을 실행한다.</summary>
    [DefaultExecutionOrder(-2000)]
    public class GamePlayBootstrap : MonoBehaviour
    {
        /// <summary>게임 시작 시 실행할 내비게이션 설정.</summary>
        [field: SerializeField]
        public NavigationConfig StartNavigation { get; private set; } = default;

        /// <summary>게임 시작 시 호출되어 서비스를 초기화하고 시작 내비게이션을 실행한다.</summary>
        protected virtual void Start()
        {
            BootstrapGamePlayAsync().Forget();
        }

        /// <summary>게임플레이 부트스트랩 비동기 작업을 처리한다.</summary>
        private async UniTaskVoid BootstrapGamePlayAsync()
        {
            await SceneService.Instance.BootstrapAsync(SceneType.Zone);
            await GamePlayDirector.Instance.NavigateAsync(StartNavigation.BuildUri(), destroyCancellationToken);
        }
    }
}
