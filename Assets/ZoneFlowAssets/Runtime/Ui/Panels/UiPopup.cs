using System.Threading;
using Cysharp.Threading.Tasks;

namespace ZoneFlow
{
    /// <summary>팝업 UI의 기반 클래스. 스택에 push·pop될 때의 생명주기 훅을 제공한다.</summary>
    public abstract class UiPopup : UiPanel
    {
        /// <summary>팝업이 스택에 pushed될 때 호출되는 훅.</summary>
        public virtual UniTask OnPushedAsync(CancellationToken ct) => UniTask.CompletedTask;

        /// <summary>팝업이 스택에서 popped될 때 호출되는 훅.</summary>
        public virtual UniTask OnPoppedAsync(CancellationToken ct) => UniTask.CompletedTask;
    }
}
