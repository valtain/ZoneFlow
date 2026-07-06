using System.Collections.Generic;
using UnityEngine;

namespace ZoneFlow.Battle
{
    /// <summary>
    /// 전투 개시에 필요한 초기화 데이터 POCO.
    /// <para>
    /// 파티·적 전투원 목록과 결정론 시드를 보유하며 <see cref="BattleEngine"/> 생성자에 주입된다.
    /// <c>PartyService</c> 등 시뮬 서비스에 직접 참조하지 않는다.
    /// </para>
    /// </summary>
    public sealed class BattleSetup
    {
        /// <summary>플레이어 파티 전투원 목록(읽기 전용).</summary>
        public IReadOnlyList<Combatant> Party { get; }

        /// <summary>적 전투원 목록(읽기 전용).</summary>
        public IReadOnlyList<Combatant> Enemies { get; }

        /// <summary>결정론 재현을 위한 시드.</summary>
        public int Seed { get; }

        /// <summary>
        /// 전투 설정을 생성한다.
        /// </summary>
        /// <param name="party">플레이어 파티 전투원 목록. 비어있으면 안 된다.</param>
        /// <param name="enemies">적 전투원 목록. 비어있으면 안 된다.</param>
        /// <param name="seed">결정론 시드.</param>
        public BattleSetup(IReadOnlyList<Combatant> party, IReadOnlyList<Combatant> enemies, int seed)
        {
            Debug.Assert(party != null && party.Count > 0,
                "BattleSetup: party는 null이거나 빈 목록일 수 없다.");
            Debug.Assert(enemies != null && enemies.Count > 0,
                "BattleSetup: enemies는 null이거나 빈 목록일 수 없다.");

            Party   = party;
            Enemies = enemies;
            Seed    = seed;
        }
    }
}
