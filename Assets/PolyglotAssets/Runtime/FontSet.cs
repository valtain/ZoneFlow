using System.Collections.Generic;
using TMPro;

namespace Polyglot
{
    /// <summary>locale 하나에 대응하는 불변 폰트 세트. 기본 폰트·전역 fallback·활성 스타일시트를 담는다.</summary>
    public sealed class FontSet
    {
        /// <summary>locale 기본 폰트(TMP_Settings 기본 폰트로 적용된다).</summary>
        public TMP_FontAsset DefaultFont { get; }

        /// <summary>TMP_Settings 전역 fallback 목록.</summary>
        public IReadOnlyList<TMP_FontAsset> GlobalFallback { get; }

        /// <summary>locale에 대응하는 활성 스타일시트.</summary>
        public TMP_StyleSheet ActiveStyleSheet { get; }

        /// <summary>세 값을 받아 불변 인스턴스를 생성한다.</summary>
        public FontSet(TMP_FontAsset defaultFont, IReadOnlyList<TMP_FontAsset> globalFallback, TMP_StyleSheet activeStyleSheet)
        {
            DefaultFont = defaultFont;
            GlobalFallback = globalFallback;
            ActiveStyleSheet = activeStyleSheet;
        }
    }
}
