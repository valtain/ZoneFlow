using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Polyglot.Editor.Tests
{
    /// <summary>FontStripProcessor.StripFonts가 TMP_Text의 폰트 참조를 제거하는 오염 방지 불변식을 검증한다.</summary>
    internal class FontStripTests
    {
        private const string FontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        private GameObject _go;

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
                Object.DestroyImmediate(_go);
        }

        /// <summary>StripFonts는 폰트가 지정된 TMP_Text 컴포넌트 1개를 스트립하고 font를 null로 만든다.</summary>
        [Test]
        public void StripFonts_RemovesFontReference()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            Assert.IsNotNull(font, $"테스트용 폰트 애셋을 찾을 수 없습니다: {FontAssetPath}");

            _go = new GameObject("t", typeof(RectTransform));
            var text = _go.AddComponent<TextMeshProUGUI>();
            text.font = font;
            Assert.IsNotNull(text.font);

            int n = FontStripProcessor.StripFonts(_go);

            Assert.AreEqual(1, n);
            Assert.IsNull(text.font);
        }
    }
}
