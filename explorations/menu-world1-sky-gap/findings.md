# 탐색 결과

**결론**: FadeScreen이 alpha=0에서 시작해 0.3초에 걸쳐 불투명해지는 동안 카메라(ClearFlags=Skybox)가 배경을 렌더링해 하늘이 노출된다. 이는 단순 버그가 아니라 TransitionFX 타이밍 설계 문제이며, PanelMode와 ZoneMode가 동일한 FadeScreen을 쓰는 구조적 불일치도 함께 드러났다.

**채택된 방향**:
- Candidate A (즉시): InstantBlackScreen 추가 + Transition 선택 모듈화 + PanelMode 자체 연출 분리
- Candidate B (백로그): TransitionFX 효과 지정 확장 설계
- Candidate C (백로그): ZoneRegistry 역할 확대 — Mode-Zone 수명 분리

**폐기된 방향**:
- FadeIn을 ModeOutAsync 이전으로 이동 — ModeOut 연출이 검정 뒤에 숨어 UX 퇴보
- CoreServices 카메라 ClearFlags 변경 — 증상만 가리는 임시방편

**생성된 이슈**: 이슈 A(TASK), 이슈 B(backlog), 이슈 C(backlog) — BACKLOG.md 참조

**CLAUDE.md 반영 필요**: 없음
