using UnityEngine;

namespace ZoneFlow
{
    /// <summary>
    /// 씬 배치 기반 정적 서비스 접근 패턴을 제공하는 제네릭 추상 베이스 클래스.
    /// 서비스는 코드에서 생성하지 않고 씬에 GameObject로 배치하여 등록한다.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public abstract class MonoService<T> : MonoBehaviour where T : MonoService<T>
    {
        /// <summary>현재 씬에 등록된 서비스 인스턴스.</summary>
        public static T Instance { get; private set; }

        /// <summary>인스턴스가 등록되어 있으면 true.</summary>
        public static bool IsReady => Instance != null;

        /// <summary>
        /// Instance를 등록한다. 동일 씬에 두 개 이상 배치된 경우 Assert를 발생시킨다.
        /// </summary>
        protected virtual void Awake()
        {
            Debug.Assert(Instance == null, $"[{typeof(T).Name}] 중복 인스턴스가 감지되었습니다. 씬에 하나만 배치하세요.");
            Instance = (T)this;
        }

        /// <summary>씬 언로드 시 Instance를 해제한다.</summary>
        protected virtual void OnDestroy()
        {
            Instance = null;
        }
    }
}
