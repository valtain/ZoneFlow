using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Polyglot;
using TMPro;
using UnityEditor;

namespace ZoneFlow.Tests.Editor
{
    /// <summary>
    /// boot 티어 스타일시트의 자급자족 불변식을 검증한다. boot 티어는 프리셋을 등록하지 않으므로
    /// 시트가 <c>&lt;font=&gt;</c>를 참조하면 TMP가 이름 해석에 실패해 태그를 리터럴 텍스트로 렌더한다.
    /// </summary>
    internal class BootFontStyleSheetTests
    {
        private static readonly string[] s_locales = { "en", "ko", "ja", "zh-Hans" };

        private const string BootFontRefPathFormat = "Assets/ZoneFlowAssets/Fonts/Noto/Boot/BootFontRef_{0}.asset";
        private const string ContentFontRefPathFormat = "Assets/ZoneFlowAssets/Fonts/Noto/FontRef_{0}.asset";

        private static TMP_StyleSheet LoadStyleSheet(string pathFormat, string locale)
        {
            var path = string.Format(pathFormat, locale);
            var fontRef = AssetDatabase.LoadAssetAtPath<FontRef>(path);
            Assert.IsNotNull(fontRef, $"FontRef를 찾을 수 없습니다: {path}");
            Assert.IsNotNull(fontRef.StyleSheet, $"{path}에 StyleSheet이 배정되지 않았습니다.");
            return fontRef.StyleSheet;
        }

        /// <summary>스타일시트가 노출하는 스타일 이름 목록. TMP가 내부 리스트만 공개하므로 직렬화로 열거한다.</summary>
        private static List<string> StyleNamesOf(TMP_StyleSheet sheet)
        {
            var so = new SerializedObject(sheet);
            var list = so.FindProperty("m_StyleList");
            Assert.IsNotNull(list, $"{sheet.name}에서 m_StyleList를 찾지 못했습니다.");

            var names = new List<string>();
            for (var i = 0; i < list.arraySize; i++)
            {
                names.Add(list.GetArrayElementAtIndex(i).FindPropertyRelative("m_Name").stringValue);
            }

            return names;
        }

        /// <summary>
        /// boot 티어 스타일시트의 어떤 스타일도 <c>&lt;font=&gt;</c>를 참조하지 않는다.
        /// 참조하면 boot 부팅 시 해당 태그가 화면에 그대로 찍힌다(원 증상).
        /// </summary>
        [Test]
        public void BootStyleSheets_DoNotReferenceFontTag([ValueSource(nameof(s_locales))] string locale)
        {
            var sheet = LoadStyleSheet(BootFontRefPathFormat, locale);

            foreach (var name in StyleNamesOf(sheet))
            {
                var style = sheet.GetStyle(name);
                Assert.That(style.styleOpeningDefinition, Does.Not.Contain("<font="),
                    $"{sheet.name}의 '{name}' opening 정의가 boot 티어에 없는 폰트를 참조합니다.");
                Assert.That(style.styleClosingDefinition, Does.Not.Contain("</font>"),
                    $"{sheet.name}의 '{name}' closing 정의가 boot 티어에 없는 폰트를 참조합니다.");
            }
        }

        /// <summary>
        /// boot·content 스타일시트가 같은 스타일 이름·해시 집합을 노출한다.
        /// String Table의 <c>&lt;style="Title"&gt;</c>이 두 티어 모두에서 해석되어야 하기 때문이다.
        /// </summary>
        [Test]
        public void BootAndContentStyleSheets_ExposeSameStyles([ValueSource(nameof(s_locales))] string locale)
        {
            var bootSheet = LoadStyleSheet(BootFontRefPathFormat, locale);
            var contentSheet = LoadStyleSheet(ContentFontRefPathFormat, locale);

            var bootNames = StyleNamesOf(bootSheet);
            CollectionAssert.AreEquivalent(StyleNamesOf(contentSheet), bootNames,
                $"{locale}: boot·content 스타일 이름 집합이 다릅니다.");

            foreach (var name in bootNames)
            {
                Assert.AreEqual(contentSheet.GetStyle(name).hashCode, bootSheet.GetStyle(name).hashCode,
                    $"{locale}: '{name}' 스타일 해시가 티어 간 다릅니다.");
            }
        }

        /// <summary>
        /// 스타일의 pre-parsed 태그 배열이 정의 문자열과 일치한다.
        /// <see cref="TMP_Style.RefreshStyle"/>은 생성자에서만 호출되고 역직렬화 경로엔 없어,
        /// 런타임이 실제로 파싱하는 것은 정의 문자열이 아니라 이 배열이다.
        /// </summary>
        [Test]
        public void BootStyleSheets_TagArraysMatchDefinitions([ValueSource(nameof(s_locales))] string locale)
        {
            var sheet = LoadStyleSheet(BootFontRefPathFormat, locale);

            foreach (var name in StyleNamesOf(sheet))
            {
                var style = sheet.GetStyle(name);
                CollectionAssert.AreEqual(
                    style.styleOpeningDefinition.Select(c => (uint)c).ToArray(), style.styleOpeningTagArray,
                    $"{sheet.name}의 '{name}' opening 태그 배열이 정의와 어긋납니다 — RefreshStyle 없이 편집된 시트입니다.");
                CollectionAssert.AreEqual(
                    style.styleClosingDefinition.Select(c => (uint)c).ToArray(), style.styleClosingTagArray,
                    $"{sheet.name}의 '{name}' closing 태그 배열이 정의와 어긋납니다 — RefreshStyle 없이 편집된 시트입니다.");
            }
        }
    }
}
