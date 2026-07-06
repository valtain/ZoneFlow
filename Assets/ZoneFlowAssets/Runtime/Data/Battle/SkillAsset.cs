using UnityEngine;

namespace ZoneFlow.Battle
{
    /// <summary>스킬 대상 진영.</summary>
    public enum BattleTargetSide
    {
        /// <summary>적 진영을 대상으로 한다.</summary>
        Enemy,

        /// <summary>아군 진영을 대상으로 한다.</summary>
        Ally,

        /// <summary>자기 자신을 대상으로 한다.</summary>
        Self,
    }

    /// <summary>스킬 종류.</summary>
    public enum SkillKind
    {
        /// <summary>데미지를 입히는 스킬.</summary>
        Damage,

        /// <summary>HP를 회복하는 스킬.</summary>
        Heal,
    }

    /// <summary>
    /// 스킬 정의 ScriptableObject.
    /// 식별자는 에셋 파일명(<c>so.name</c>)을 사용한다 — 별도 Id 필드 없음.
    /// </summary>
    [CreateAssetMenu(menuName = "ZoneFlow/Battle/SkillAsset")]
    public sealed class SkillAsset : ScriptableObject
    {
        /// <summary>UI에 표시할 스킬 이름.</summary>
        [field: SerializeField] public string DisplayName { get; private set; } = string.Empty;

        /// <summary>스킬 종류(데미지·힐).</summary>
        [field: SerializeField] public SkillKind Kind { get; private set; } = SkillKind.Damage;

        /// <summary>스킬 파워. 데미지/힐 계산 시 추가 수치로 사용된다.</summary>
        [field: SerializeField] public int Power { get; private set; } = 10;

        /// <summary>스킬 대상 진영.</summary>
        [field: SerializeField] public BattleTargetSide TargetSide { get; private set; } = BattleTargetSide.Enemy;
    }
}
