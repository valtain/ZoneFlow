using UnityEngine;

namespace ZoneFlow
{
    /// <summary>존(Zone) 씬 또는 프리팹을 참조하는 ScriptableObject 자산. 에셋 파일명을 ZoneId로 사용한다.</summary>
    [CreateAssetMenu(fileName = "Zone", menuName = "ZoneFlow/GamePlay/Zone")]
    public class ZoneAsset : ScriptableObject
    {
        /// <summary>존이 씬 기반일 때 로드할 씬 이름. 프리팹 기반이면 비워 둔다.</summary>
        [field: SerializeField] public string SceneName { get; private set; } = default;

        /// <summary>존이 프리팹 기반일 때 인스턴스화할 프리팹. 씬 기반이면 null.</summary>
        [field: SerializeField] public GameObject ZonePrefab { get; private set; } = default;

        /// <summary>SceneName이 비어 있지 않으면 씬 기반 존이다.</summary>
        public bool IsSceneBased => !string.IsNullOrEmpty(SceneName);

        /// <summary>에셋 파일명을 ZoneId로 사용한다.</summary>
        public string ZoneId => name;
    }
}
