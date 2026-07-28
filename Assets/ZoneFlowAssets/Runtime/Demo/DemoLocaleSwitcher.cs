using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>
    /// LocalizationDemo 씬 전용 locale 스위처. 버튼 클릭에서 locale 코드를 받아
    /// <see cref="FontService.SelectLocaleAsync"/>를 호출한다 — 문자열(String Table)·스타일(TMP_StyleSheet)·
    /// 폰트가 한 번의 locale 전환으로 동시에 갱신되는 것을 보여주는 데모 전용 드라이버다.
    /// </summary>
    public class DemoLocaleSwitcher : MonoBehaviour
    {
        /// <summary>버튼 OnClick(String)에 static 인자로 연결한다(예: "ko", "ja", "zh-Hans", "en").</summary>
        /// <param name="localeCode">전환할 locale 코드.</param>
        public void SelectLocale(string localeCode)
        {
            Debug.Assert(FontService.IsReady, "[DemoLocaleSwitcher] CoreServices에 FontService가 없습니다.");
            FontService.Instance.SelectLocaleAsync(localeCode).Forget();
        }
    }
}
