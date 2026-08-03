using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Polyglot
{
    /// <summary>Inspector에 직접 참조된 <see cref="FontCatalog"/>로부터 즉시 <see cref="FontSet"/>을 반환하는 <see cref="IFontProvider"/> 구현.</summary>
    public sealed class DirectRefFontProvider : IFontProvider
    {
        private readonly FontCatalog _catalog;

        /// <summary>대상 <see cref="FontCatalog"/>를 주입받아 인스턴스를 생성한다.</summary>
        /// <param name="catalog">locale → FontRef 매핑을 보유한 카탈로그.</param>
        public DirectRefFontProvider(FontCatalog catalog)
        {
            Debug.Assert(catalog != null);
            _catalog = catalog;
        }

        /// <summary>지정 locale 코드에 대응하는 폰트 세트를 즉시 반환한다. <paramref name="tier"/>는 인터페이스 계약을 맞추기 위해 받되 무시한다 —
        /// <see cref="FontCatalog"/> 직접참조에는 티어 구분이 없다(테스트/직접참조 전용 프로바이더).</summary>
        /// <param name="localeCode">Localization locale 코드(예: "ko", "ja", "zh-Hans").</param>
        /// <param name="tier">사용하지 않음(티어 미구분).</param>
        /// <param name="ct">취소 토큰(직접참조 즉시 반환이라 사용하지 않음).</param>
        public UniTask<FontSet> LoadAsync(string localeCode, FontTier tier, CancellationToken ct)
        {
            FontRef fontRef = _catalog.Resolve(localeCode);
            Debug.Assert(fontRef != null, $"FontCatalog에 locale '{localeCode}' 항목이 없습니다.");

            var fontSet = new FontSet(fontRef.DefaultFont, fontRef.GlobalFallback, fontRef.StyleSheet, fontRef.Presets);
            return UniTask.FromResult(fontSet);
        }
    }
}
