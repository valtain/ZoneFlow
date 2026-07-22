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
        /// <summary>locale 코드 → 폰트 세트 매핑을 보유한 카탈로그.</summary>
        [field: SerializeField] public FontCatalog Catalog { get; private set; }

        /// <summary>
        /// 활성 locale에 대응하는 폰트 세트를 로드해 TMP에 적용한다(부팅 1회).
        /// <see cref="Catalog"/>가 미배정이면 skip한다.
        /// </summary>
        public async UniTask BootAsync()
        {
            if (Catalog == null)
            {
                Debug.Log("[FontService] FontCatalog 미배정 — 폰트 부팅 skip");
                return;
            }

            var provider = new DirectRefFontProvider(Catalog);
            var facade = new TmpFontFacade();
            var engine = new FontEngine(provider, facade);
            await engine.BootAsync(destroyCancellationToken);
        }
    }
}
