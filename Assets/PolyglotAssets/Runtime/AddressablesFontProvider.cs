using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

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
        private const string ContentEntryKey = "font";
        private const string BootEntryKey = "font-boot";

        /// <summary>티어에 대응하는 Asset Table 엔트리키를 반환한다(엔트리키 자체가 티어 선택자 — 기구 1a).</summary>
        /// <param name="tier">조회할 <see cref="FontTier"/>.</param>
        private static string EntryKeyFor(FontTier tier) => tier switch
        {
            FontTier.Boot => BootEntryKey,
            FontTier.Content => ContentEntryKey,
            _ => ContentEntryKey
        };

        /// <summary>
        /// 지정 locale·티어의 FontRef를 Asset Table에서 비동기로 로드해 폰트 세트를 반환한다.
        /// content 티어는 원격 그룹이라 실패할 수 있으며, 이때 예외 대신 <c>null</c>을 반환해
        /// 호출자가 직전 티어(boot)를 유지하게 한다.
        /// </summary>
        /// <param name="localeCode">Localization locale 코드(예: "ko", "ja", "zh-Hans").</param>
        /// <param name="tier">로드할 <see cref="FontTier"/>(Boot/Content).</param>
        /// <param name="ct">취소 토큰.</param>
        /// <returns>로드한 폰트 세트. 실패 시 <c>null</c>.</returns>
        public async UniTask<FontSet> LoadAsync(string localeCode, FontTier tier, CancellationToken ct)
        {
            var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
            Debug.Assert(locale != null, $"locale '{localeCode}'를 찾지 못했습니다.");

            string entryKey = EntryKeyFor(tier);

            // 동기 GetLocalizedAsset(WaitForCompletion)은 Localization 초기화 완료 콜백(ResourceManager.Update)
            // 안에서 호출되면 Update를 재진입한다(AQ-10). 비동기 핸들을 await해 블로킹 로드를 제거한다.
            var handle = LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<FontRef>(TableName, entryKey, locale);

            FontRef fontRef = tier == FontTier.Content
                ? await LoadContentOrNullAsync(handle, localeCode, ct)
                : await handle.ToUniTask(cancellationToken: ct);

            if (fontRef == null)
            {
                // content 실패는 LoadContentOrNullAsync가 이미 로그로 표면화했다. boot 티어는 Local 그룹이라
                // 실패가 원격 장애일 수 없고 구성 오류이므로 단언으로 드러낸다.
                Debug.Assert(tier == FontTier.Content, $"Asset Table '{TableName}'에 locale '{localeCode}' 엔트리 '{entryKey}' FontRef가 없습니다.");
                return null;
            }

            return new FontSet(fontRef.DefaultFont, fontRef.GlobalFallback, fontRef.StyleSheet, fontRef.Presets);
        }

        /// <summary>
        /// content 티어 핸들을 await하되 원격 로드 실패를 null로 흡수하고 오류를 표면화한다.
        /// 취소는 부팅 흐름 제어 신호이므로 그대로 전파한다.
        /// </summary>
        /// <param name="handle">await할 로드 핸들.</param>
        /// <param name="localeCode">오류 메시지에 실을 locale 코드.</param>
        /// <param name="ct">취소 토큰.</param>
        private static async UniTask<FontRef> LoadContentOrNullAsync(AsyncOperationHandle<FontRef> handle, string localeCode, CancellationToken ct)
        {
            try
            {
                return await handle.ToUniTask(cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Polyglot] locale '{localeCode}' content 폰트 로드 실패 — boot 티어를 기능적 바닥으로 유지합니다: {e.Message}");
                return null;
            }
        }
    }
}
