namespace ZoneFlow
{
    /// <summary>게임플레이 모드의 생명주기 상태.</summary>
    public enum ModeState
    {
        /// <summary>생성됨.</summary>
        Created,
        /// <summary>재생 시작됨.</summary>
        Played,
        /// <summary>모드 진입 중.</summary>
        ModeIn,
        /// <summary>활성 상태.</summary>
        Active,
        /// <summary>모드 퇴장 중.</summary>
        ModeOut,
        /// <summary>슬립 상태.</summary>
        Slept,
        /// <summary>재개됨.</summary>
        Resumed,
        /// <summary>중지됨.</summary>
        Stopped,
        /// <summary>소멸됨.</summary>
        Destroyed
    }
}
