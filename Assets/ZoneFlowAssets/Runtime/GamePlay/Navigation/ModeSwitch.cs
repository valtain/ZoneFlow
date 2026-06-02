namespace ZoneFlow
{
    /// <summary>gameplay:// URI의 전환 방식. ?switch= query param에 대응한다.</summary>
    public enum ModeSwitch
    {
        /// <summary>현재 모드를 중지하고 새 모드로 교체한다. (?switch 없음)</summary>
        Replace    = 0,
        /// <summary>현재 모드를 슬립하고 스택에 새 모드를 쌓는다. (?switch=stack)</summary>
        Stack      = 1,
        /// <summary>스택 전체를 정리한 후 새 모드를 시작한다. (?switch=replaceall)</summary>
        ReplaceAll = 2,
    }
}
