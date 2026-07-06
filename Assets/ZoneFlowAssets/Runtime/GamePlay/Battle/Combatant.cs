using System.Collections.Generic;
using UnityEngine;

namespace ZoneFlow.Battle
{
    /// <summary>
    /// 전투원 런타임 POCO.
    /// ScriptableObject 의존 없이 순수 C#으로만 구성되어 헤드리스 테스트가 가능하다.
    /// </summary>
    public sealed class Combatant
    {
        /// <summary>전투 내 고유 식별자. 결정론 정렬(Id 오름차순 타이브레이크)에 사용된다.</summary>
        public int Id { get; }

        /// <summary>이 전투원이 속한 진영.</summary>
        public BattleSide Side { get; }

        /// <summary>최대 HP.</summary>
        public int MaxHp { get; }

        /// <summary>현재 HP. 0 이하면 사망 판정.</summary>
        public int Hp { get; private set; }

        /// <summary>공격력 스탯. 데미지 계산에 사용된다.</summary>
        public int Attack { get; }

        /// <summary>속도 스탯. 턴 순서 결정에 사용된다(내림차순).</summary>
        public int Speed { get; }

        /// <summary>
        /// 스킬 파워 참조 목록(경량). 후속 슬라이스에서 SkillAsset 참조로 확장된다.
        /// 이번 청크에서는 정수 power 값만 보유한다.
        /// </summary>
        public IReadOnlyList<int> SkillPowers { get; }

        /// <summary>HP > 0이면 생존, 0 이하면 사망.</summary>
        public bool IsAlive => Hp > 0;

        /// <summary>
        /// 전투원을 생성한다.
        /// </summary>
        /// <param name="id">전투 내 고유 식별자.</param>
        /// <param name="side">진영.</param>
        /// <param name="maxHp">최대 HP.</param>
        /// <param name="attack">공격력.</param>
        /// <param name="speed">속도.</param>
        /// <param name="skillPowers">스킬 파워 목록(선택). null이면 빈 목록.</param>
        public Combatant(int id, BattleSide side, int maxHp, int attack, int speed,
            IReadOnlyList<int> skillPowers = null)
        {
            Debug.Assert(maxHp > 0, $"Combatant(id={id}): maxHp는 1 이상이어야 한다.");
            Debug.Assert(attack >= 0, $"Combatant(id={id}): attack은 0 이상이어야 한다.");
            Debug.Assert(speed >= 0, $"Combatant(id={id}): speed는 0 이상이어야 한다.");

            Id          = id;
            Side        = side;
            MaxHp       = maxHp;
            Hp          = maxHp;
            Attack      = attack;
            Speed       = speed;
            SkillPowers = skillPowers ?? System.Array.Empty<int>();
        }

        /// <summary>
        /// HP를 지정한 데미지만큼 감소시킨다. 0 미만으로 내려가지 않는다.
        /// </summary>
        /// <param name="amount">적용할 데미지 양(양수).</param>
        public void ApplyDamage(int amount)
        {
            Debug.Assert(amount >= 0, $"ApplyDamage: amount({amount})는 0 이상이어야 한다.");
            Hp = Mathf.Max(0, Hp - amount);
        }

        /// <summary>
        /// HP를 지정한 양만큼 회복한다. MaxHp를 초과하지 않는다.
        /// </summary>
        /// <param name="amount">회복량(양수).</param>
        public void ApplyHeal(int amount)
        {
            Debug.Assert(amount >= 0, $"ApplyHeal: amount({amount})는 0 이상이어야 한다.");
            Hp = Mathf.Min(MaxHp, Hp + amount);
        }
    }
}
