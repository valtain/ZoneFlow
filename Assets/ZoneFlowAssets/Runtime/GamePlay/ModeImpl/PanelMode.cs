using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>Overlay에 UI 패널만 표시하는 모드. Zone을 로드하지 않는다.</summary>
    public sealed class PanelMode : GamePlayMode
    {
        /// <summary>표시할 UI 패널 ID.</summary>
        public string PanelId { get; }

        private UiPanel _panel;

        /// <summary>패널 ID로 패널 모드를 생성한다.</summary>
        public PanelMode(string panelId) : base()
        {
            PanelId = panelId;
        }

        /// <summary>모드 진입 시 패널을 인스턴스화하고 진입 연출을 수행한다.</summary>
        protected override async UniTask OnModeInAsync(CancellationToken ct)
        {
            if (UiService.Instance.Panels == null ||
                !UiService.Instance.Panels.TryGetPanel(PanelId, out var prefab)) return;
            _panel = Object.Instantiate(prefab, UiService.Instance.Overlay.transform);
            await _panel.ShowInAsync(ct);
        }

        /// <summary>모드 퇴장 시 퇴장 연출 후 패널을 제거한다.</summary>
        protected override async UniTask OnModeOutAsync(CancellationToken ct)
        {
            if (_panel == null) return;
            await _panel.ShowOutAsync(ct);
            Object.Destroy(_panel.gameObject);
            _panel = null;
        }
    }
}
