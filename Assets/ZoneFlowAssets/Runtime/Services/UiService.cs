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
        [field: SerializeField] public UiNormalLayer Normal { get; private set; } = default;
        [field: SerializeField] public UiMainViewLayer MainView { get; private set; } = default;
        [field: SerializeField] public UiOverlayLayer Overlay { get; private set; } = default;
        [field: SerializeField] public UiPopupLayer Popup { get; private set; } = default;
        [field: SerializeField] public UiFloatingLayer Floating { get; private set; } = default;
        [field: SerializeField] public UiTransitionFxLayer TransitionFx { get; private set; } = default;
        [field: SerializeField] public UiBgCover BgCover { get; private set; } = default;

        /// <summary>현재 스택에 있는 팝업의 개수.</summary>
        public int PopupCount => Popup.Count;

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
