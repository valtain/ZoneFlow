using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Polyglot
{
    /// <summary>TMP_Settings 적용·Localization locale 조회를 이 한 곳에 격리하는 <see cref="IFontFacade"/> 구현.</summary>
    public sealed class TmpFontFacade : IFontFacade
    {
        /// <summary>
        /// Localization 선택 locale 코드를 조회한다. 미선택(Scene Controls의 None 등)이면
        /// Project Locale로 대체하며, 그마저 없으면 null을 반환한다(호출자가 부팅 skip을 판단).
        /// </summary>
        public string GetActiveLocaleCode()
        {
            var locale = LocalizationSettings.SelectedLocale;
            if (locale == null)
            {
                locale = LocalizationSettings.ProjectLocale;
            }

            return locale != null ? locale.Identifier.Code : null;
        }

        /// <summary>TMP_Settings 기본 폰트·전역 fallback·활성 스타일시트를 적용하고 프리셋을 등록한다(부팅 1회).</summary>
        /// <param name="fontSet">적용할 폰트 세트.</param>
        public void Apply(FontSet fontSet)
        {
            Debug.Assert(fontSet != null);

            // 에디트 모드 프리뷰가 TMP Settings 애셋을 영구 변경하지 않도록 driven으로 표시한다(변경 "전"에 호출).
            MarkDriven(TMP_Settings.instance, "m_defaultFontAsset");
            MarkDriven(TMP_Settings.instance, "m_defaultStyleSheet");

            TMP_Settings.defaultFontAsset = fontSet.DefaultFont;
            TMP_Settings.fallbackFontAssets = new List<TMP_FontAsset>(fontSet.GlobalFallback);
            TMP_Settings.defaultStyleSheet = fontSet.ActiveStyleSheet;

            // 스타일시트가 <font="이름">으로 참조하는 프리셋 폰트·머티리얼을 등록해 이름 해석을 가능케 한다.
            // (등록하지 않으면 <font=> 태그는 조용히 무시되어 기본 폰트로 렌더된다.)
            foreach (FontPreset preset in fontSet.Presets)
            {
                if (preset.Font == null)
                {
                    continue;
                }

                MaterialReferenceManager.AddFontAsset(preset.Font);
                if (preset.Material != null)
                {
                    MaterialReferenceManager.AddFontMaterial(preset.Material.GetInstanceID(), preset.Material);
                }
            }

            FontRuntime.RaiseApplied(fontSet);
        }

        /// <summary>
        /// 대상 속성을 에디터 driven으로 표시해, 프리뷰 중 변경이 씬/애셋에 저장되지 않게 한다.
        /// 반드시 값을 바꾸기 <b>전에</b> 호출한다. 플레이 모드·빌드에서는 아무 일도 하지 않는다.
        /// </summary>
        /// <param name="target">속성을 가진 오브젝트.</param>
        /// <param name="propertyPath">직렬화 속성 경로(예: "m_fontAsset").</param>
        public static void MarkDriven(Object target, string propertyPath)
        {
            if (target == null)
            {
                return;
            }

            EditorPropertyDriver.RegisterProperty(target, propertyPath);
        }

        /// <summary>
        /// 대상 속성의 driven 등록을 해제하고, 등록 시점 스냅샷 값으로 되돌린다.
        /// 플레이 모드·빌드에서는 아무 일도 하지 않는다.
        /// </summary>
        /// <param name="target">속성을 가진 오브젝트.</param>
        /// <param name="propertyPath">직렬화 속성 경로(예: "m_fontAsset").</param>
        public static void UnmarkDriven(Object target, string propertyPath)
        {
            if (target == null)
            {
                return;
            }

            EditorPropertyDriver.UnregisterProperty(target, propertyPath);
        }

        /// <summary>Localization 선택 locale을 localeCode로 전환한다. concrete 전용 API(IFontFacade 계약 아님).</summary>
        /// <param name="localeCode">전환할 locale 코드(예: "ko").</param>
        public void SetActiveLocale(string localeCode)
        {
            var loc = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
            Debug.Assert(loc != null, $"locale '{localeCode}'를 찾지 못했습니다.");
            LocalizationSettings.SelectedLocale = loc;
        }

        /// <summary>TMP_Settings 전역 fallback 목록을 설정한다. concrete 전용 API(IFontFacade 계약 아님).</summary>
        /// <param name="fonts">fallback으로 적용할 폰트 목록.</param>
        public void SetFallbacks(IReadOnlyList<TMP_FontAsset> fonts)
        {
            TMP_Settings.fallbackFontAssets = new List<TMP_FontAsset>(fonts);
        }
    }
}
