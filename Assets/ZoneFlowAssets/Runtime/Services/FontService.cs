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

        /// <summary>
        /// boot 티어 폰트 카탈로그(피커 라벨 fallback 용).
        /// content 카탈로그를 배정하면 전체 폰트가 씬 직접 참조로 딸려와 Tier-1 절감이 무효화된다.
        /// </summary>
        [field: SerializeField] public FontCatalog BootCatalog { get; private set; }

        private readonly TmpFontFacade _facade = new();
        private bool _isSwitching;

        // 현재 적용 중인 티어. locale 전환은 BootAsync를 재호출하므로, 이 값을 기억하지 않으면
        // content 적용 후 언어를 바꾸는 순간 boot 서브셋으로 되돌아가 본편 텍스트가 tofu가 된다.
        private FontTier _currentTier = FontTier.Boot;

        /// <summary>언어 피커에서 locale 선택을 완료한 적이 있는지 여부(first-run 게이트).</summary>
        public bool HasLocaleBeenChosen => PlayerPrefs.HasKey(PickerShownKey);

        /// <summary>
        /// Localization의 현재 선택 locale 코드. Localization API 직접 접근 대신 이 프로퍼티를 경유한다.
        /// 미선택 상태면 Project Locale 코드를 보고한다.
        /// </summary>
        public string CurrentLocaleCode => _facade.GetActiveLocaleCode();

        /// <summary>
        /// 활성 locale에 대응하는 현재 티어의 폰트 세트를 로드해 TMP에 적용한다.
        /// 티어는 boot으로 시작하며 <see cref="BootContentAsync"/>가 content로 승격한다.
        /// 폰트 로드는 <see cref="AddressablesFontProvider"/>(Localization Asset Table) 경유.
        /// <see cref="BootCatalog"/>가 미배정이면 폰트 시스템 미구성으로 보고 skip한다.
        /// </summary>
        public async UniTask BootAsync()
        {
            if (BootCatalog == null)
            {
                Debug.Log("[FontService] BootFontCatalog 미배정 — 폰트 부팅 skip");
                return;
            }

            // 폰트는 Addressables(Localization Asset Table) 경유 → 로드 전 초기화 완료를 보장한다.
            Debug.Assert(AddressableService.IsReady, "[FontService] CoreServices에 AddressableService가 없습니다.");
            await AddressableService.Instance.EnsureInitializedAsync();

            var provider = new AddressablesFontProvider();
            var engine = new FontEngine(provider, _facade);
            await engine.BootAsync(_currentTier, destroyCancellationToken);
        }

        /// <summary>
        /// content 티어 폰트 세트를 로드해 적용하고, 이후 부팅의 기본 티어를 content로 승격한다.
        /// Intro 이후·게임 시작 직전(<c>MenuPanel</c>의 새 게임 진입)에 1회 호출한다.
        /// 승격 후에는 <see cref="SelectLocaleAsync"/>의 재부팅도 content 티어를 유지한다.
        /// <para>
        /// content 티어는 원격 그룹이라 오프라인·호스팅 장애로 실패할 수 있다. 이때 TMP 상태는 손대지 않아
        /// boot 티어가 기능적 바닥(floor)으로 남으며, 티어 승격 자체는 되돌리지 않으므로 이후
        /// <see cref="SelectLocaleAsync"/> 호출이 content 로드를 재시도한다.
        /// </para>
        /// </summary>
        public async UniTask BootContentAsync()
        {
            _currentTier = FontTier.Content;
            await BootAsync();
        }

        /// <summary>
        /// localeCode를 활성 locale로 선택·영속화하고 해당 locale의 폰트 세트를 재부팅한다.
        /// 언어 피커의 선택 확정 시 호출한다.
        /// </summary>
        public async UniTask SelectLocaleAsync(string localeCode)
        {
            // 전환 도중 연타로 재진입하면 폰트 리부트가 겹쳐 상태가 꼬일 수 있어 가드한다.
            if (_isSwitching)
            {
                return;
            }

            _isSwitching = true;
            try
            {
                _facade.SetActiveLocale(localeCode);
                PlayerPrefs.SetString(LocaleKey, localeCode);
                PlayerPrefs.SetInt(PickerShownKey, 1);
                PlayerPrefs.Save();
                await BootAsync();
            }
            finally
            {
                _isSwitching = false;
            }
        }

        /// <summary>
        /// 언어 피커 자기 라벨(네이티브 표기) 렌더용으로 <see cref="BootCatalog"/>의 모든 폰트를
        /// TMP 전역 fallback으로 임시 적용한다. 이후 <see cref="SelectLocaleAsync"/>가 정규
        /// per-locale fallback으로 덮어써 되돌린다.
        /// </summary>
        public void ApplyPickerFallbacks()
        {
            Debug.Assert(BootCatalog != null, "[FontService] BootFontCatalog 미배정입니다.");
            _facade.SetFallbacks(BootCatalog.AllFonts());
        }
    }
}
