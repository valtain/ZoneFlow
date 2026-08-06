using System.Threading;
using Cysharp.Threading.Tasks;

namespace Polyglot
{
    /// <summary>locale별 폰트 세트를 로드하는 프로바이더 인터페이스. 구현 교체(Direct→Addressables) 시 호출부는 무변경이다.</summary>
    public interface IFontProvider
    {
        /// <summary>
        /// 지정 locale 코드·티어에 대응하는 폰트 세트를 비동기로 로드한다.
        /// 로드에 실패하면 <c>null</c>을 반환한다 — 호출자는 TMP 상태를 그대로 두어 직전 티어를 유지해야 한다.
        /// </summary>
        /// <param name="localeCode">Localization locale 코드(예: "ko", "ja", "zh-Hans").</param>
        /// <param name="tier">로드할 <see cref="FontTier"/>(Boot/Content).</param>
        /// <param name="ct">취소 토큰.</param>
        /// <returns>로드한 폰트 세트. 실패 시 <c>null</c>.</returns>
        UniTask<FontSet> LoadAsync(string localeCode, FontTier tier, CancellationToken ct);
    }
}
