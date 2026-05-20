using UnityEngine;

namespace ZoneFlow
{
    /// <summary>씬을 ScriptableObject로 참조하는 자산. 씬 이름을 문자열로 감싸는 역할을 수행한다.</summary>
    [CreateAssetMenu(fileName = "Scene", menuName = "ZoneFlow/Scene")]
    public class SceneSo : ScriptableObject
    {
        /// <summary>로드할 씬의 이름.</summary>
        [field: SerializeField]
        public string SceneName { get; private set; }
    }
}
