using UnityEngine;
using Cysharp.Threading.Tasks;

namespace ZoneFlow
{
    /// <summary>게임플레이 부트스트랩: DevBootstrap 씬에 배치되어 Zone 씬을 초기화하고 시작 존(StartZone)을 로드한다.</summary>
    [DefaultExecutionOrder(-2000)]
    public class GamePlayBootstrap : MonoBehaviour
    {
        /// <summary>게임 시작 시 로드할 첫 번째 존.</summary>
        [field: SerializeField]
        public SceneSo StartZone { get; set; }

        /// <summary>
        /// 게임 시작 시 호출되어 서비스를 초기화하고 시작 존을 로드한다.
        /// </summary>
        protected virtual void Start()
        {
            _ = BootstrapGamePlayAsync();
        }

        /// <summary>게임플레이 부트스트랩 비동기 작업을 처리한다.</summary>
        private async UniTaskVoid BootstrapGamePlayAsync()
        {
            await SceneService.Instance.BootstrapAsync(SceneType.Zone);
            await SceneService.Instance.LoadSceneAdditiveAsync(StartZone);
        }
    }
}
