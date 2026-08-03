using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Polyglot;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Tables;
using UnityEngine.TextCore.LowLevel;

namespace Polyglot.Editor
{
    /// <summary>
    /// 부팅 UI(Intro·메뉴·언어 피커)에 필요한 글리프만 담은 per-locale Static <see cref="TMP_FontAsset"/> 서브셋을
    /// 베이크하는 Editor 툴(tiered-font-loading Tier-1). 산출물의 FontRef 래핑·Asset Table 배선은
    /// 후속 태스크(#112·#113)가 담당하며, 이 툴은 베이크 자체만 수행한다.
    /// </summary>
    public static class BootSubsetFontBaker
    {
        /// <summary>서브셋 베이크 결과 하나(locale 단위)의 보고 정보.</summary>
        public readonly struct BakeResult
        {
            /// <summary>locale 코드.</summary>
            public string Locale { get; }

            /// <summary>요청한 유니크 글리프 수(String Table + 피커 라벨 + ASCII 합집합).</summary>
            public int RequestedCount { get; }

            /// <summary>소스 폰트에서 실제로 추가된 글리프 수.</summary>
            public int AddedCount { get; }

            /// <summary>소스 폰트에 없어 추가하지 못한 글리프. 베이크 실패가 아니라 보고용 정보다.</summary>
            public string MissingCharacters { get; }

            /// <summary>베이크된 에셋 경로.</summary>
            public string AssetPath { get; }

            /// <summary>BakeResult를 생성한다.</summary>
            public BakeResult(string locale, int requestedCount, int addedCount, string missingCharacters, string assetPath)
            {
                Locale = locale;
                RequestedCount = requestedCount;
                AddedCount = addedCount;
                MissingCharacters = missingCharacters ?? string.Empty;
                AssetPath = assetPath;
            }
        }

        // 출처: Assets/ZoneFlowAssets/Runtime/Ui/Screens/IntroScreen.cs의 LocaleOptions 네이티브 라벨.
        // Editor 어셈블리는 해당 파일의 private 상수에 접근할 수 없어 값만 그대로 재사용한다 —
        // LocaleOptions의 라벨이 바뀌면 이 배열도 함께 갱신해야 한다.
        private static readonly string[] s_pickerLabels = { "English", "한국어", "日本語", "简体中文" };

        private static readonly string[] s_locales = { "en", "ko", "ja", "zh-Hans" };

        private static readonly FieldInfo s_sourceFontFileField = typeof(TMP_FontAsset).GetField(
            "m_SourceFontFile",
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

        private const string TablesFolder = "Assets/Settings/Localization/Tables";
        private const string FontRefFolder = "Assets/ZoneFlowAssets/Fonts/Noto";

        /// <summary>베이크 산출물의 기본 출력 폴더. 후속 태스크가 이 경로의 에셋을 FontRef로 래핑한다.</summary>
        public const string DefaultOutputFolder = "Assets/ZoneFlowAssets/Fonts/Noto/Boot";

        private const int SamplingPointSize = 90;
        private const int AtlasPadding = 9;
        private const int AtlasSize = 1024;

        /// <summary>en/ko/ja/zh-Hans 4개 locale의 boot 서브셋 폰트를 <see cref="DefaultOutputFolder"/>에 베이크한다.</summary>
        [MenuItem("Tools/Polyglot/Bake Boot Subset Fonts")]
        public static void BakeAllToDefaultFolder()
        {
            BakeAll(DefaultOutputFolder);
        }

        /// <summary>en/ko/ja/zh-Hans 4개 locale의 boot 서브셋 폰트를 지정 폴더에 베이크하고 결과를 콘솔에 보고한다.</summary>
        public static List<BakeResult> BakeAll(string outputFolder)
        {
            var results = new List<BakeResult>();
            foreach (var locale in s_locales)
            {
                var result = BakeLocale(locale, outputFolder);
                if (result.HasValue)
                {
                    results.Add(result.Value);
                }
            }

            foreach (var r in results)
            {
                if (r.MissingCharacters.Length > 0)
                {
                    Debug.LogWarning(
                        $"[BootSubsetFontBaker] {r.Locale}: 요청 {r.RequestedCount}자 중 {r.MissingCharacters.Length}자가 소스 폰트에 없어 미포함(누락 — 베이크 실패 아님): \"{r.MissingCharacters}\"");
                }

                Debug.Log(
                    $"[BootSubsetFontBaker] {r.Locale} 베이크 완료 — 요청 {r.RequestedCount} / 추가 {r.AddedCount} / 누락 {r.MissingCharacters.Length} → {r.AssetPath}");
            }

            return results;
        }

        /// <summary>
        /// 단일 locale의 boot 서브셋 폰트를 베이크한다.
        /// 글리프 집합 = 해당 locale IntroStrings·MenuStrings String Table 전체 엔트리 + 피커 4라벨 + ASCII(0x20-0x7E).
        /// FontRef 또는 소스 폰트(TTF)를 찾지 못하면 null을 반환한다.
        /// </summary>
        public static BakeResult? BakeLocale(string locale, string outputFolder)
        {
            var sourceFont = ResolveSourceFont(locale);
            if (sourceFont == null)
            {
                return null;
            }

            var charSet = CollectCharacterSet(locale);
            var assetName = $"BootSubset_{locale}";

            var fontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont, SamplingPointSize, AtlasPadding, GlyphRenderMode.SDFAA,
                AtlasSize, AtlasSize, AtlasPopulationMode.Dynamic, false);
            fontAsset.name = assetName;

            // includeFontFeatures=false — true로 두면 TMP가 소스 폰트 전체 글리프의 OpenType GPOS(커닝)
            // 테이블을 통째로 가져와(#109 실측 재확인: en 서브셋 기준 7MB→2.15MB) 서브셋 취지를 무효화한다.
            // 부팅 UI 짧은 문자열에는 정밀 커닝이 불필요하므로 기본 글리프 데이터만 포함한다.
            fontAsset.TryAddCharacters(charSet, out string missing, false);

            // Dynamic으로 만들어 채운 뒤 Static으로 전환 — 128/256/512는 1024² 미만이라 공간 부족(#109 스파이크 실측).
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;
            ClearSourceFontReference(fontAsset);

            var assetPath = SaveFontAsset(fontAsset, assetName, outputFolder);

            return new BakeResult(locale, charSet.Length, fontAsset.characterTable.Count, missing, assetPath);
        }

