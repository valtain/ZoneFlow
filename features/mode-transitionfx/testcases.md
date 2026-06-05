# Test Cases: mode-transitionfx

1. Exploration ↔ Battle 전환: ModeOut 완료 → 블랙 → Zone 로드 → 화면 공개 → HUD 슬라이드인 순서 확인
2. PanelMode(오버레이) 전환 시 동일 순서 확인
3. Stack(StackAsync) → Pop(PopAsync) 연속 전환: 깜빡임 없음
4. 전환 중 ct 취소: 블랙 화면 고정 없음
5. SleepAsync/ResumeAsync 경로(StackAsync, PopAsync)도 동일 순서 적용
