using TMPro;
using UnityEngine;

namespace Polyglot
{
    /// <summary>
    /// locale 폰트 세트를 스스로 반영하는 TMP 텍스트. 적용 신호(<see cref="FontRuntime.Applied"/>)를 구독해
    /// 기본 폰트·스타일시트를 갱신한다. <c>[ExecuteAlways]</c>라 <b>Play 없이 에디트 모드에서도</b> 즉시 반영된다.
    /// 갱신 전 해당 속성을 driven으로 표시하므로 프리뷰 값이 씬에 저장되지 않는다(폰트 미직렬화 불변식 유지).
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("Polyglot/Polyglot Text")]
    public class PolyglotText : TextMeshProUGUI
    {
        /// <summary>활성화 시 구독하고, 이미 적용된 폰트 세트가 있으면 즉시 반영한다.</summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            FontRuntime.Applied += OnFontSetApplied;
            if (FontRuntime.Current != null)
            {
                OnFontSetApplied(FontRuntime.Current);
            }
        }

        /// <summary>비활성화 시 구독을 해제한다.</summary>
        protected override void OnDisable()
        {
            FontRuntime.Applied -= OnFontSetApplied;
            base.OnDisable();
        }

        private void OnFontSetApplied(FontSet fontSet)
        {
            if (fontSet == null)
            {
                return;
            }

            // 값을 바꾸기 전에 driven 등록 — 프리뷰 변경이 씬에 저장되지 않게 한다.
            TmpFontFacade.MarkDriven(this, "m_fontAsset");
            TmpFontFacade.MarkDriven(this, "m_StyleSheet");

            if (fontSet.DefaultFont != null)
            {
                font = fontSet.DefaultFont;
            }

            styleSheet = fontSet.ActiveStyleSheet;
            SetAllDirty();
        }
    }
}
