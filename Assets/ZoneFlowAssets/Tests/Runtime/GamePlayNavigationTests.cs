using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ZoneFlow.Tests.Runtime
{
    /// <summary>Intro → Menu → World1 → World2 씬 기반 내비게이션 기본 동작 검증.</summary>
    public class GamePlayNavigationTests
    {
        [UnitySetUp]
        public IEnumerator SetUp() => UniTask.ToCoroutine(async () =>
        {
            await SceneService.EnsureCoreServicesLoaded();
        });

        [UnityTearDown]
        public IEnumerator TearDown() => UniTask.ToCoroutine(async () =>
        {
            // CoreServices 외 로드된 씬(Zone 씬 등) 언로드
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.name != "CoreServices" && scene.isLoaded)
                    await SceneManager.UnloadSceneAsync(scene).ToUniTask();
            }

            if (SceneManager.GetSceneByName("CoreServices").isLoaded)
                await SceneManager.UnloadSceneAsync("CoreServices").ToUniTask();
        });

        [UnityTest]
        public IEnumerator Navigate_IntroToWorld2_ModeTransitions() =>
            UniTask.ToCoroutine(async () =>
            {
                var d = GamePlayDirector.Instance;
                var ct = CancellationToken.None;

                // Intro: ShellMode, Zone=intro, id=null, Replace → Stack=1
                await d.NavigateAsync("gameplay://shell/intro", ct);
                AssertMode<ShellMode>("Intro", d, expectedStack: 1);
                Assert.IsNull(((ShellMode)d.ActiveMode).PanelId, "[Intro] PanelId null 기대");

                // Menu: ShellMode, no zone, Stack → Stack=2
                await d.NavigateAsync("gameplay://shell?id=menu&switch=stack", ct);
                AssertMode<ShellMode>("Menu", d, expectedStack: 2);
                Assert.AreEqual("menu", ((ShellMode)d.ActiveMode).PanelId, "[Menu] PanelId");

                // World1: ExplorationMode, ReplaceAll → Stack=1 (Intro+Menu 모두 해제)
                await d.NavigateAsync("gameplay://exploration/world1?switch=replaceall", ct);
                AssertMode<ExplorationMode>("World1", d, expectedStack: 1);

                // World2: ExplorationMode, Replace → Stack=1
                await d.NavigateAsync("gameplay://exploration/world2", ct);
                AssertMode<ExplorationMode>("World2", d, expectedStack: 1);
            });

        private static void AssertMode<T>(string step, GamePlayDirector d, int expectedStack)
            where T : GamePlayMode
        {
            Assert.IsInstanceOf<T>(d.ActiveMode,        $"[{step}] {typeof(T).Name} 기대");
            Assert.AreEqual(ModeState.Active, d.ActiveMode.State, $"[{step}] State=Active 기대");
            Assert.AreEqual(expectedStack, d.ModeStack.Count,     $"[{step}] Stack={expectedStack} 기대");
        }
    }
}
