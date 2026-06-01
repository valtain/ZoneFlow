using System.Threading;
using Cysharp.Threading.Tasks;

namespace ZoneFlow
{
    /// <summary>화면 전환 효과의 인터페이스. 초기화, Show, Hide 단계를 제공한다.</summary>
    public interface IUiTransitionEffect
    {
        /// <summary>효과를 초기화한다.</summary>
        void Initialize(UiBgCover bgCover);

        /// <summary>효과를 표시한다.</summary>
        UniTask ShowAsync(CancellationToken ct);

        /// <summary>효과를 숨긴다.</summary>
        UniTask HideAsync();
    }
}
