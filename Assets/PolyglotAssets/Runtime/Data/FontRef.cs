using TMPro;
using UnityEngine;

namespace Polyglot
{
    /// <summary>locale 하나의 전체 폰트 세트를 보유하는 ScriptableObject. 식별자는 에셋 파일명(<c>so.name</c>)을 사용한다 — 별도 Id 필드 없음.</summary>
    [CreateAssetMenu(menuName = "Polyglot/Font Ref")]
    public sealed class FontRef : ScriptableObject
    {
        /// <summary>locale 기본 폰트(TMP_Settings 기본 폰트로 적용된다).</summary>
        [field: SerializeField] public TMP_FontAsset DefaultFont { get; private set; }

        /// <summary>TMP_Settings 전역 fallback 목록.</summary>
        [field: SerializeField] public TMP_FontAsset[] GlobalFallback { get; private set; } = System.Array.Empty<TMP_FontAsset>();

        /// <summary>locale에 대응하는 활성 스타일시트.</summary>
        [field: SerializeField] public TMP_StyleSheet StyleSheet { get; private set; }

        /// <summary>에디터 표시용 라벨(메타 정보, 식별자 아님).</summary>
        [field: SerializeField] public string DisplayName { get; private set; } = string.Empty;
    }
}
