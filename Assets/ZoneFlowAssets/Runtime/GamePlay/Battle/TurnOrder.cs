using System.Collections.Generic;
using UnityEngine;

namespace ZoneFlow.Battle
{
    /// <summary>
    /// 전투원 목록으로부터 행동 순서를 결정하는 순수 턴 큐.
    /// <para>
    /// 정렬 규칙: Speed 내림차순, 동률이면 <see cref="Combatant.Id"/> 오름차순.
    /// 사망자(<see cref="Combatant.IsAlive"/> == false)는 스킵한다(라운드로빈).
    /// </para>
    /// </summary>
    public sealed class TurnOrder
    {
        private readonly List<Combatant> _sorted;
        private int _index;

        /// <summary>
        /// 전투원 목록을 받아 정렬된 턴 큐를 구성한다.
        /// 목록은 복사되며 원본을 변경해도 순서에 영향을 주지 않는다.
        /// </summary>
        /// <param name="combatants">전투에 참여하는 전투원 목록.</param>
        public TurnOrder(IReadOnlyList<Combatant> combatants)
        {
            Debug.Assert(combatants != null, "TurnOrder: combatants는 null일 수 없다.");

            _sorted = new List<Combatant>(combatants);
            _sorted.Sort(CompareBySpeedThenId);
            _index = 0;
        }

        /// <summary>
        /// 현재 순번의 생존 전투원을 반환하고 포인터를 다음으로 전진한다.
        /// 생존자가 한 명도 없으면 null을 반환한다.
        /// </summary>
        /// <returns>다음 행동 전투원. 전원 사망이면 null.</returns>
        public Combatant Next()
        {
            if (_sorted.Count == 0) return null;

            // 한 바퀴(전체 길이)를 탐색해도 생존자가 없으면 null
            int attempts = 0;
            while (attempts < _sorted.Count)
            {
                var candidate = _sorted[_index];
                _index = (_index + 1) % _sorted.Count;
                attempts++;

                if (candidate.IsAlive)
                    return candidate;
            }
            return null;
        }

        // Speed 내림차순, 동률이면 Id 오름차순
        private static int CompareBySpeedThenId(Combatant x, Combatant y)
        {
            int speedComp = y.Speed.CompareTo(x.Speed); // 내림차순
            return speedComp != 0 ? speedComp : x.Id.CompareTo(y.Id); // 오름차순
        }
    }
}
