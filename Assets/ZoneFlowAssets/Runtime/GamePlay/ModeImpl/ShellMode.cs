using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZoneFlow.Player;

namespace ZoneFlow
{
    /// <summary>UI 셸 모드. 특정 패널을 표시하며 선택적으로 존에 진입한다.</summary>
    public sealed class ShellMode : GamePlayMode
    {
        /// <summary>표시할 UI 패널 ID.</summary>
        public string PanelId { get; }

        /// <summary>패널 ID, ZoneAsset, 스폰 포인트 ID로 셸 모드를 생성한다.</summary>
        public ShellMode(string panelId, ZoneAsset zoneAsset = null, string spawnPointId = null)
            : base(zoneAsset, spawnPointId)
        {
            PanelId = panelId;
        }

        /// <summary>모드 진입 시 존이 있으면 플레이어를 스폰 포인트로 텔레포트한다.</summary>
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
