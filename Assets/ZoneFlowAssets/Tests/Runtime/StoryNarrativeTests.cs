using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ZoneFlow.Tests.Runtime
{
    /// <summary>
    /// AQ-2 검증: Story 진행 상태($story_progress)가 Zone 전환(Zone 씬 unload/load) 후에도 보존되는지 확인한다.
    /// 진행 상태는 ContentServices 씬의 DialogueService(DialogueRunner+변수 저장소)가 소유하므로,
    /// Zone 씬이 바뀌어도 ContentServices가 살아있는 한 유지된다.
    /// </summary>
    public class StoryNarrativeTests
    {
        private const string StoryProgress = "$story_progress";

        [UnitySetUp]
        public IEnumerator SetUp() => UniTask.ToCoroutine(async () =>
        {
            await SceneService.EnsureCoreServicesLoaded();
            await SceneService.EnsureContentServicesLoaded();
        });

        [UnityTearDown]
        public IEnumerator TearDown() => UniTask.ToCoroutine(async () =>
        {
            // CoreServices 외 로드된 씬(ContentServices, Zone 씬 등) 언로드
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
        public IEnumerator StoryProgress_PersistsAcrossZoneTransition() =>
            UniTask.ToCoroutine(async () =>
            {
                var d = GamePlayDirector.Instance;
                var ct = CancellationToken.None;

                Assert.IsTrue(DialogueService.IsReady,
                    "ContentServices 로드 후 DialogueService.Instance가 유효해야 한다.");

                // intro 노드 실행 → Yarn <<set $story_progress = 1>> 검증
                DialogueService.Instance.StartDialogue("intro");
                await DialogueService.Instance.AwaitDialogueAsync();

                Assert.IsTrue(DialogueService.Instance.TryGetFloat(StoryProgress, out var afterIntro),
                    "intro 실행 후 $story_progress가 존재해야 한다.");
                Assert.AreEqual(1f, afterIntro, "intro 실행 후 $story_progress=1 이어야 한다.");

                // Story 모드로 village 진입 후 dungeon_0으로 Zone 전환 (Zone 씬 unload/load)
                await d.NavigateAsync("gameplay://story/village?switch=replaceall", ct);
                AssertStoryMode("Village 진입", d);

                await d.NavigateAsync("gameplay://story/dungeon_0", ct);
                AssertStoryMode("Dungeon 전환", d);

                // 핵심: Zone 전환 후에도 진행 상태가 보존되어야 한다 (AQ-2)
                Assert.IsTrue(DialogueService.Instance.TryGetFloat(StoryProgress, out var afterTransition),
                    "Zone 전환 후에도 $story_progress가 존재해야 한다.");
                Assert.AreEqual(1f, afterTransition,
                    "Zone 전환(씬 unload/load)을 거쳐도 $story_progress가 보존되어야 한다.");

                // 전환 후 zone_a 노드 실행 → 보존된 값 위에서 진행 (1 → 2)
                DialogueService.Instance.StartDialogue("zone_a");
                await DialogueService.Instance.AwaitDialogueAsync();

                Assert.IsTrue(DialogueService.Instance.TryGetFloat(StoryProgress, out var afterZoneA),
                    "zone_a 실행 후 $story_progress가 존재해야 한다.");
                Assert.AreEqual(2f, afterZoneA,
                    "zone_a는 보존된 진행 상태(1) 위에서 이어져 2가 되어야 한다.");
            });

        private static void AssertStoryMode(string step, GamePlayDirector d)
        {
            Assert.IsInstanceOf<StoryMode>(d.ActiveMode, $"[{step}] StoryMode 기대");
            Assert.AreEqual(ModeState.Active, d.ActiveMode.State, $"[{step}] State=Active 기대");
            Assert.AreEqual(1, d.ModeStack.Count, $"[{step}] Stack=1 기대");
        }
    }
}
