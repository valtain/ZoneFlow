using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
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

                // Intro: ShellMode, Zone=intro, Replace → Stack=1
                await d.NavigateAsync("gameplay://shell/intro", ct);
                AssertMode<ShellMode>("Intro", d, expectedStack: 1);

                // Menu: PanelMode, no zone, Stack → Stack=2
                await d.NavigateAsync("gameplay://panel?id=menu&switch=stack", ct);
                AssertMode<PanelMode>("Menu", d, expectedStack: 2);
                Assert.AreEqual("menu", ((PanelMode)d.ActiveMode).PanelId, "[Menu] PanelId");

                // World1: ExplorationMode, ReplaceAll → Stack=1 (Intro+Menu 모두 해제)
                await d.NavigateAsync("gameplay://exploration/world1?switch=replaceall", ct);
                AssertMode<ExplorationMode>("World1", d, expectedStack: 1);

                // World2: ExplorationMode, Replace → Stack=1
                await d.NavigateAsync("gameplay://exploration/world2", ct);
                AssertMode<ExplorationMode>("World2", d, expectedStack: 1);
            });

        /// <summary>
        /// w1 → w1_b → w2_b → w2 → w1 존 사이클 내비게이션 검증.
        /// 각 단계에서 올바른 Zone이 활성화되어 있는지 확인한다.
        /// </summary>
        [UnityTest]
        public IEnumerator Navigate_ZoneCycle_W1_W1b_W2b_W2_W1() =>
            UniTask.ToCoroutine(async () =>
            {
                var d = GamePlayDirector.Instance;
                var ct = CancellationToken.None;

                // world1 진입
                await d.NavigateAsync("gameplay://exploration/world1?switch=replaceall", ct);
                AssertMode<ExplorationMode>("World1", d, expectedStack: 1);
                Assert.AreEqual("world1", FindActiveZone()?.ZoneId, "[World1] ZoneId 불일치");

                // world1_b 진입 (같은 씬 내 다른 Zone)
                await d.NavigateAsync("gameplay://exploration/world1_b", ct);
                AssertMode<ExplorationMode>("World1_b", d, expectedStack: 1);
                Assert.AreEqual("world1_b", FindActiveZone()?.ZoneId, "[World1_b] ZoneId 불일치");

                // world2_b 진입 (다른 씬)
                await d.NavigateAsync("gameplay://exploration/world2_b", ct);
                AssertMode<ExplorationMode>("World2_b", d, expectedStack: 1);
                Assert.AreEqual("world2_b", FindActiveZone()?.ZoneId, "[World2_b] ZoneId 불일치");

                // world2 진입 (같은 씬 내 다른 Zone)
                await d.NavigateAsync("gameplay://exploration/world2", ct);
                AssertMode<ExplorationMode>("World2", d, expectedStack: 1);
                Assert.AreEqual("world2", FindActiveZone()?.ZoneId, "[World2] ZoneId 불일치");

                // world1 복귀
                await d.NavigateAsync("gameplay://exploration/world1?switch=replaceall", ct);
                AssertMode<ExplorationMode>("World1 (복귀)", d, expectedStack: 1);
                Assert.AreEqual("world1", FindActiveZone()?.ZoneId, "[World1 복귀] ZoneId 불일치");
            });

        /// <summary>현재 씬에서 활성화된 Zone 중 첫 번째를 반환한다.</summary>
        private static Zone FindActiveZone()
        {
            var zones = Object.FindObjectsByType<Zone>(FindObjectsSortMode.None);
            return zones.Length > 0 ? zones[0] : null;
        }

        private static void AssertMode<T>(string step, GamePlayDirector d, int expectedStack)
            where T : GamePlayMode
        {
            Assert.IsInstanceOf<T>(d.ActiveMode,        $"[{step}] {typeof(T).Name} 기대");
            Assert.AreEqual(ModeState.Active, d.ActiveMode.State, $"[{step}] State=Active 기대");
            Assert.AreEqual(expectedStack, d.ModeStack.Count,     $"[{step}] Stack={expectedStack} 기대");
        }
    }
}
