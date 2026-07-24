using System.Collections.Generic;
using TMPro;

namespace Polyglot
{
    /// <summary>locale 하나에 대응하는 불변 폰트 세트. 기본 폰트·전역 fallback·활성 스타일시트·스타일 참조 폰트를 담는다.</summary>
    public sealed class FontSet
    {
        /// <summary>locale 기본 폰트(TMP_Settings 기본 폰트로 적용된다).</summary>
        public TMP_FontAsset DefaultFont { get; }

        /// <summary>TMP_Settings 전역 fallback 목록(없는 글리프 보강 전용).</summary>
        public IReadOnlyList<TMP_FontAsset> GlobalFallback { get; }

        /// <summary>locale에 대응하는 활성 스타일시트.</summary>
        public TMP_StyleSheet ActiveStyleSheet { get; }

        /// <summary>스타일별 폰트·머티리얼 프리셋(기본 폰트·fallback과 별개). 부팅 시 등록되어 <c>&lt;font=&gt;</c> 이름 해석을 가능케 한다.</summary>
        public IReadOnlyList<FontPreset> Presets { get; }

        /// <summary>폰트 세트 값들을 받아 불변 인스턴스를 생성한다.</summary>
        public FontSet(TMP_FontAsset defaultFont, IReadOnlyList<TMP_FontAsset> globalFallback, TMP_StyleSheet activeStyleSheet, IReadOnlyList<FontPreset> presets = null)
        {
            DefaultFont = defaultFont;
            GlobalFallback = globalFallback;
            ActiveStyleSheet = activeStyleSheet;
            Presets = presets ?? System.Array.Empty<FontPreset>();
        }
    }
}
