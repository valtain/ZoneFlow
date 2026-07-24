using System;

namespace Polyglot
{
    /// <summary>
    /// 마지막으로 적용된 <see cref="FontSet"/>을 보관하고 적용 사실을 알리는 전역 신호.
    /// <see cref="PolyglotText"/>가 이를 구독해 스스로 폰트를 갱신한다(에디트 모드 포함).
    /// </summary>
    public static class FontRuntime
    {
        /// <summary>현재 적용된 폰트 세트. 아직 적용 전이면 null.</summary>
        public static FontSet Current { get; private set; }

        /// <summary>폰트 세트가 적용될 때마다 발생한다.</summary>
        public static event Action<FontSet> Applied;

        /// <summary>폰트 세트 적용을 기록하고 구독자에게 알린다. <see cref="TmpFontFacade"/>가 호출한다.</summary>
        /// <param name="fontSet">적용된 폰트 세트.</param>
        internal static void RaiseApplied(FontSet fontSet)
        {
            Current = fontSet;
            Applied?.Invoke(fontSet);
        }
    }
}
