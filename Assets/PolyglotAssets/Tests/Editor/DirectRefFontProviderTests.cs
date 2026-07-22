using System.Threading;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Polyglot.Editor.Tests
{
    /// <summary>DirectRefFontProvider가 FontCatalog 매핑을 그대로 FontSet에 반영하는지 검증한다.</summary>
    internal class DirectRefFontProviderTests
    {
        private const string FontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        private TMP_FontAsset _font;
        private FontRef _fontRef;
        private FontCatalog _catalog;

        [SetUp]
        public void SetUp()
        {
            _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            Assert.IsNotNull(_font, $"테스트용 폰트 애셋을 찾을 수 없습니다: {FontAssetPath}");

            _fontRef = ScriptableObject.CreateInstance<FontRef>();
            var fontRefSo = new SerializedObject(_fontRef);
            fontRefSo.FindProperty("<DefaultFont>k__BackingField").objectReferenceValue = _font;
            var fallbackProp = fontRefSo.FindProperty("<GlobalFallback>k__BackingField");
            fallbackProp.arraySize = 1;
            fallbackProp.GetArrayElementAtIndex(0).objectReferenceValue = _font;
            fontRefSo.ApplyModifiedPropertiesWithoutUndo();

            _catalog = ScriptableObject.CreateInstance<FontCatalog>();
            var catalogSo = new SerializedObject(_catalog);
            var entriesProp = catalogSo.FindProperty("<Entries>k__BackingField");
            entriesProp.arraySize = 1;
            var entryProp = entriesProp.GetArrayElementAtIndex(0);
            entryProp.FindPropertyRelative("<LocaleCode>k__BackingField").stringValue = "ko";
            entryProp.FindPropertyRelative("<Font>k__BackingField").objectReferenceValue = _fontRef;
            catalogSo.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            if (_catalog != null)
                Object.DestroyImmediate(_catalog);
            if (_fontRef != null)
                Object.DestroyImmediate(_fontRef);
        }

        /// <summary>카탈로그가 매핑한 FontRef의 값이 그대로 담긴 FontSet을 반환한다.</summary>
        [Test]
        public void LoadAsync_ResolvedLocale_ReturnsMappedFontSet()
        {
            var provider = new DirectRefFontProvider(_catalog);

            FontSet result = provider.LoadAsync("ko", CancellationToken.None).GetAwaiter().GetResult();

            Assert.IsNotNull(result);
            Assert.AreEqual(_font, result.DefaultFont);
            Assert.AreEqual(1, result.GlobalFallback.Count);
            Assert.AreEqual(_font, result.GlobalFallback[0]);
        }
    }
}
