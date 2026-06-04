using System.Threading;
using Cysharp.Threading.Tasks;
using ZoneFlow.Player;

namespace ZoneFlow
{
    /// <summary>Zone이 Shell 환경 자체가 되는 모드. Zone을 로드하고 플레이어를 SpawnPoint에 배치한다.</summary>
    public sealed class ShellMode : GamePlayMode
    {
        /// <summary>ZoneAsset과 스폰 포인트 ID로 셸 모드를 생성한다.</summary>
        public ShellMode(ZoneAsset zoneAsset, string spawnPointId = null)
            : base(zoneAsset, spawnPointId) { }

        /// <summary>모드 진입 시 플레이어를 SpawnPoint에 배치한다.</summary>
        protected override UniTask OnModeInAsync(CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }
    }
}
