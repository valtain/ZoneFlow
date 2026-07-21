namespace Polyglot
{
    /// <summary>TMP·Localization API 접점을 격리하는 facade 계약. 구현체는 TMPro.*·UnityEngine.Localization.* 직접 호출을 이 한 곳에 가둔다.</summary>
    public interface IFontFacade
    {
        /// <summary>Localization 선택 locale 코드를 조회한다(미선택 시 기본 locale).</summary>
        string GetActiveLocaleCode();

        /// <summary>TMP_Settings 기본 폰트·전역 fallback·활성 스타일시트를 적용한다(부팅 1회).</summary>
        /// <param name="fontSet">적용할 폰트 세트.</param>
        void Apply(FontSet fontSet);
    }
}
