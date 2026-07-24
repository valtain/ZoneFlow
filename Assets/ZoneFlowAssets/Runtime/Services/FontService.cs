using Cysharp.Threading.Tasks;
using Polyglot;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>
    /// Polyglot 폰트 엔진을 부팅하는 서비스. Polyglot 타입만 참조하며
    /// TMPro·Localization API에는 직접 접근하지 않는다(격리는 Polyglot 내부 facade가 담당).
    /// </summary>
    public sealed class FontService : MonoService<FontService>
    {
        // PlayerPrefs 키. IntroScreen의 언어 피커가 실제로 선택을 완료했을 때만 기록된다.
        // Localization의 PlayerPrefLocaleSelector가 초기화마다 자동 기록하는 selected-locale과는
        // 별도 — 이 키만으로 first-run(피커 표시 여부)을 판정한다.
        private const string PickerShownKey = "language-picker-shown";
        private const string LocaleKey = "selected-locale";

        /// <summary>locale 코드 → 폰트 세트 매핑을 보유한 카탈로그.</summary>
        [field: SerializeField] public FontCatalog Catalog { get; private set; }

        private readonly TmpFontFacade _facade = new();

        /// <summary>언어 피커에서 locale 선택을 완료한 적이 있는지 여부(first-run 게이트).</summary>
        public bool HasLocaleBeenChosen => PlayerPrefs.HasKey(PickerShownKey);

        /// <summary>
        /// 활성 locale에 대응하는 폰트 세트를 로드해 TMP에 적용한다(부팅 1회).
        /// 폰트 로드는 <see cref="AddressablesFontProvider"/>(Localization Asset Table) 경유.
        /// <see cref="Catalog"/>가 미배정이면 폰트 시스템 미구성으로 보고 skip한다.
        /// </summary>
        public async UniTask BootAsync()
        {
            if (Catalog == null)
            {
                Debug.Log("[FontService] FontCatalog 미배정 — 폰트 부팅 skip");
                return;
            }

            var provider = new AddressablesFontProvider();
            var engine = new FontEngine(provider, _facade);
            await engine.BootAsync(destroyCancellationToken);
        }

        /// <summary>
        /// localeCode를 활성 locale로 선택·영속화하고 해당 locale의 폰트 세트를 재부팅한다.
        /// 언어 피커의 선택 확정 시 호출한다.
        /// </summary>
        public async UniTask SelectLocaleAsync(string localeCode)
        {
            _facade.SetActiveLocale(localeCode);
            PlayerPrefs.SetString(LocaleKey, localeCode);
            PlayerPrefs.SetInt(PickerShownKey, 1);
            PlayerPrefs.Save();
            await BootAsync();
        }

        /// <summary>
        /// 언어 피커 자기 라벨(네이티브 표기) 렌더용으로 <see cref="Catalog"/>의 모든 폰트를
        /// TMP 전역 fallback으로 임시 적용한다. 이후 <see cref="SelectLocaleAsync"/>가 정규
        /// per-locale fallback으로 덮어써 되돌린다.
        /// </summary>
        public void ApplyPickerFallbacks()
        {
            Debug.Assert(Catalog != null, "[FontService] FontCatalog 미배정입니다.");
            _facade.SetFallbacks(Catalog.AllFonts());
        }
    }
}
