using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Polyglot
{
    /// <summary>TMP_Settings 적용·Localization locale 조회를 이 한 곳에 격리하는 <see cref="IFontFacade"/> 구현.</summary>
    public sealed class TmpFontFacade : IFontFacade
    {
        /// <summary>Localization 선택 locale 코드를 조회한다(미선택 시 기본 locale).</summary>
        public string GetActiveLocaleCode()
        {
            var locale = LocalizationSettings.SelectedLocale;
            Debug.Assert(locale != null, "LocalizationSettings.SelectedLocale이 null입니다.");
            return locale != null ? locale.Identifier.Code : null;
        }

        /// <summary>TMP_Settings 기본 폰트·전역 fallback·활성 스타일시트를 적용한다(부팅 1회).</summary>
        /// <param name="fontSet">적용할 폰트 세트.</param>
        public void Apply(FontSet fontSet)
        {
            Debug.Assert(fontSet != null);

            TMP_Settings.defaultFontAsset = fontSet.DefaultFont;
            TMP_Settings.fallbackFontAssets = new List<TMP_FontAsset>(fontSet.GlobalFallback);
            TMP_Settings.defaultStyleSheet = fontSet.ActiveStyleSheet;
        }
    }
}
