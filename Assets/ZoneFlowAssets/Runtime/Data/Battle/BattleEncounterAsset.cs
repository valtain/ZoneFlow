using UnityEngine;

namespace ZoneFlow.Battle
{
    /// <summary>
    /// 전투 조우 설정 ScriptableObject.
    /// 파티·적 목록과 결정론 시드를 보유하며, <see cref="CombatantFactory"/>가 이를 읽어 <see cref="BattleSetup"/>을 구성한다.
    /// 식별자는 에셋 파일명(<c>so.name</c>)을 사용한다 — 별도 Id 필드 없음.
    /// </summary>
    [CreateAssetMenu(menuName = "ZoneFlow/Battle/BattleEncounterAsset")]
    public sealed class BattleEncounterAsset : ScriptableObject
    {
        /// <summary>플레이어 파티 전투원 목록.</summary>
        [field: SerializeField] public PersonaAsset[] Party { get; private set; } = System.Array.Empty<PersonaAsset>();

        /// <summary>적 전투원 목록.</summary>
        [field: SerializeField] public EnemyAsset[] Enemies { get; private set; } = System.Array.Empty<EnemyAsset>();

        /// <summary>결정론 재현을 위한 시드. BattleSetup에 전달된다.</summary>
        [field: SerializeField] public int Seed { get; private set; } = 42;
    }
}
