namespace ZoneFlow
{
    /// <summary>모드 전환 방식.</summary>
    public enum ModeSwitchType
    {
        /// <summary>현재 모드를 중지하고 새 모드로 교체한다.</summary>
        Replace,
        /// <summary>현재 모드를 슬립하고 스택에 새 모드를 쌓는다.</summary>
        Stack,
        /// <summary>현재 모드를 제거하고 이전 모드를 재개한다.</summary>
        Pop
    }
}
