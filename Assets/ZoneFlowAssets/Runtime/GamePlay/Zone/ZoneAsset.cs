using System;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>Zone의 식별자와 로드할 씬 이름을 정의하는 데이터 클래스. ZoneAssetRegistry 항목으로 관리된다.</summary>
    [Serializable]
    public class ZoneAsset
    {
        /// <summary>Zone 식별자. NavigationUri의 ZoneId 세그먼트와 일치해야 한다.</summary>
        [field: SerializeField] public string ZoneId { get; private set; } = default;

        /// <summary>Zone의 씬 이름.</summary>
        [field: SerializeField] public string SceneName { get; private set; } = default;
    }
}
