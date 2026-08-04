using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Polyglot.Editor
{
    /// <summary>
    /// Localization의 driven property 룩업에 <see cref="PolyglotText"/>를 등록한다.
    ///
    /// Localization 패키지는 <c>LocalizedMonoBehaviour.Editor_RegisterKnownDrivenProperties</c>가
    /// UnityEvent 리스너의 <c>(target.GetType(), methodName)</c> 문자열 쌍을 Dictionary로 조회해
    /// driven 등록 여부를 판단한다. Dictionary 조회는 상속을 타지 않으므로, 패키지가 미리 등록해 둔
    /// <c>(TextMeshProUGUI, "set_text")</c> 키는 서브클래스인 <see cref="PolyglotText"/>와 매칭되지
    /// 않는다. 이 프로젝트의 씬 리스너는 <c>(PolyglotText, "SetText")</c>로 바인딩되어 있어 타입·메서드
    /// 두 조건 모두 어긋나고, 조회 실패 시 경고 없이 이벤트가 그대로 invoke되어 프리뷰 locale 문자열이
    /// <c>m_text</c>에 영구 직렬화된다(#119).
    ///
    /// 룩업은 <c>internal static</c> get-only 프로퍼티(필드 아님)이며 <c>Unity.Localization</c>
    /// 어셈블리의 <c>UnityEngine.Localization.LocalizationPropertyDriver</c>에 있다. 이 어셈블리의
    /// <c>InternalsVisibleTo</c>에 <c>Polyglot.Editor</c>가 없으므로 리플렉션이 유일한 접근 경로다.
    /// </summary>
    [InitializeOnLoad]
    public static class TextDrivenPropertyRegistrar
    {
        static TextDrivenPropertyRegistrar()
        {
            IDictionary lookup = GetLookup();
            if (lookup == null)
            {
                Debug.LogWarning("[Polyglot] Localization driven 룩업을 찾지 못했습니다 — " +
                                 "PolyglotText의 m_text가 프리뷰 값으로 직렬화될 수 있습니다.");
                return;
            }

            // 현재 씬 리스너가 바인딩한 이름. Localize 마법사가 아니라 인스펙터에서 수동으로
            // PolyglotText.SetText를 등록했기 때문에 이 이름이 쓰인다.
            lookup[(typeof(PolyglotText), "SetText")] = "m_text";

            // TMP_Text.text의 공식 setter 이름 — 향후 공식 Localize 마법사로 리바인딩되는 경우 대비.
            lookup[(typeof(PolyglotText), "set_text")] = "m_text";
        }

        /// <summary>
        /// 리플렉션으로 <c>LocalizationPropertyDriver.UnityEventDrivenPropertiesLookup</c>을 가져온다.
        /// 타입/멤버가 없으면(패키지 버전 변경 등) null을 반환한다 — 호출부가 경고 로그로 안전히 처리한다.
        /// </summary>
        static IDictionary GetLookup()
        {
            var type = Type.GetType("UnityEngine.Localization.LocalizationPropertyDriver, Unity.Localization");
            var property = type?.GetProperty("UnityEventDrivenPropertiesLookup",
                                              BindingFlags.NonPublic | BindingFlags.Static);
            return property?.GetValue(null) as IDictionary;
        }
    }
}
