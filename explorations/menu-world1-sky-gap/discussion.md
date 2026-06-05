# 탐색 로그

- [2026-06-05 | start] Menu→World1 전환 시 하늘(빈 공간) 노출 현상 탐색 시작.

- [2026-06-05 | explore] 근본 원인 특정. `GamePlayDirector.ReplaceAllAsync()`(라인 134)에서 `ModeOutAsync` 완료 후 `FadeScreen.ShowAsync`가 alpha=0→1로 0.3초 걸리는 동안 CoreServices 카메라(m_ClearFlags:1=Skybox)가 배경을 렌더링해 하늘이 노출됨. `UiBgCover.FadeInAsync`는 `startValue: CanvasGroup.alpha`로 시작하므로 이전 FadeOut이 alpha=0으로 끝난 후 항상 0에서 시작.

- [2026-06-05 | explore] 설계 방향 논의. (1) TransitionFX 효과는 Mode 타입이 아닌 Zone/Navigation 레벨에서 지정해야 함. (2) PanelMode는 FadeScreen 불필요 — 패널이 ShowInAsync/ShowOutAsync로 자체 연출 담당. (3) Mode-Zone 수명 분리 필요 — 현재 구조는 Mode가 ZoneAsset을 소유해 Mode 유지+Zone 전환, Zone 유지+Mode 전환 케이스를 지원하기 어려움.

- [2026-06-05 | decision] 3개 이슈로 분리. A(즉시): InstantBlackScreen + Transition 모듈화. B(백로그): TransitionFX 확장 설계. C(백로그): ZoneRegistry 역할 확대.

- [2026-06-05 | promote] Candidate A → TASK 이슈. Candidate B → 백로그 이슈. Candidate C → 백로그 이슈.

- [2026-06-05 | close] 탐색 완료.
