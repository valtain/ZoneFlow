using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Polyglot.Editor
{
    /// <summary>
    /// <b>에디트 모드 전용</b> 프리뷰 훅. Game View/Scene Controls로 locale을 전환하면 런타임
    /// <see cref="FontEngine"/>을 그대로 재실행해 해당 locale 폰트를 TMP에 적용한다(driven이라 씬 미저장).
    /// Play 모드에서는 no-op — 런타임 부팅은 게임 측 FontService가 소유하며, 빌드에는 이 훅이 존재하지 않아
    /// "런타임 swap 없음" 설계와 일치시키기 위함이다.
    /// </summary>
    [InitializeOnLoad]
    static class FontPreviewHook
    {
        static FontPreviewHook() => LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

        static void OnLocaleChanged(Locale _)
        {
            // Play 모드는 게임 측 FontService가 부팅을 소유한다 — 에디터 프리뷰 훅이 중복 부팅하지 않는다.
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            var catalog = FindFontCatalog();
            if (catalog == null)
            {
                return;
            }

            new FontEngine(new DirectRefFontProvider(catalog), new TmpFontFacade())
                .BootAsync(CancellationToken.None).Forget();
        }

        static FontCatalog FindFontCatalog()
        {
            string[] guids = AssetDatabase.FindAssets("t:FontCatalog");
            if (guids.Length == 0)
            {
                return null;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<FontCatalog>(path);
        }
    }
}
