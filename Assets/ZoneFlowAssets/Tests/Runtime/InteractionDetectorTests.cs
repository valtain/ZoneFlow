using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace ZoneFlow.Tests.Runtime
{
    /// <summary>
    /// InteractionDetector 근접 최근접 추적 동작 검증.
    /// interaction-prompt testcases 1·2·3(사정권 진입·이탈·최근접 선택)의 로직 토대를 자동 검증한다.
    /// 프롬프트 표시(페이드)·폴백 텍스트·가독성(4·5)은 testcases.md의 수동 항목으로 확인한다.
    /// </summary>
    public class InteractionDetectorTests
    {
        private readonly List<GameObject> _spawned = new();

        // 디텍터 _scanInterval(0.1s)보다 길게 대기해 최소 1회 스캔을 보장한다.
        private const float ScanWait = 0.2f;

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _spawned)
                if (go != null) Object.Destroy(go);
            _spawned.Clear();
        }

        private InteractionDetector CreateDetector(Vector3 pos)
        {
            var go = new GameObject("Detector") { transform = { position = pos } };
            _spawned.Add(go);
            return go.AddComponent<InteractionDetector>();
        }

        private FakeInteractable CreateInteractable(string id, string label, Vector3 pos)
        {
            var go = new GameObject($"Interactable_{id}") { transform = { position = pos } };
            go.AddComponent<SphereCollider>();
            var fake = go.AddComponent<FakeInteractable>();
            fake.Configure(id, label);
            _spawned.Add(go);
            return fake;
        }

        /// <summary>사정권(기본 반경 4) 내에 들어온 대상을 Current로 추적하고 1회 통지한다. (testcase 1)</summary>
        [UnityTest]
        public IEnumerator Detector_DetectsInteractableInRange()
        {
            var detector = CreateDetector(Vector3.zero);
            var target = CreateInteractable("portal_a", "숲 입구", new Vector3(0f, 0f, 2f));

            IInteractable lastNotified = null;
            int notifyCount = 0;
            detector.NearestChanged += n => { lastNotified = n; notifyCount++; };

            Physics.SyncTransforms();
            yield return new WaitForSeconds(ScanWait);

            Assert.AreSame(target, detector.Current, "사정권 내 대상이 Current로 추적되어야 함");
            Assert.AreSame(target, lastNotified, "NearestChanged가 대상으로 통지되어야 함");
            Assert.AreEqual(1, notifyCount, "진입 시 1회만 통지되어야 함");
        }

        /// <summary>사정권을 벗어나면 Current가 null이 되고 null로 통지한다. (testcase 2)</summary>
        [UnityTest]
        public IEnumerator Detector_ClearsWhenOutOfRange()
        {
            var detector = CreateDetector(Vector3.zero);
            var target = CreateInteractable("portal_a", "숲 입구", new Vector3(0f, 0f, 2f));

            Physics.SyncTransforms();
            yield return new WaitForSeconds(ScanWait);
            Assert.AreSame(target, detector.Current, "사전 조건: 사정권 내 추적 중");

            IInteractable lastNotified = target;
            detector.NearestChanged += n => lastNotified = n;

            target.transform.position = new Vector3(0f, 0f, 20f); // 반경(4) 밖
            Physics.SyncTransforms();
            yield return new WaitForSeconds(ScanWait);

            Assert.IsNull(detector.Current, "사정권 이탈 시 Current는 null이어야 함");
            Assert.IsNull(lastNotified, "사정권 이탈 시 NearestChanged(null)로 통지되어야 함");
        }

        /// <summary>여러 대상이면 최근접만 선택하고, 더 가까워진 대상으로 전환한다. (testcase 3)</summary>
        [UnityTest]
        public IEnumerator Detector_SelectsNearest_AndSwitchesOnMove()
        {
            var detector = CreateDetector(Vector3.zero);
            var far = CreateInteractable("far", "먼 것", new Vector3(0f, 0f, 3f));
            var near = CreateInteractable("near", "가까운 것", new Vector3(0f, 0f, 1f));

            Physics.SyncTransforms();
            yield return new WaitForSeconds(ScanWait);
            Assert.AreSame(near, detector.Current, "여러 대상 중 최근접이 선택되어야 함");

            // far를 더 가깝게 이동 → 최근접 전환
            far.transform.position = new Vector3(0f, 0f, 0.3f);
            Physics.SyncTransforms();
            yield return new WaitForSeconds(ScanWait);
            Assert.AreSame(far, detector.Current, "더 가까워진 대상으로 최근접이 전환되어야 함");
        }

        /// <summary>테스트용 IInteractable 더블.</summary>
        private sealed class FakeInteractable : MonoBehaviour, IInteractable
        {
            public string InteractableId { get; private set; }
            public string DisplayLabel { get; private set; }

            public void Configure(string id, string label)
            {
                InteractableId = id;
                DisplayLabel = label;
            }

            public UniTask OnInteractAsync(GamePlayDirector director, CancellationToken ct)
                => UniTask.CompletedTask;
        }
    }
}
