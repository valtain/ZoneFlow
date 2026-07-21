using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Polyglot
{
    /// <summary>부팅 시 1회 활성 locale을 조회 → 폰트 세트를 로드 → TMP에 적용하는 부팅 엔진. 런타임 재적용(swap)은 하지 않는다.</summary>
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

        /// <summary>활성 locale을 조회해 대응 폰트 세트를 로드하고 TMP_Settings에 적용한다(부팅 1회).</summary>
        /// <param name="ct">취소 토큰.</param>
        public async UniTask BootAsync(CancellationToken ct)
        {
            string localeCode = _facade.GetActiveLocaleCode();
            Debug.Assert(!string.IsNullOrEmpty(localeCode), "활성 locale 코드를 확인할 수 없습니다.");

            FontSet fontSet = await _provider.LoadAsync(localeCode, ct);
            _facade.Apply(fontSet);
        }
    }
}
