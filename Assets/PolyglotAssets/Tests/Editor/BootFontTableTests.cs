using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;

namespace Polyglot.Editor.Tests
{
    /// <summary>Asset Table "Fonts"의 "font-boot" 엔트리가 각 locale의 BootFontRef로 정상 배선됐는지 검증한다.</summary>
    internal class BootFontTableTests
    {
        private const string TableCollectionName = "Fonts";
        private const string BootEntryKey = "font-boot";

        /// <summary>locale별 "font-boot" 엔트리가 해당 BootFontRef를 가리키고, DefaultFont가 TTF 미포함 Static 아틀라스인지 확인한다.</summary>
        [TestCase("en")]
        [TestCase("ko")]
        [TestCase("ja")]
        [TestCase("zh-Hans")]
        public void BootEntry_ResolvesToStaticBootFontRef(string localeCode)
        {
            var collection = LocalizationEditorSettings.GetAssetTableCollection(TableCollectionName);
            Assert.IsNotNull(collection, $"Asset Table 컬렉션 '{TableCollectionName}'을 찾을 수 없습니다.");

            AssetTable table = null;
            foreach (var t in collection.AssetTables)
            {
                var candidate = t as AssetTable;
                if (candidate != null && candidate.LocaleIdentifier.Code == localeCode)
                {
                    table = candidate;
                    break;
                }
            }
            Assert.IsNotNull(table, $"locale '{localeCode}' 테이블을 찾을 수 없습니다.");

            var sharedEntry = collection.SharedData.GetEntry(BootEntryKey);
            Assert.IsNotNull(sharedEntry, $"'{BootEntryKey}' 키가 SharedData에 없습니다.");

            var tableEntry = table.GetEntry(sharedEntry.Id);
            Assert.IsNotNull(tableEntry, $"locale '{localeCode}' 테이블에 '{BootEntryKey}' 엔트리가 없습니다.");
            Assert.IsFalse(tableEntry.IsEmpty, $"locale '{localeCode}'의 '{BootEntryKey}' 엔트리가 비어 있습니다.");

            string assetPath = AssetDatabase.GUIDToAssetPath(tableEntry.Guid);
            Assert.IsNotEmpty(assetPath, $"GUID '{tableEntry.Guid}'에 대응하는 에셋 경로를 찾을 수 없습니다.");

            var fontRef = AssetDatabase.LoadAssetAtPath<FontRef>(assetPath);
            Assert.IsNotNull(fontRef, $"경로 '{assetPath}'에서 FontRef를 로드하지 못했습니다.");
            Assert.AreEqual($"BootFontRef_{localeCode}", fontRef.name);

            TMP_FontAsset defaultFont = fontRef.DefaultFont;
            Assert.IsNotNull(defaultFont, $"'{fontRef.name}'의 DefaultFont가 비어 있습니다.");
            Assert.AreEqual(AtlasPopulationMode.Static, defaultFont.atlasPopulationMode);
            Assert.IsNull(defaultFont.sourceFontFile, $"'{defaultFont.name}'에 TTF 소스가 포함되어 있습니다(부팅 서브셋은 TTF 미포함이어야 합니다).");
        }
    }
}
