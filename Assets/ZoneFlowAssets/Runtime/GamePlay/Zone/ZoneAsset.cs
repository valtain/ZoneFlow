using System;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>Zone의 식별자와 로드 방식을 정의하는 데이터 클래스. ZoneAssetRegistry 항목으로 관리된다.</summary>
    [Serializable]
    public class ZoneAsset
    {
        /// <summary>Zone 식별자. NavigationUri의 ZoneId 세그먼트와 일치해야 한다.</summary>
        [field: SerializeField] public string ZoneId { get; private set; } = default;

        /// <summary>씬 기반 Zone의 씬 이름. 프리팹 기반이면 비워 둔다.</summary>
        [field: SerializeField] public string SceneName { get; private set; } = default;

        /// <summary>프리팹 기반 Zone의 루트 프리팹. 씬 기반이면 null.</summary>
        [field: SerializeField] public GameObject ZonePrefab { get; private set; } = default;

        /// <summary>SceneName이 비어 있지 않으면 씬 기반 Zone이다.</summary>
        public bool IsSceneBased => !string.IsNullOrEmpty(SceneName);
    }
}
