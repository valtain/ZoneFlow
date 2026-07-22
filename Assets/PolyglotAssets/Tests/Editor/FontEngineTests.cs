using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TMPro;

namespace Polyglot.Editor.Tests
{
    /// <summary>FontEngine.BootAsync의 부팅 결정론(조회 순서·적용 대상)을 fake seam으로 검증한다.</summary>
    internal class FontEngineTests
    {
        private class FakeFontProvider : IFontProvider
        {
            public string RequestedLocale;
            public FontSet ToReturn;

            public UniTask<FontSet> LoadAsync(string localeCode, CancellationToken ct)
            {
                RequestedLocale = localeCode;
                return UniTask.FromResult(ToReturn);
            }
        }

        private class FakeFontFacade : IFontFacade
        {
            public string LocaleToReturn;
            public FontSet AppliedSet;

            public string GetActiveLocaleCode() => LocaleToReturn;

            public void Apply(FontSet fontSet)
            {
                AppliedSet = fontSet;
            }
        }

        /// <summary>BootAsync는 facade가 반환한 locale 코드로 provider를 조회하고, provider가 반환한 그 FontSet을 facade에 적용한다.</summary>
        [Test]
        public void BootAsync_UsesFacadeLocale_AndAppliesProviderResult()
        {
            var provider = new FakeFontProvider
            {
                ToReturn = new FontSet(null, new TMP_FontAsset[0], null)
            };
            var facade = new FakeFontFacade
            {
                LocaleToReturn = "ja"
            };

            new FontEngine(provider, facade).BootAsync(CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("ja", provider.RequestedLocale);
            Assert.AreSame(provider.ToReturn, facade.AppliedSet);
        }
    }
}
