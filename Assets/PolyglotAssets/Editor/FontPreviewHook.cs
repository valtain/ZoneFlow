using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Polyglot.Editor
{
    /// <summary>
    /// 에디터에서 Game View locale을 전환하면 런타임 <see cref="FontEngine"/>을 그대로 재실행해
    /// 해당 locale의 폰트를 TMP_Settings에 프리뷰로 적용한다. 컴포넌트는 손대지 않는다(불변식 유지).
    /// FontCatalog가 아직 없는 현재는 no-op 스캐폴딩 상태다.
    /// </summary>
    [InitializeOnLoad]
    static class FontPreviewHook
    {
        static FontPreviewHook() => LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

        static void OnLocaleChanged(Locale _)
        {
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
