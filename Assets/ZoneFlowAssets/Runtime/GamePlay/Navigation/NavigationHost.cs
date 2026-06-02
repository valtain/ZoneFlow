namespace ZoneFlow
{
    /// <summary>gameplay:// URI host에 대응하는 내비게이션 호스트.</summary>
    public enum NavigationHost
    {
        /// <summary>탐색 모드. gameplay://exploration</summary>
        Exploration = 0,
        /// <summary>전투 모드. gameplay://battle</summary>
        Battle      = 1,
        /// <summary>스토리 모드. gameplay://story</summary>
        Story       = 2,
        /// <summary>셸(UI 전용) 모드. gameplay://shell</summary>
        Shell       = 3,

        /// <summary>이전 모드로 복귀. gameplay://pop</summary>
        Pop    = 100,
        /// <summary>포털 리다이렉트. gameplay://portal</summary>
        Portal = 101,
    }
}
