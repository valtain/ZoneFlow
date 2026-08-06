using NUnit.Framework;
using UnityEngine;

namespace ZoneFlow.Tests.Runtime
{
    /// <summary>
    /// MonoService&lt;T&gt; 등록·중복·해제 시나리오 검증.
    /// Awake·OnDestroy는 Play Mode에만 호출되므로(MonoService에 [ExecuteAlways]가 없다)
    /// EditMode에서는 AddComponent가 등록을 일으키지 않는다 — 이 테스트는 Play Mode 전용이다.
    /// </summary>
    internal class MonoServiceTests
    {
        private class TestService : MonoService<TestService> { }

        private GameObject _go;

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
                Object.DestroyImmediate(_go);
        }

        /// <summary>씬에 배치된 구체 클래스가 Awake 시 Instance에 자동 등록된다.</summary>
        [Test]
        public void Instance_RegisteredOnAwake()
        {
            _go = new GameObject();
            _go.AddComponent<TestService>();

            Assert.IsNotNull(TestService.Instance);
            Assert.IsTrue(TestService.IsReady);
        }

        /// <summary>동일 씬에 두 번째 인스턴스가 Awake되면 Assert가 발생한다.</summary>
        [Test]
        public void Awake_DuplicateInstance_TriggersAssert()
        {
            _go = new GameObject();
            _go.AddComponent<TestService>();

            // Unity의 Debug.Assert는 기본적으로 예외를 던지지 않으므로
            // LogAssert로 Assert 로그 발생 여부를 검증한다.
            UnityEngine.TestTools.LogAssert.Expect(LogType.Assert, new System.Text.RegularExpressions.Regex("중복 인스턴스"));

            var go2 = new GameObject();
            go2.AddComponent<TestService>();
            Object.DestroyImmediate(go2);
        }

        /// <summary>GameObject가 파괴되면 Instance가 null로 해제된다.</summary>
        [Test]
        public void Instance_ClearedOnDestroy()
        {
            _go = new GameObject();
            _go.AddComponent<TestService>();

            Object.DestroyImmediate(_go);
            _go = null;

            Assert.IsNull(TestService.Instance);
            Assert.IsFalse(TestService.IsReady);
        }
    }
}
