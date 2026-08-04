using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;

namespace Polyglot.Editor.Tests
{
    /// <summary>
    /// "컴포넌트는 폰트를 직렬화하지 않고 Style만 지정한다"의 <b>후반부</b>를 지키는 설정 불변식을 검증한다.
    ///
    /// <see cref="TMP_Text.textStyle"/> getter는 메시 재생성 때마다 해시를 재해석하고, 해석에 실패하면
    /// <c>m_TextStyleHashCode</c>를 Normal로 <b>되쓴다</b>. 이 필드는 driven이 아니라 저장되어야 하는
    /// 저작 정보이므로, 되쓰기는 다음 저장 때 그대로 디스크에 남아 지정한 스타일이 소리 없이 사라진다.
    /// 해석 경로는 컴포넌트 <c>m_StyleSheet</c>(저장 시 항상 스트립됨) → <see cref="TMP_Settings.defaultStyleSheet"/>
    /// 뿐이므로, <b>부팅 전 에디트 모드</b>에서도 해석이 성립하려면 TMP Settings의 디스크 기본 스타일시트가
    /// Polyglot canonical style 집합을 갖고 있어야 한다.
    /// </summary>
    internal class StyleSheetConfigTests
    {
        private const string TableCollectionName = "Fonts";
        private const string TmpSettingsAssetPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
        private const string TmpDefaultStyleSheetPath = "Assets/TextMesh Pro/Resources/Style Sheets/Default Style Sheet.asset";

        // 폰트 Asset Table에서 스타일시트를 끌어오는 두 tier 엔트리(BootFontTableTests와 동일한 순회 패턴).
        private static readonly string[] EntryKeys = { "font", "font-boot" };

        // TMP 기본 시트에 없는 Polyglot 고유 스타일 — 해석 실패 시 리셋되는 대표 케이스.
        private const string ProbeStyleName = "Emphasis";

        private static readonly Regex DefaultStyleSheetGuidRegex =
            new Regex(@"m_defaultStyleSheet:\s*\{fileID:\s*-?\d+,\s*guid:\s*([0-9a-f]{32})");

        private TMP_StyleSheet _originalStyleSheet;
        private Scene _scene;

