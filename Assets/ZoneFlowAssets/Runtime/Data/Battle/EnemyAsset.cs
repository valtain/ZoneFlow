using UnityEngine;
using ZoneFlow.BattleView;

namespace ZoneFlow.Battle
{
    /// <summary>
    /// 적 전투원 정의 ScriptableObject.
    /// 식별자는 에셋 파일명(<c>so.name</c>)을 사용한다 — 별도 Id 필드 없음.
    /// 전투 개시 시 <see cref="CombatantFactory"/>가 런타임 POCO(<see cref="Combatant"/>)로 변환한다.
    /// </summary>
    [CreateAssetMenu(menuName = "ZoneFlow/Battle/EnemyAsset")]
    public sealed class EnemyAsset : ScriptableObject
    {
        /// <summary>UI에 표시할 적 이름.</summary>
        [field: SerializeField] public string DisplayName { get; private set; } = string.Empty;

        /// <summary>최대 HP.</summary>
        [field: SerializeField] public int MaxHp { get; private set; } = 50;

        /// <summary>공격력 스탯.</summary>
        [field: SerializeField] public int Attack { get; private set; } = 10;

        /// <summary>속도 스탯. 턴 순서 결정에 사용된다(내림차순).</summary>
        [field: SerializeField] public int Speed { get; private set; } = 5;

        /// <summary>보유 스킬 목록. 전투 개시 시 Damage 스킬 Power가 Combatant.SkillPowers에 매핑된다.</summary>
        [field: SerializeField] public SkillAsset[] Skills { get; private set; } = System.Array.Empty<SkillAsset>();

        /// <summary>3D 전투 뷰 프리팹(VRM 모델 포함). 미설정이면 캡슐로 폴백.</summary>
        [field: SerializeField] public BattleActorView BattleView { get; private set; }
    }
}
