using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ZoneFlow.Tests.Runtime
{
    /// <summary>Intro → Menu → Village → Dungeon 씬 기반 내비게이션 기본 동작 검증.</summary>
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
        public IEnumerator Navigate_IntroToDungeon_ModeTransitions() =>
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

                // Village: ExplorationMode, ReplaceAll → Stack=1 (Intro+Menu 모두 해제)
                await d.NavigateAsync("gameplay://exploration/village?switch=replaceall", ct);
                AssertMode<ExplorationMode>("Village", d, expectedStack: 1);

                // Dungeon: ExplorationMode, Replace → Stack=1
                await d.NavigateAsync("gameplay://exploration/dungeon_0", ct);
                AssertMode<ExplorationMode>("Dungeon", d, expectedStack: 1);
            });

        /// <summary>
        /// village → dungeon → village 존 사이클 내비게이션 검증.
        /// 각 단계에서 올바른 Zone이 활성화되어 있는지 확인한다 (씬 단위 load/unload).
        /// </summary>
        [UnityTest]
        public IEnumerator Navigate_ZoneCycle_Village_Dungeon_Village() =>
            UniTask.ToCoroutine(async () =>
            {
                var d = GamePlayDirector.Instance;
                var ct = CancellationToken.None;

                // village 진입
                await d.NavigateAsync("gameplay://exploration/village?switch=replaceall", ct);
                AssertMode<ExplorationMode>("Village", d, expectedStack: 1);
                Assert.AreEqual("village", FindActiveZone()?.ZoneId, "[Village] ZoneId 불일치");

                // dungeon 진입 (다른 씬)
                await d.NavigateAsync("gameplay://exploration/dungeon_0", ct);
                AssertMode<ExplorationMode>("Dungeon", d, expectedStack: 1);
                Assert.AreEqual("dungeon_0", FindActiveZone()?.ZoneId, "[Dungeon] ZoneId 불일치");

                // village 복귀 (다시 다른 씬)
                await d.NavigateAsync("gameplay://exploration/village?switch=replaceall", ct);
                AssertMode<ExplorationMode>("Village (복귀)", d, expectedStack: 1);
                Assert.AreEqual("village", FindActiveZone()?.ZoneId, "[Village 복귀] ZoneId 불일치");
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
