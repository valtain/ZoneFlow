using System.Threading;
using Cysharp.Threading.Tasks;

namespace ZoneFlow
{
    /// <summary>Overlay에 UI 패널만 표시하는 모드. Zone을 로드하지 않는다.</summary>
    public sealed class PanelMode : GamePlayMode
    {
        /// <summary>표시할 UI 패널 ID.</summary>
        public string PanelId { get; }

        /// <summary>패널 ID로 패널 모드를 생성한다.</summary>
        public PanelMode(string panelId) : base()
        {
            PanelId = panelId;
        }

        /// <summary>모드 진입 시 패널을 Overlay 레이어에 표시한다.</summary>
        protected override async UniTask OnModeInAsync(CancellationToken ct)
        {
            if (ShellPanelRegistry.TryGetPrefab(PanelId, out var prefab))
                await UiService.Instance.SetOverlayAsync(prefab, ct);
        }

        /// <summary>모드 퇴장 시 Overlay 레이어를 비운다.</summary>
        protected override UniTask OnModeOutAsync(CancellationToken ct)
            => UiService.Instance.ClearOverlayAsync(ct);
    }
}
