using System;

namespace ZoneFlow
{
    /// <summary>
    /// 에디터 디버그 윈도우용 상태 변경 알림 허브.
    /// 이벤트/메서드 자체는 항상 컴파일되며, 실제 발행(호출)만 호출부에서 #if UNITY_EDITOR로 게이팅한다.
    /// 따라서 빌드에서는 구독자도 호출도 없어 비용이 0이고, API 표면은 조건부 컴파일로 흔들리지 않는다.
    /// </summary>
    public static class GamePlayDebug
    {
        /// <summary>모드 스택 또는 Zone 로드 상태가 바뀔 때 발행된다.</summary>
        public static event Action StateChanged;

        /// <summary>구독자에게 상태 변경을 통지한다. 발행 호출부는 #if UNITY_EDITOR로 감싸 빌드 비용을 차단한다.</summary>
        public static void NotifyStateChanged() => StateChanged?.Invoke();
    }
}
