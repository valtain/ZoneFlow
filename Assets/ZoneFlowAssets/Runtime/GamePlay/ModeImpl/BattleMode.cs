using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZoneFlow.Player;

namespace ZoneFlow
{
    /// <summary>전투 모드. 스폰 포인트로 플레이어를 텔레포트한다.</summary>
    public sealed class BattleMode : GamePlayMode
    {
        /// <summary>ZoneAsset과 스폰 포인트 ID로 전투 모드를 생성한다.</summary>
        public BattleMode(ZoneAsset zoneAsset = null, string spawnPointId = null)
            : base(zoneAsset, spawnPointId) { }

        /// <summary>모드 진입 시 플레이어를 스폰 포인트로 텔레포트한다.</summary>
        protected override UniTask OnModeInAsync(CancellationToken ct)
        {
            if (Zone != null)
                TeleportPlayer(Zone.GetSpawnPoint(SpawnPointId));
            return UniTask.CompletedTask;
        }

        private static void TeleportPlayer(SpawnPoint sp)
        {
            var player = Object.FindFirstObjectByType<PlayerController>();
            if (player == null) return;
            player.transform.SetPositionAndRotation(sp.SpawnTransform.position, sp.SpawnTransform.rotation);
        }
    }
}
