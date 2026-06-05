using System;
using UnityEngine;

namespace ZoneFlow.Player
{
    /// <summary>플레이어의 수치 스탯 데이터. 변경 시 OnChanged 이벤트를 발행한다.</summary>
    public sealed class PlayerStats
    {
        public int Hp { get; private set; }
        public int MaxHp { get; private set; }
        public float HpRatio => MaxHp > 0 ? (float)Hp / MaxHp : 0f;

        public event Action<PlayerStats> OnChanged;

        public PlayerStats(int maxHp)
        {
            MaxHp = maxHp;
            Hp = maxHp;
        }

        public void SetHp(int hp)
        {
            Hp = Mathf.Clamp(hp, 0, MaxHp);
            OnChanged?.Invoke(this);
        }
    }
}
