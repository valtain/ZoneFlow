using System.Threading;
using Cysharp.Threading.Tasks;
using ZoneFlow.Player;

namespace ZoneFlow
{
    /// <summary>자유 탐색 모드. SpawnPoint에 플레이어를 배치하고 HUD를 표시한다.</summary>
    public sealed class ExplorationMode : GamePlayMode
    {
        private HudPanel _hud;

        /// <summary>ZoneAsset과 스폰 포인트 ID로 탐색 모드를 생성한다.</summary>
        public ExplorationMode(ZoneAsset zoneAsset = null, string spawnPointId = null)
            : base(zoneAsset, spawnPointId) { }

        /// <summary>Zone 로드 직후 HUD를 숨김 상태로 생성한다.</summary>
        protected override async UniTask OnPlayedAsync(CancellationToken ct)
        {
            if (UiService.Instance.Panels == null) return;
            if (!UiService.Instance.Panels.TryGetPanel(HudPanel.PanelId, out var prefab)) return;
            if (prefab is not HudPanel hudPrefab) return;

            _hud = await UiService.Instance.SetMainViewAsync(hudPrefab, ct);
            _hud.Initialize(ZoneAsset);
        }

        /// <summary>모드 진입 시 플레이어를 배치하고 HUD를 슬라이드인한다.</summary>
        protected override async UniTask OnModeInAsync(CancellationToken ct)
        {
            if (Zone != null)
                PlayerService.Instance.SpawnAt(Zone.GetSpawnPoint(SpawnPointId));

            await UiService.Instance.ShowMainViewAsync(ct);
        }

        /// <summary>모드 퇴장 시 HUD를 슬라이드아웃한다.</summary>
        protected override UniTask OnModeOutAsync(CancellationToken ct)
            => UiService.Instance.HideMainViewAsync(ct);

        /// <summary>모드 종료 시 HUD 인스턴스를 파괴한다.</summary>
        protected override UniTask OnStoppedAsync(CancellationToken ct)
        {
            UiService.Instance.ClearMainView();
            _hud = null;
            return UniTask.CompletedTask;
        }
    }
}
