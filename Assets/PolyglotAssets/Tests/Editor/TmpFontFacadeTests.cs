using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Polyglot.Editor.Tests
{
    /// <summary>TmpFontFacade.Apply가 FontSet 값을 TMP_Settings에 그대로 반영하는지 검증한다.</summary>
    internal class TmpFontFacadeTests
    {
        private const string FontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        private TMP_FontAsset _originalDefaultFont;
        private List<TMP_FontAsset> _originalFallback;
        private TMP_StyleSheet _originalStyleSheet;

        [SetUp]
        public void SetUp()
        {
            _originalDefaultFont = TMP_Settings.defaultFontAsset;
            _originalFallback = TMP_Settings.fallbackFontAssets;
            _originalStyleSheet = TMP_Settings.defaultStyleSheet;
        }

        [TearDown]
        public void TearDown()
        {
            TMP_Settings.defaultFontAsset = _originalDefaultFont;
            TMP_Settings.fallbackFontAssets = _originalFallback;
            TMP_Settings.defaultStyleSheet = _originalStyleSheet;
        }

        /// <summary>Apply 호출 후 TMP_Settings의 기본 폰트·전역 fallback·스타일시트가 FontSet 값과 일치한다.</summary>
        [Test]
        public void Apply_SetsTmpSettingsFromFontSet()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            Assert.IsNotNull(font, $"테스트용 폰트 애셋을 찾을 수 없습니다: {FontAssetPath}");

            var fontSet = new FontSet(font, new[] { font }, null);

            new TmpFontFacade().Apply(fontSet);

            Assert.AreEqual(font, TMP_Settings.defaultFontAsset);
            Assert.IsTrue(TMP_Settings.fallbackFontAssets.SequenceEqual(new[] { font }));
            Assert.IsNull(TMP_Settings.defaultStyleSheet);
        }
    }
}
