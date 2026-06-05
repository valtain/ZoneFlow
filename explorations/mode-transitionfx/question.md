# mode-transitionfx — 탐색 질문

> Mode 전환 시 TransitionFx(블랙 페이드)를 어떻게 삽입할 것인가?

## 컨텍스트

TransitionFx 인프라(`FadeScreen`, `UiTransitionFxLayer`, `UiService.Transition<T>()`)는
CoreServices 씬까지 배선이 완료되어 있으나, `GamePlayDirector`의 4개 전환 메서드에서
단 한 번도 호출되지 않는다 (사용처 0건).

구조적 제약: `OnModeOutAsync`는 `StopAndDestroyAsync` 내부 맨 앞에,
`OnModeInAsync`는 `PlayAsync` 내부 맨 끝에 포함되어 있어, Director 레벨 단순 래핑 불가.

## 탐색 범위

- `GamePlayMode` 라이프사이클 분해 방식
- `GamePlayDirector` 4개 전환 메서드 재구성
- SleepAsync/ResumeAsync 경로 처리 (`StackAsync`, `PopAsync`)

Out of scope: TransitionFx 효과 종류 추가 (FadeScreen 이외), 모드별 개별 효과 지정

## 성공 기준

확정된 목표 시퀀스가 모든 전환 경로에서 구현된다:

```text
ModeOut [화면 노출] → [Fade In / 블랙] → Stop/ZoneRelease → ZoneLoad/OnPlayed → [Fade Out] → ModeIn [화면 노출]
```

- PanelMode 포함 전체 Mode 전환에 일괄 적용
- ct 취소 시 블랙 화면 고정 없음
- Stack → Pop 연속 전환 시 깜빡임 없음
