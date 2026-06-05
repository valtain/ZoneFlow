# 후보 방향들

## Candidate A: InstantBlackScreen + Transition 모듈화
**상태**: promoted

- `InstantBlackScreen : IUiTransitionEffect` 추가 (duration=0, 즉시 alpha=1)
- GamePlayDirector Transition 효과 선택을 별도 메서드로 분리
- PanelMode 전환에서 FadeScreen 제거 → `UiPanel.ShowInAsync()`/`ShowOutAsync()` 호출로 대체
- Promoted → TASK 이슈

## Candidate B: TransitionFX 확장 설계
**상태**: promoted

- ZoneAsset 레벨 기본 효과 지정
- NavigationRequest 레벨 오버라이드
- 우선순위 레이어 설계 (URI > Zone > Mode > 기본값)
- Promoted → 백로그 이슈

## Candidate C: ZoneRegistry 역할 확대
**상태**: promoted

- Director에서 ActiveZone을 Mode와 별도 추적
- Mode 유지 + Zone 전환, Zone 유지 + Mode 전환 케이스 지원
- GamePlayMode에서 ZoneAsset 소유 제거 방향 검토
- Promoted → 백로그 이슈

## 탈락 후보

### FadeIn 시작을 ModeOutAsync 이전으로 이동
**상태**: eliminated — ModeOut 연출(패널 슬라이드아웃 등)이 검정 뒤에 숨어버려 UX 퇴보.
Candidate A의 InstantBlackScreen으로 더 깔끔하게 해결 가능.

### CoreServices 카메라 ClearFlags를 SolidColor(검정)으로 변경
**상태**: eliminated — 증상만 가리는 임시방편. 근본 원인(FadeScreen 타이밍)은 그대로 남음.
