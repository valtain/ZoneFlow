namespace Polyglot
{
    /// <summary>폰트 로딩 단계(티어). 부팅 시점과 콘텐츠 진입 시점의 폰트 세트를 구분한다.</summary>
    public enum FontTier
    {
        /// <summary>부팅 직후 즉시 렌더가 필요한 최소 폰트 세트(Intro·메뉴·피커용 per-locale 서브셋).</summary>
        Boot,

        /// <summary>콘텐츠 진입 후 사용하는 전체 폰트 세트.</summary>
        Content
    }
}
