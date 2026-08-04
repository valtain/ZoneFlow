using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Polyglot.Editor.Tests
{
    /// <summary>
    /// FontStripProcessor가 저장 왕복(디스크 직렬화) 후에도 "TMP 컴포넌트는 폰트를 직렬화하지 않는다"는
    /// 불변식을 유지하는지 검증한다. in-memory 스트립만으로는 충분하지 않다 —
    /// Localization의 driven property 스냅샷이 디스크 값을 되살릴 수 있다(#116).
    /// </summary>
    internal class FontStripRoundTripTests
    {
        private const string FontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
        private const string TempFolderParent = "Assets/PolyglotAssets/Tests/Editor";
        private const string TempFolderName = "Temp";
        private const string TempFolder = TempFolderParent + "/" + TempFolderName;

        private static readonly Regex TargetFieldRegex =
            new Regex(@"m_(?:fontAsset|sharedMaterial|StyleSheet):\s*\{fileID:\s*(-?\d+)");

        private static readonly Regex StyleHashRegex =
            new Regex(@"m_TextStyleHashCode:\s*(-?\d+)");

        private TMP_FontAsset _originalDefaultFont;
        private List<TMP_FontAsset> _originalFallback;
        private TMP_StyleSheet _originalStyleSheet;
        private Scene _scene;

        [SetUp]
        public void SetUp()
        {
            _originalDefaultFont = TMP_Settings.defaultFontAsset;
            _originalFallback = TMP_Settings.fallbackFontAssets;
            _originalStyleSheet = TMP_Settings.defaultStyleSheet;

            if (!AssetDatabase.IsValidFolder(TempFolder))
            {
                AssetDatabase.CreateFolder(TempFolderParent, TempFolderName);
            }

            // Test Runner가 EditMode 실행 중 제공하는 활성 씬은 이미 익명(제목 없음) 상태라
            // NewSceneMode.Additive로 새 씬을 추가하면 "untitled scene unsaved" 예외가 발생한다.
            // Single 모드로 그 익명 씬 자체를 교체한다 — 사용자가 연 실제 씬(Intro 등)은
            // Test Runner가 실행 전후로 별도 보존/복원하므로 여기서 건드리지 않는다.
            _scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [TearDown]
        public void TearDown()
        {
            TMP_Settings.defaultFontAsset = _originalDefaultFont;
            TMP_Settings.fallbackFontAssets = _originalFallback;
            TMP_Settings.defaultStyleSheet = _originalStyleSheet;

            // _scene은 다음 테스트의 SetUp에서 NewScene(Single)이 교체·폐기한다(유일한 씬이라
            // CloseScene으로 제거할 수 없음). 이 어셈블리는 EditMode 전용이라 Test Runner가
            // 실행 전후로 실제 사용자 씬(Intro 등)을 별도 보존/복원한다.
            if (AssetDatabase.IsValidFolder(TempFolder))
            {
                AssetDatabase.DeleteAsset(TempFolder);
            }
        }

        /// <summary>PolyglotText가 driven 등록한 폰트가 씬 저장 후에도 디스크에서 스트립돼 있어야 한다(주 재현).</summary>
        [Test]
        public void SceneSave_StripsDrivenFont()
        {
            var font = LoadFont(FontAssetPath);

            var go = new GameObject("DrivenText", typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(go, _scene);
            var text = go.AddComponent<PolyglotText>();
            text.font = font;
            SetStyleHash(text, 12345);
            TmpFontFacade.MarkDriven(text, "m_fontAsset");

            FontStripProcessor.StripFonts(go);

            string scenePath = TempFolder + "/DrivenSave.unity";
            EditorSceneManager.SaveScene(_scene, scenePath);

            string yaml = File.ReadAllText(scenePath);
            AssertAllTargetFieldsAreStripped(yaml);
            AssertStyleHashPreserved(yaml, 12345);
        }

        /// <summary>
        /// driven 등록이 없는 plain TextMeshProUGUI는 리플렉션 null 기록만으로 저장 후에도 클린해야 한다.
        /// TMP_Settings.defaultFontAsset을 sentinel로 지정해 Step B(재오염 경로) 판별 프로브를 겸한다.
        /// </summary>
        [Test]
        public void SceneSave_StripsUndrivenFont()
        {
            var font = LoadFont(FontAssetPath);
            var sentinel = CreateTempSentinelFont();
            TMP_Settings.defaultFontAsset = sentinel;

            var go = new GameObject("UndrivenText", typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(go, _scene);
            var text = go.AddComponent<TextMeshProUGUI>();
            text.font = font;
            SetStyleHash(text, 54321);

            FontStripProcessor.StripFonts(go);

            string scenePath = TempFolder + "/UndrivenSave.unity";
            EditorSceneManager.SaveScene(_scene, scenePath);

            string yaml = File.ReadAllText(scenePath);
            AssertAllTargetFieldsAreStripped(yaml);
            AssertStyleHashPreserved(yaml, 54321);

            Debug.Log($"[FontStripRoundTripTests] Step B 프로브 — 저장 후 in-memory font: " +
                $"{(text.font != null ? text.font.name : "null")} (sentinel: {sentinel.name})");
        }

        /// <summary>
        /// 프리팹 저장 경로: PrefabUtility.SaveAsPrefabAsset은 PrefabStage.prefabSaving을 발화시키지 않으므로
        /// FontStripMenu와 동일하게 StripFonts를 명시 호출한 뒤 저장한다. driven·undriven 컴포넌트가 혼재한다.
        /// </summary>
        [Test]
        public void PrefabSave_StripsFont()
        {
            var font = LoadFont(FontAssetPath);

            var root = new GameObject("Root", typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(root, _scene);

            var drivenGo = new GameObject("Driven", typeof(RectTransform));
            drivenGo.transform.SetParent(root.transform, false);
            var drivenText = drivenGo.AddComponent<PolyglotText>();
            drivenText.font = font;
            TmpFontFacade.MarkDriven(drivenText, "m_fontAsset");

            var undrivenGo = new GameObject("Undriven", typeof(RectTransform));
            undrivenGo.transform.SetParent(root.transform, false);
            var undrivenText = undrivenGo.AddComponent<TextMeshProUGUI>();
            undrivenText.font = font;

            FontStripProcessor.StripFonts(root);

            string prefabPath = TempFolder + "/PrefabSave.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

            string yaml = File.ReadAllText(prefabPath);
            AssertAllTargetFieldsAreStripped(yaml);
        }

        /// <summary>순진하게 UnregisterProperty만 추가하는 회귀를 막는 가드 — 재저장해도 여전히 클린해야 한다.</summary>
        [Test]
        public void SceneSave_IsIdempotent()
        {
            var font = LoadFont(FontAssetPath);

            var go = new GameObject("IdemText", typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(go, _scene);
            var text = go.AddComponent<PolyglotText>();
            text.font = font;
            TmpFontFacade.MarkDriven(text, "m_fontAsset");

            FontStripProcessor.StripFonts(go);

            string scenePath = TempFolder + "/IdempotentSave.unity";
            EditorSceneManager.SaveScene(_scene, scenePath);
            FontStripProcessor.StripFonts(go);
            EditorSceneManager.SaveScene(_scene, scenePath);

            string yaml = File.ReadAllText(scenePath);
            AssertAllTargetFieldsAreStripped(yaml);
        }

        private static TMP_FontAsset LoadFont(string path)
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            Assert.IsNotNull(font, $"테스트용 폰트 애셋을 찾을 수 없습니다: {path}");
            return font;
        }

        /// <summary>
        /// sentinel 폰트를 임시 폴더에 복제해 만든다. TMP는 텍스트가 렌더될 때 폰트의 공유 머티리얼에
        /// _ScaleRatio 값을 재계산해 기록하므로, 프로젝트의 실제 폰트 애셋을 sentinel로 쓰면 테스트 실행만으로
        /// 추적 중인 애셋이 dirty해진다. 복제본은 TearDown에서 임시 폴더와 함께 삭제되므로 부수효과가 남지 않는다.
        /// </summary>
        private static TMP_FontAsset CreateTempSentinelFont()
        {
            string sentinelPath = TempFolder + "/SentinelFont.asset";
            Assert.IsTrue(AssetDatabase.CopyAsset(FontAssetPath, sentinelPath), "sentinel 폰트 복제에 실패했습니다.");
            return LoadFont(sentinelPath);
        }

        private static void SetStyleHash(TMP_Text text, int hash)
        {
            typeof(TMP_Text)
                .GetField("m_TextStyleHashCode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(text, hash);
        }

        private static void AssertAllTargetFieldsAreStripped(string yaml)
        {
            MatchCollection matches = TargetFieldRegex.Matches(yaml);
            Assert.Greater(matches.Count, 0, "YAML에서 대상 필드(m_fontAsset/m_sharedMaterial/m_StyleSheet)를 찾지 못했습니다.");
            foreach (Match m in matches)
            {
                Assert.AreEqual("0", m.Groups[1].Value, $"필드가 스트립되지 않고 디스크에 값이 남았습니다: {m.Value}");
            }
        }

        private static void AssertStyleHashPreserved(string yaml, int expectedHash)
        {
            Match m = StyleHashRegex.Match(yaml);
            Assert.IsTrue(m.Success, "YAML에서 m_TextStyleHashCode를 찾지 못했습니다.");
            Assert.AreEqual(expectedHash.ToString(), m.Groups[1].Value, "스타일 저작 정보(m_TextStyleHashCode)가 저장 과정에서 훼손되었습니다.");
        }
    }
}
