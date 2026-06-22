using System.Threading;
using Cysharp.Threading.Tasks;

namespace ZoneFlow
{
    /// <summary>자유 탐색 모드. SpawnPoint에 플레이어를 배치하고 HUD를 표시한다.</summary>
    public sealed class ExplorationMode : GamePlayMode
    {
        private ExplorationHudPanel _hud;
        private InteractionPromptPanel _prompt;

        /// <summary>ZoneAsset과 스폰 포인트 ID로 탐색 모드를 생성한다.</summary>
        public ExplorationMode(ZoneAsset zoneAsset = null, string spawnPointId = null)
            : base(zoneAsset, spawnPointId) { }

        /// <summary>Zone 로드 직후 HUD와 상호작용 프롬프트를 숨김 상태로 생성한다.</summary>
        protected override async UniTask OnPlayedAsync(CancellationToken ct)
        {
            if (UiService.Instance.Panels == null) return;
            if (!UiService.Instance.Panels.TryGetPanel(ExplorationHudPanel.PanelId, out var prefab)) return;
            if (prefab.asset is not ExplorationHudPanel hudPrefab) return;

            _hud = await UiService.Instance.SetMainViewAsync(hudPrefab, ct);
            _hud.Initialize(ZoneAsset);

            if (UiService.Instance.Panels.TryGetPanel(InteractionPromptPanel.PanelId, out var promptRef)
                && promptRef.asset is InteractionPromptPanel promptPrefab)
            {
                _prompt = await UiService.Instance.SetFloatingAsync(promptPrefab, ct);
            }
        }

        /// <summary>모드 진입 시 플레이어를 배치하고 HUD를 슬라이드인한 뒤 프롬프트를 연결한다.</summary>
        protected override async UniTask OnModeInAsync(CancellationToken ct)
        {
            await UiService.Instance.ShowMainViewAsync(ct);
            _prompt?.Bind();
        }

        /// <summary>모드 퇴장 시 프롬프트 구독을 해제하고 HUD를 슬라이드아웃한다.</summary>
        protected override UniTask OnModeOutAsync(CancellationToken ct)
        {
            _prompt?.Unbind();
            return UiService.Instance.HideMainViewAsync(ct);
        }

        /// <summary>모드 종료 시 HUD와 프롬프트 인스턴스를 파괴한다.</summary>
        protected override UniTask OnStoppedAsync(CancellationToken ct)
        {
            UiService.Instance.ClearMainViewIfIs(_hud);
            _hud = null;
            UiService.Instance.ClearFloatingIfIs(_prompt);
            _prompt = null;
            return UniTask.CompletedTask;
        }
    }
}
