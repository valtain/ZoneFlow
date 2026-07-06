namespace ZoneFlow.Battle
{
    /// <summary>
    /// <see cref="BattleEngine.SubmitAction"/> 한 번의 실행 결과 값 객체.
    /// </summary>
    public readonly struct ActionResult
    {
        /// <summary>이번 액션에서 대상에게 가한 데미지.</summary>
        public int DamageDealt { get; }

        /// <summary>이번 액션으로 대상이 사망(HP가 0 이하)했는지 여부.</summary>
        public bool IsKilled { get; }

        /// <summary>
        /// 액션 결과를 생성한다.
        /// </summary>
        /// <param name="damageDealt">가한 데미지.</param>
        /// <param name="isKilled">대상 사망 여부.</param>
        public ActionResult(int damageDealt, bool isKilled)
        {
            DamageDealt = damageDealt;
            IsKilled    = isKilled;
        }
    }
}