        [SetUp]
        public void SetUp()
        {
            _originalStyleSheet = TMP_Settings.defaultStyleSheet;

            // FontStripRoundTripTests와 동일한 이유로 Single 모드 — EditMode 러너가 제공하는 익명 씬을 교체한다.
            _scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        [TearDown]
        public void TearDown()
        {
            TMP_Settings.defaultStyleSheet = _originalStyleSheet;
        }

        /// <summary>
        /// Asset Table이 배선한 모든 locale 스타일시트(content·boot 두 tier)가 동일한 (이름, 해시) 집합을 갖는지 확인한다.
        /// locale마다 style이 갈리면 locale 전환만으로 저작 정보가 리셋되므로 이것이 근본 불변식이다.
        /// </summary>
        [Test]
        public void AllStyleSheets_ShareIdenticalStyleSet()
        {
            var sheets = CollectWiredStyleSheets();
            Assert.Greater(sheets.Count, 1, "비교할 스타일시트가 2개 미만입니다 — Asset Table 배선을 확인하세요.");

            TMP_StyleSheet reference = sheets[0];
            Dictionary<string, int> referenceSet = ReadStyleSet(reference);
            Assert.Greater(referenceSet.Count, 0, $"'{reference.name}'에 style이 하나도 없습니다.");

            for (var i = 1; i < sheets.Count; i++)
            {
                Dictionary<string, int> set = ReadStyleSet(sheets[i]);
                CollectionAssert.AreEquivalent(referenceSet, set,
                    $"'{sheets[i].name}'의 style 집합이 '{reference.name}'과 다릅니다 — locale 전환 시 저작 스타일이 리셋됩니다.");
            }
        }

        /// <summary>
        /// TMP Settings의 <b>디스크</b> 기본 스타일시트가 canonical style 집합을 전부 해석하는지 확인한다.
        /// in-memory 값은 부팅·프리뷰가 locale 시트로 driven 교체하므로, 디스크 YAML을 직접 읽어 판정한다.
        /// 이 설정이 조용히 되돌아가면 부팅 전 에디트 모드에서 저작 스타일이 사라진다.
        /// </summary>
        [Test]
        public void TmpSettingsDefaultStyleSheet_ResolvesAllAuthoredStyles()
        {
            TMP_StyleSheet onDisk = LoadOnDiskDefaultStyleSheet();
            Dictionary<string, int> canonical = ReadStyleSet(CollectWiredStyleSheets()[0]);

            foreach (KeyValuePair<string, int> style in canonical)
            {
                Assert.IsNotNull(onDisk.GetStyle(style.Value),
                    $"TMP Settings 디스크 기본 스타일시트 '{onDisk.name}'가 style '{style.Key}'({style.Value})를 해석하지 못합니다.");
            }
        }

        /// <summary>출하 설정(디스크 기본 스타일시트)에서는 저작한 style 해시가 재해석 후에도 보존된다.</summary>
        [Test]
        public void AuthoredStyleHash_SurvivesResolve_WithShippedDefaultStyleSheet()
        {
            TMP_StyleSheet onDisk = LoadOnDiskDefaultStyleSheet();
            TMP_Style probe = onDisk.GetStyle(ProbeStyleName);
            Assert.IsNotNull(probe, $"디스크 기본 스타일시트에 '{ProbeStyleName}' style이 없습니다.");

            TMP_Settings.defaultStyleSheet = onDisk;
            TMP_Text text = CreateStrippedText(probe.hashCode);

            TMP_Style resolved = ResolveStyle(text);

            Assert.AreEqual(ProbeStyleName, resolved.name, "저작한 style이 해석되지 않았습니다.");
            Assert.AreEqual(probe.hashCode, GetStyleHash(text),
                $"저작한 style '{ProbeStyleName}' 해시가 재해석 과정에서 훼손되었습니다.");
        }

        /// <summary>
        /// 해석 실패 시 TMP가 해시를 Normal로 되쓰는 실패 모드를 고정한다 — 위 설정 불변식이 왜 필요한지의 근거.
        /// TMP 기본 시트에는 Polyglot 고유 style이 없으므로 지정이 사라진다.
        /// </summary>
        [Test]
        public void AuthoredStyleHash_IsResetToNormal_WhenStyleSheetCannotResolve()
        {
            TMP_StyleSheet polyglotSheet = LoadOnDiskDefaultStyleSheet();
            TMP_Style probe = polyglotSheet.GetStyle(ProbeStyleName);
            Assert.IsNotNull(probe, $"디스크 기본 스타일시트에 '{ProbeStyleName}' style이 없습니다.");

            var tmpDefault = AssetDatabase.LoadAssetAtPath<TMP_StyleSheet>(TmpDefaultStyleSheetPath);
            Assert.IsNotNull(tmpDefault, $"TMP 기본 스타일시트를 찾을 수 없습니다: {TmpDefaultStyleSheetPath}");
            Assert.IsNull(tmpDefault.GetStyle(probe.hashCode),
                $"전제 위반 — TMP 기본 시트가 '{ProbeStyleName}'을 해석합니다. 다른 probe style을 골라야 합니다.");

            TMP_Settings.defaultStyleSheet = tmpDefault;
            TMP_Text text = CreateStrippedText(probe.hashCode);

            ResolveStyle(text);

            Assert.AreEqual(TMP_Style.NormalStyle.hashCode, GetStyleHash(text),
                "해석 실패 시 Normal로 되쓰는 TMP 동작이 바뀌었습니다 — 설정 불변식의 전제를 재검토하세요.");
        }

        /// <summary>
        /// 스타일 재해석을 1회 강제한다. <see cref="TMP_Text.textStyle"/> getter가 해석과 실패 시 되쓰기를
        /// 모두 수행하는 지점이며, <c>PopulateTextProcessingArray</c>가 <b>메시 재생성 때마다</b> 호출한다.
        /// 렌더 경유(<c>ForceMeshUpdate</c>) 대신 이 지점을 직접 호출해 캔버스·렌더 조건에 결과가 좌우되지 않게 한다.
        /// </summary>
        private static TMP_Style ResolveStyle(TMP_Text text)
        {
            return text.textStyle;
        }

        /// <summary>
        /// 저장 직후 상태(폰트·스타일시트 참조가 스트립된 PolyglotText)를 재현한다. 컴포넌트 <c>m_StyleSheet</c>가
        /// 비어야 해석이 <see cref="TMP_Settings.defaultStyleSheet"/>로 넘어가므로, 실제 저장 경로와 같은
        /// <see cref="FontStripProcessor.StripFonts"/>를 통과시킨다.
        /// </summary>
        private TMP_Text CreateStrippedText(int styleHash)
        {
            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            SceneManager.MoveGameObjectToScene(canvasGo, _scene);

            var textGo = new GameObject("StyledText", typeof(RectTransform));
            textGo.transform.SetParent(canvasGo.transform, false);
            var text = textGo.AddComponent<PolyglotText>();
            text.text = "probe";

            FontStripProcessor.StripFonts(canvasGo);
            SetStyleHash(text, styleHash);
            return text;
        }

        /// <summary>Asset Table "Fonts"의 두 tier 엔트리가 가리키는 모든 locale 스타일시트를 중복 없이 모은다.</summary>
        private static List<TMP_StyleSheet> CollectWiredStyleSheets()
        {
            var collection = LocalizationEditorSettings.GetAssetTableCollection(TableCollectionName);
            Assert.IsNotNull(collection, $"Asset Table 컬렉션 '{TableCollectionName}'을 찾을 수 없습니다.");

            var sheets = new List<TMP_StyleSheet>();
            foreach (string key in EntryKeys)
            {
                var sharedEntry = collection.SharedData.GetEntry(key);
                Assert.IsNotNull(sharedEntry, $"'{key}' 키가 SharedData에 없습니다.");

                foreach (var t in collection.AssetTables)
                {
                    var table = t as AssetTable;
                    if (table == null)
                    {
                        continue;
                    }

                    var tableEntry = table.GetEntry(sharedEntry.Id);
                    if (tableEntry == null || tableEntry.IsEmpty)
                    {
                        continue;
                    }

                    string assetPath = AssetDatabase.GUIDToAssetPath(tableEntry.Guid);
                    var fontRef = AssetDatabase.LoadAssetAtPath<FontRef>(assetPath);
                    Assert.IsNotNull(fontRef, $"경로 '{assetPath}'에서 FontRef를 로드하지 못했습니다.");
                    Assert.IsNotNull(fontRef.StyleSheet,
                        $"'{fontRef.name}'의 StyleSheet가 비어 있습니다 — 부팅 시 style 해석이 불가능해집니다.");

                    if (!sheets.Contains(fontRef.StyleSheet))
                    {
                        sheets.Add(fontRef.StyleSheet);
                    }
                }
            }
            return sheets;
        }

        /// <summary>TMP Settings YAML에서 기록된 guid를 읽어 <b>디스크</b> 기본 스타일시트를 로드한다.</summary>
        private static TMP_StyleSheet LoadOnDiskDefaultStyleSheet()
        {
            string yaml = File.ReadAllText(TmpSettingsAssetPath);
            Match m = DefaultStyleSheetGuidRegex.Match(yaml);
            Assert.IsTrue(m.Success, $"{TmpSettingsAssetPath}에서 m_defaultStyleSheet를 찾지 못했습니다.");

            string path = AssetDatabase.GUIDToAssetPath(m.Groups[1].Value);
            var sheet = AssetDatabase.LoadAssetAtPath<TMP_StyleSheet>(path);
            Assert.IsNotNull(sheet, $"guid '{m.Groups[1].Value}'에서 TMP_StyleSheet를 로드하지 못했습니다(경로: '{path}').");
            return sheet;
        }

        /// <summary>스타일시트의 (이름 → 해시) 집합을 직렬화 데이터에서 읽는다(<c>styles</c> 프로퍼티는 internal).</summary>
        private static Dictionary<string, int> ReadStyleSet(TMP_StyleSheet sheet)
        {
            var so = new SerializedObject(sheet);
            SerializedProperty list = so.FindProperty("m_StyleList");
            Assert.IsNotNull(list, $"'{sheet.name}'에서 m_StyleList를 찾지 못했습니다.");

            var set = new Dictionary<string, int>();
            for (var i = 0; i < list.arraySize; i++)
            {
                SerializedProperty element = list.GetArrayElementAtIndex(i);
                set[element.FindPropertyRelative("m_Name").stringValue] =
                    element.FindPropertyRelative("m_HashCode").intValue;
            }
            return set;
        }

        private static void SetStyleHash(TMP_Text text, int hash)
        {
            StyleHashField().SetValue(text, hash);
        }

        private static int GetStyleHash(TMP_Text text)
        {
            return (int)StyleHashField().GetValue(text);
        }

        private static System.Reflection.FieldInfo StyleHashField()
        {
            return typeof(TMP_Text).GetField("m_TextStyleHashCode",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        }
    }
}
