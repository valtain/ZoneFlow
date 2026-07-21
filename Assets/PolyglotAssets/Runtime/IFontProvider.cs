using System.Threading;
using Cysharp.Threading.Tasks;

namespace Polyglot
{
    /// <summary>locale별 폰트 세트를 로드하는 프로바이더 인터페이스. 구현 교체(Direct→Addressables) 시 호출부는 무변경이다.</summary>
    public interface IFontProvider
    {
        /// <summary>지정 locale 코드에 대응하는 폰트 세트를 비동기로 로드한다.</summary>
        /// <param name="localeCode">Localization locale 코드(예: "ko", "ja", "zh-Hans").</param>
        /// <param name="ct">취소 토큰.</param>
        UniTask<FontSet> LoadAsync(string localeCode, CancellationToken ct);
    }
}
