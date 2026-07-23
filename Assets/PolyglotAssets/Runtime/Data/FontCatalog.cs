using TMPro;
using UnityEngine;

namespace Polyglot
{
    /// <summary>locale 코드 → <see cref="FontRef"/> 1:1 매핑을 보유하는 ScriptableObject.</summary>
    [CreateAssetMenu(menuName = "Polyglot/Font Catalog")]
    public sealed class FontCatalog : ScriptableObject
    {
        /// <summary>locale 코드와 이에 대응하는 <see cref="FontRef"/> 한 쌍.</summary>
        [System.Serializable]
        public struct Entry
        {
            /// <summary>locale 코드(예: "ko", "en").</summary>
            [field: SerializeField] public string LocaleCode { get; private set; }

            /// <summary>해당 locale의 폰트 세트.</summary>
            [field: SerializeField] public FontRef Font { get; private set; }
        }

        /// <summary>locale 코드 → FontRef 매핑 목록.</summary>
        [field: SerializeField] public Entry[] Entries { get; private set; } = System.Array.Empty<Entry>();

        /// <summary>locale 코드에 대응하는 FontRef를 반환한다. 없으면 null.</summary>
        public FontRef Resolve(string localeCode)
        {
            foreach (Entry entry in Entries)
            {
                if (entry.LocaleCode == localeCode)
                {
                    return entry.Font;
                }
            }

            return null;
        }

        /// <summary>모든 Entry의 <see cref="FontRef.DefaultFont"/>를 null이 아닌 것만 모아 반환한다.</summary>
        public System.Collections.Generic.IReadOnlyList<TMP_FontAsset> AllFonts()
        {
            var fonts = new System.Collections.Generic.List<TMP_FontAsset>();
            foreach (Entry entry in Entries)
            {
                if (entry.Font != null && entry.Font.DefaultFont != null)
                    fonts.Add(entry.Font.DefaultFont);
            }

            return fonts;
        }
    }
}
