using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace Polyglot.Editor.Tests
{
    /// <summary>FontEngine.BootAsync의 부팅 결정론(조회 순서·적용 대상)을 fake seam으로 검증한다.</summary>
    internal class FontEngineTests
    {
        private class FakeFontProvider : IFontProvider
        {
            public string RequestedLocale;
            public FontTier RequestedTier;
            public FontSet ToReturn;
            public int LoadCount;

            public UniTask<FontSet> LoadAsync(string localeCode, FontTier tier, CancellationToken ct)
            {
                RequestedLocale = localeCode;
                RequestedTier = tier;
                LoadCount++;
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

            new FontEngine(provider, facade).BootAsync(FontTier.Content, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual("ja", provider.RequestedLocale);
            Assert.AreEqual(FontTier.Content, provider.RequestedTier);
            Assert.AreSame(provider.ToReturn, facade.AppliedSet);
        }

        /// <summary>활성 locale이 없으면(Scene Controls의 None 등) 폰트 로드·적용을 모두 건너뛴다.</summary>
        [Test]
        public void BootAsync_WithoutActiveLocale_SkipsLoadAndApply()
        {
            var provider = new FakeFontProvider
            {
                ToReturn = new FontSet(null, new TMP_FontAsset[0], null)
            };
            var facade = new FakeFontFacade
            {
                LocaleToReturn = null
            };

            new FontEngine(provider, facade).BootAsync(FontTier.Content, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(0, provider.LoadCount);
            Assert.IsNull(facade.AppliedSet);
        }

        /// <summary>
        /// provider가 null을 반환하면(원격 content 로드 실패) 오류를 표면화하고 Apply를 건너뛴다 —
        /// TMP 상태가 그대로 남아 직전 티어(boot)가 기능적 바닥으로 유지되는 것이 floor 불변식이다.
        /// </summary>
        [Test]
        public void BootAsync_WhenLoadFails_SurfacesErrorAndSkipsApply()
        {
            var provider = new FakeFontProvider
            {
                ToReturn = null
            };
            var facade = new FakeFontFacade
            {
                LocaleToReturn = "ko"
            };

            LogAssert.Expect(LogType.Error, "[Polyglot] locale 'ko' Content 티어 폰트 로드 실패 — 직전 폰트 상태를 유지합니다");

            new FontEngine(provider, facade).BootAsync(FontTier.Content, CancellationToken.None).GetAwaiter().GetResult();

            Assert.AreEqual(1, provider.LoadCount);
            Assert.IsNull(facade.AppliedSet);
        }
    }
}
