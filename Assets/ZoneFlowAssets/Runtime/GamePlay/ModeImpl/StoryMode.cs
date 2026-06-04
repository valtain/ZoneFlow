using System.Threading;
using Cysharp.Threading.Tasks;
using ZoneFlow.Player;

namespace ZoneFlow
{
    /// <summary>스토리 연출 모드. SpawnPoint에 플레이어를 배치한다.</summary>
    public sealed class StoryMode : GamePlayMode
    {
        /// <summary>ZoneAsset과 스폰 포인트 ID로 스토리 모드를 생성한다.</summary>
        public StoryMode(ZoneAsset zoneAsset = null, string spawnPointId = null)
            : base(zoneAsset, spawnPointId) { }

        /// <summary>모드 진입 시 플레이어를 SpawnPoint에 배치한다. Player가 없으면 생성한다.</summary>
        protected override UniTask OnModeInAsync(CancellationToken ct)
        {
            if (Zone != null)
                PlayerService.Instance.SpawnAt(Zone.GetSpawnPoint(SpawnPointId));
            return UniTask.CompletedTask;
        }
    }
}
