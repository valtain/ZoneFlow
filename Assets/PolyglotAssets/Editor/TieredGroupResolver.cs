using System;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Localization.Addressables;
using UnityEngine.Localization;
using Object = UnityEngine.Object;

namespace Polyglot.Editor
{
    /// <summary>
    /// boot 티어 폰트 자산을 locale 그룹과 <b>다른</b> Addressables 그룹으로 보내는 <see cref="GroupResolver"/>.
    /// Localization이 기본 제공하는 resolver는 locale로만 그룹을 가르기 때문에 boot·content FontRef가 한 그룹
    /// (=PackTogether이므로 한 번들)에 묶이고, 그 결과 content 폰트의 소스 TTF까지 boot과 같은 배포 단위로 나간다
    /// — 티어를 나눈 목적(로컬 동봉량 최소화)이 무효화된다. 이 resolver가 그 경계를 복원한다.
    /// </summary>
    /// <remarks>
    /// Localization은 그룹 배치를 소유하며 엔트리를 read-only로 잠그므로 그룹을 손으로 옮기면 재정렬 때
    /// 되돌아간다. 그래서 패키지가 열어둔 유일한 확장점인 <see cref="GetExpectedGroupName"/>을 사용한다.
    /// 이 훅은 테이블 정보를 받지 않으므로(에셋과 locale만 받는다) <c>font</c>·<c>font-boot</c>가 같은
    /// <c>Fonts</c> 테이블에 있다는 사실과 무관하게 동작하며, 판별 기준은 에셋 이름뿐이다.
    /// </remarks>
    [Serializable]
    public sealed class TieredGroupResolver : GroupResolver
    {
        /// <summary>boot 티어 FontRef 에셋의 이름 접두사. 이 접두사가 티어 판별자다.</summary>
        public const string BootAssetPrefix = "BootFontRef_";

        /// <summary>boot 티어 자산이 들어갈 그룹 이름에 덧붙는 접미사(예: <c>Localization-Assets-ko-Boot</c>).</summary>
        public const string BootGroupSuffix = "-Boot";

        /// <summary>기본 인스턴스를 생성한다. <c>[SerializeReference]</c> 역직렬화가 요구한다.</summary>
        public TieredGroupResolver()
        {
        }

        /// <summary>base와 동일한 그룹 이름 규칙을 쓰되 locale 그룹 이름에 <see cref="BootGroupSuffix"/>를 덧붙인다.</summary>
        /// <param name="localeGroupNamePattern">locale별 그룹 이름 패턴.</param>
        /// <param name="sharedGroupName">여러 locale이 공유하는 자산이 들어갈 그룹 이름.</param>
        public TieredGroupResolver(string localeGroupNamePattern, string sharedGroupName)
            : base(localeGroupNamePattern, sharedGroupName)
        {
        }

        /// <summary>
        /// 자산이 들어갈 그룹 이름을 반환한다. boot 티어 자산이면 base가 고른 locale 그룹 이름에
        /// <see cref="BootGroupSuffix"/>를 붙여 별도 그룹으로 보내고, 그 외에는 base 판단을 그대로 따른다.
        /// </summary>
        /// <param name="locales">이 자산에 의존하는 locale 목록(모든 locale이 쓰면 null).</param>
        /// <param name="asset">그룹에 배치할 자산.</param>
        /// <param name="aaSettings">그룹을 조회·생성할 Addressables 설정.</param>
        public override string GetExpectedGroupName(IList<LocaleIdentifier> locales, Object asset, AddressableAssetSettings aaSettings)
        {
            string groupName = base.GetExpectedGroupName(locales, asset, aaSettings);
            if (!IsBootTierAsset(asset))
            {
                return groupName;
            }

            // base가 shared 그룹을 고른 경우(= locale 간 공유 자산)는 티어보다 공유가 우선이므로 건드리지 않는다.
            // boot FontRef는 locale 전용이라 실제로는 이 경로로 오지 않지만, base의 판단을 덮지 않는다는 계약을 지킨다.
            string sharedGroupName = GetExpectedSharedGroupName(locales, asset, aaSettings);
            return groupName == sharedGroupName ? groupName : groupName + BootGroupSuffix;
        }

        /// <summary>자산이 boot 티어에 속하는지 이름 접두사로 판별한다.</summary>
        /// <param name="asset">판별할 자산.</param>
        public static bool IsBootTierAsset(Object asset)
        {
            return asset != null && asset.name.StartsWith(BootAssetPrefix, StringComparison.Ordinal);
        }
    }
}
