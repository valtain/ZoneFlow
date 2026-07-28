using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Polyglot
{
    /// <summary>
    /// Localization <c>Asset Table</c>("Fonts")에서 locale별 <see cref="FontRef"/>를 로드하는
    /// <see cref="IFontProvider"/> 구현. 로딩은 Localization이 Addressables로 처리하며 refcount·preload를
    /// 관리한다(직접 재구현 회피). 에디트 모드에선 AssetDatabase 직접 접근이라 프리뷰도 동작한다.
    /// </summary>
    public sealed class AddressablesFontProvider : IFontProvider
    {
        private const string TableName = "Fonts";
        private const string EntryKey = "font";

        /// <summary>지정 locale의 FontRef를 Asset Table에서 비동기로 로드해 폰트 세트를 반환한다.</summary>
        /// <param name="localeCode">Localization locale 코드(예: "ko", "ja", "zh-Hans").</param>
        /// <param name="ct">취소 토큰.</param>
        public async UniTask<FontSet> LoadAsync(string localeCode, CancellationToken ct)
        {
            var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
            Debug.Assert(locale != null, $"locale '{localeCode}'를 찾지 못했습니다.");

            // 동기 GetLocalizedAsset(WaitForCompletion)은 Localization 초기화 완료 콜백(ResourceManager.Update)
            // 안에서 호출되면 Update를 재진입한다(AQ-10). 비동기 핸들을 await해 블로킹 로드를 제거한다.
            var handle = LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<FontRef>(TableName, EntryKey, locale);
            FontRef fontRef = await handle.ToUniTask(cancellationToken: ct);
            Debug.Assert(fontRef != null, $"Asset Table '{TableName}'에 locale '{localeCode}' FontRef가 없습니다.");

            return new FontSet(fontRef.DefaultFont, fontRef.GlobalFallback, fontRef.StyleSheet, fontRef.Presets);
        }
    }
}
