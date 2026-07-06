using System.Collections.Generic;
using UnityEngine;

namespace ZoneFlow.Battle
{
    /// <summary>
    /// <see cref="BattleEncounterAsset"/> ScriptableObject를 런타임 POCO(<see cref="BattleSetup"/>)로 변환하는 팩토리.
    /// SO↔엔진 경계를 이 한 지점으로 국한하여 결합도를 최소화한다(ADR-0004).
    /// MonoBehaviour 무의존 — 순수 C# 정적 클래스.
    /// </summary>
    public static class CombatantFactory
    {
        /// <summary>
        /// <see cref="BattleEncounterAsset"/>을 읽어 <see cref="BattleSetup"/>을 구성한다.
        /// 파티와 적 전체에 걸쳐 결정론적 유니크 int Id를 부여한다(파티 0부터 오름차순 → 적 이어서).
        /// </summary>
        /// <param name="encounter">전투 조우 설정 SO.</param>
        /// <returns>전투 엔진에 주입할 BattleSetup.</returns>
        public static BattleSetup BuildSetup(BattleEncounterAsset encounter)
        {
            Debug.Assert(encounter != null, "[CombatantFactory] encounter가 null이다.");
            Debug.Assert(encounter.Party != null && encounter.Party.Length > 0,
                "[CombatantFactory] encounter.Party가 비어있다.");
            Debug.Assert(encounter.Enemies != null && encounter.Enemies.Length > 0,
                "[CombatantFactory] encounter.Enemies가 비어있다.");

            int nextId = 0;

            var party = new List<Combatant>(encounter.Party.Length);
            foreach (var persona in encounter.Party)
            {
                Debug.Assert(persona != null,
                    $"[CombatantFactory] Party[{nextId}] PersonaAsset이 null이다.");
                party.Add(BuildFromPersona(persona, nextId++, BattleSide.Player));
            }

            var enemies = new List<Combatant>(encounter.Enemies.Length);
            foreach (var enemy in encounter.Enemies)
            {
                Debug.Assert(enemy != null,
                    $"[CombatantFactory] Enemies[{nextId - encounter.Party.Length}] EnemyAsset이 null이다.");
                enemies.Add(BuildFromEnemy(enemy, nextId++, BattleSide.Enemy));
            }

            return new BattleSetup(party, enemies, encounter.Seed);
        }

        // ─────────────────────────────────────────────────────────────
        // 내부 헬퍼
        // ─────────────────────────────────────────────────────────────

        private static Combatant BuildFromPersona(PersonaAsset persona, int id, BattleSide side)
        {
            return new Combatant(
                id:          id,
                side:        side,
                maxHp:       persona.MaxHp,
                attack:      persona.Attack,
                speed:       persona.Speed,
                skillPowers: ExtractDamageSkillPowers(persona.Skills));
        }

        private static Combatant BuildFromEnemy(EnemyAsset enemy, int id, BattleSide side)
        {
            return new Combatant(
                id:          id,
                side:        side,
                maxHp:       enemy.MaxHp,
                attack:      enemy.Attack,
                speed:       enemy.Speed,
                skillPowers: ExtractDamageSkillPowers(enemy.Skills));
        }

        /// <summary>
        /// SkillAsset 배열에서 Damage 종류의 스킬 Power만 추출한다.
        /// Heal 등은 이번 슬라이스 auto-policy에서 미사용이어도 데이터는 보존(Damage만 POCO에 매핑).
        /// </summary>
        private static IReadOnlyList<int> ExtractDamageSkillPowers(SkillAsset[] skills)
        {
            if (skills == null || skills.Length == 0)
                return System.Array.Empty<int>();

            var powers = new List<int>(skills.Length);
            foreach (var skill in skills)
            {
                if (skill != null && skill.Kind == SkillKind.Damage)
                    powers.Add(skill.Power);
            }
            return powers;
        }
    }
}
