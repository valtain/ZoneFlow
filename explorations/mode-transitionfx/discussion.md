# 탐색 로그

- [2026-06-05 | start] 탐색 시작. 주제: Mode 전환 중 TransitionFx 적용.

- [2026-06-05 | brainstorm] TransitionFx 인프라(`FadeScreen`, `UiTransitionFxLayer`, `UiService.Transition<T>()`)는 CoreServices 씬까지 배선 완료. `GamePlayDirector` 4개 전환 메서드(Replace/Stack/ReplaceAll/Pop)에서 사용처 0건 확인. `OnModeOutAsync`는 `StopAndDestroyAsync` 내부 맨 앞, `OnModeInAsync`는 `PlayAsync` 내부 맨 끝에 위치하여 Director 레벨 단순 래핑 불가. 구조적 분해 필요.

- [2026-06-05 | decision] 사용자 확정: PanelMode 포함 전체 Mode 전환 일괄 적용. 목표 시퀀스: `ModeOut(화면) → [Fade In] → Stop/Load → [Fade Out] → ModeIn(화면)`.

- [2026-06-05 | decision] 네이밍 비교(`ExitAsync`/`EnterAsync` vs `ModeOutAsync`/`ModeInAsync`): `ModeState` enum 및 기존 훅과의 일관성으로 `ModeOutAsync`/`ModeInAsync` 채택.

- [2026-06-05 | decision] Candidate A(4단계 분리) 채택, Candidate B(bool 파라미터) 폐기.
- [2026-06-05 | promote] Candidate A → features/mode-transitionfx/ 생성
