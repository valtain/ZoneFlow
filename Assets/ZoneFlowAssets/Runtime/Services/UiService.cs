using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>
    /// UI 레이어 전체를 관장하는 서비스. CoreServices 씬에 배치한다.
    /// 각 레이어는 독립 Canvas를 가지며 sortingOrder로 렌더 순서를 결정한다.
    /// </summary>
    public sealed class UiService : MonoService<UiService>
    {
        [field: SerializeField] public PanelCatalog Panels { get; private set; } = default;

        [field: SerializeField] public UiNormalLayer Normal { get; private set; } = default;
        [field: SerializeField] public UiMainViewLayer MainView { get; private set; } = default;
        [field: SerializeField] public UiOverlayLayer Overlay { get; private set; } = default;
        [field: SerializeField] public UiPopupLayer Popup { get; private set; } = default;
        [field: SerializeField] public UiFloatingLayer Floating { get; private set; } = default;
        [field: SerializeField] public UiTransitionFxLayer TransitionFx { get; private set; } = default;
        [field: SerializeField] public UiBgCover BgCover { get; private set; } = default;

        /// <summary>현재 스택에 있는 팝업의 개수.</summary>
        public int PopupCount => Popup.Count;

        /// <summary>지정한 패널을 MainView 레이어에 인스턴스화한다. 패널은 숨김 상태로 생성된다.</summary>
        public UniTask<T> SetMainViewAsync<T>(T prefab, CancellationToken ct = default) where T : UiPanel
            => MainView.SetAsync(prefab, ct);

        /// <summary>MainView 레이어의 현재 패널을 표시한다.</summary>
        public UniTask ShowMainViewAsync(CancellationToken ct = default)
            => MainView.ShowAsync(ct);

        /// <summary>MainView 레이어의 현재 패널을 숨긴다.</summary>
        public UniTask HideMainViewAsync(CancellationToken ct = default)
            => MainView.HideAsync(ct);

        /// <summary>MainView 레이어의 현재 패널을 파괴한다. 호출 전에 HideMainViewAsync로 퇴장 연출을 완료해야 한다.</summary>
        public void ClearMainView()
            => MainView.Clear();

        /// <summary>
        /// panel이 현재 MainView 패널이면 Clear한다.
        /// 이미 다른 패널로 교체된 경우에는 panel만 파괴하고 현재 패널은 유지한다.
        /// </summary>
        public void ClearMainViewIfIs(UiPanel panel)
            => MainView.ClearIfIs(panel);

        /// <summary>지정한 패널을 Overlay 레이어에 표시한다. 기존 패널은 제거된다.</summary>
        public UniTask<T> SetOverlayAsync<T>(T prefab, CancellationToken ct = default) where T : UiPanel
            => Overlay.SetAsync(prefab, ct);

        /// <summary>Overlay 레이어의 현재 패널을 제거한다.</summary>
        public UniTask ClearOverlayAsync(CancellationToken ct = default)
            => Overlay.ClearAsync(ct);

        /// <summary>지정된 팝업을 스택에 push하고 반환한다.</summary>
        public UniTask<T> PushPopupAsync<T>(T prefab, CancellationToken ct = default) where T : UiPopup
            => Popup.PushAsync(prefab, ct);

        /// <summary>최상위 팝업을 pop한다.</summary>
        public UniTask PopPopupAsync(CancellationToken ct = default)
            => Popup.PopAsync(ct);

        /// <summary>모든 팝업을 pop한다.</summary>
        public UniTask PopAllPopupsAsync(CancellationToken ct = default)
            => Popup.PopAllAsync(ct);

        /// <summary>화면 전환 효과를 시작하고 TransitionFxScope를 반환한다. await using으로 자동 종료된다.</summary>
        public static UniTask<TransitionFxScope> Transition<T>(CancellationToken ct = default)
            where T : IUiTransitionEffect => Instance.TransitionFx.Scope<T>(ct);
    }
}
