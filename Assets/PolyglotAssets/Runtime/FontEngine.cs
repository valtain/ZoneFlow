using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Polyglot
{
    /// <summary>활성 locale을 조회 → 폰트 세트를 로드 → TMP에 적용하는 부팅 엔진. locale 변경 시 <see cref="BootAsync"/> 재호출로 재적용한다(per-component 폰트 swap은 하지 않음 — ADR-0008).</summary>
    public sealed class FontEngine
    {
        private readonly IFontProvider _provider;
        private readonly IFontFacade _facade;

        /// <summary>폰트 로드 담당 프로바이더와 locale 조회·TMP 적용 담당 facade를 주입받아 인스턴스를 생성한다.</summary>
        /// <param name="provider">locale별 폰트 세트를 로드하는 프로바이더.</param>
        /// <param name="facade">TMP·Localization API 접점을 격리하는 facade.</param>
        public FontEngine(IFontProvider provider, IFontFacade facade)
        {
            Debug.Assert(provider != null);
            Debug.Assert(facade != null);
            _provider = provider;
            _facade = facade;
        }

        /// <summary>
        /// 활성 locale을 조회해 대응 폰트 세트를 로드하고 TMP_Settings에 적용한다(부팅 1회).
        /// 로드에 실패하면 적용을 건너뛰어 직전 폰트 상태를 유지한다 — content 티어가 원격에서
        /// 내려오지 않아도 boot 티어가 기능적 바닥(floor)으로 남는다.
        /// </summary>
        /// <param name="tier">로드할 <see cref="FontTier"/>(Boot/Content).</param>
        /// <param name="ct">취소 토큰.</param>
        public async UniTask BootAsync(FontTier tier, CancellationToken ct)
        {
            string localeCode = _facade.GetActiveLocaleCode();
            if (string.IsNullOrEmpty(localeCode))
            {
                Debug.Log("[Polyglot] 활성 locale 없음 — 폰트 부팅 skip");
                return;
            }

            FontSet fontSet = await _provider.LoadAsync(localeCode, tier, ct);
            if (fontSet == null)
            {
                Debug.LogError($"[Polyglot] locale '{localeCode}' {tier} 티어 폰트 로드 실패 — 직전 폰트 상태를 유지합니다");
                return;
            }

            _facade.Apply(fontSet);
        }
    }
}