        private static string CollectCharacterSet(string locale)
        {
            var chars = new HashSet<char>();

            AddTableCharacters(chars, $"{TablesFolder}/IntroStrings_{locale}.asset");
            AddTableCharacters(chars, $"{TablesFolder}/MenuStrings_{locale}.asset");

            foreach (var label in s_pickerLabels)
            {
                foreach (var c in label)
                {
                    chars.Add(c);
                }
            }

            for (var c = (char)0x20; c <= (char)0x7E; c++)
            {
                chars.Add(c);
            }

            return new string(chars.OrderBy(c => c).ToArray());
        }

        private static void AddTableCharacters(HashSet<char> chars, string tablePath)
        {
            var table = AssetDatabase.LoadAssetAtPath<StringTable>(tablePath);
            if (table == null)
            {
                Debug.LogError($"[BootSubsetFontBaker] String Table을 찾을 수 없음: {tablePath}");
                return;
            }

            foreach (var c in table.GenerateCharacterSet())
            {
                chars.Add(c);
            }
        }

        private static Font ResolveSourceFont(string locale)
        {
            var fontRefPath = $"{FontRefFolder}/FontRef_{locale}.asset";
            var fontRef = AssetDatabase.LoadAssetAtPath<FontRef>(fontRefPath);
            if (fontRef == null || fontRef.DefaultFont == null)
            {
                Debug.LogError($"[BootSubsetFontBaker] FontRef 또는 DefaultFont를 찾을 수 없음: {fontRefPath}");
                return null;
            }

            var sourceFont = fontRef.DefaultFont.sourceFontFile;
            if (sourceFont == null)
            {
                Debug.LogError($"[BootSubsetFontBaker] {fontRefPath}의 DefaultFont에 sourceFontFile이 없어 베이크 불가");
            }

            return sourceFont;
        }

        private static void ClearSourceFontReference(TMP_FontAsset fontAsset)
        {
            // Static 전환 후에도 소스 TTF 직렬화 참조가 남으면 Addressables 빌드 시 TTF가
            // 의존성으로 함께 번들링되어 서브셋 베이크의 목적(용량 절감)이 무효화된다 — 명시적으로 끊는다.
            s_sourceFontFileField?.SetValue(fontAsset, null);
        }

        private static string SaveFontAsset(TMP_FontAsset fontAsset, string assetName, string outputFolder)
        {
            EnsureFolder(outputFolder);

            var assetPath = $"{outputFolder}/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath) != null)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            AssetDatabase.CreateAsset(fontAsset, assetPath);

            fontAsset.material.name = $"{assetName} Material";
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

            foreach (var tex in fontAsset.atlasTextures)
            {
                tex.name = $"{assetName} Atlas";
                AssetDatabase.AddObjectToAsset(tex, fontAsset);
            }

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assetPath);

            return assetPath;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            var lastSlash = folder.LastIndexOf('/');
            var parent = folder.Substring(0, lastSlash);
            var name = folder.Substring(lastSlash + 1);

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
