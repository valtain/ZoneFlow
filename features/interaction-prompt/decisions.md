# interaction-prompt — 설계 결정

Source: [portal-interaction-label](../../explorations/portal-interaction-label/findings.md)

- 표시 레이어: UiService.Floating | 게임플레이 위·팝업 아래, 프롬프트 전용 레이어로 적합
- 감지 방식: 근접(최근접 스캔) | TPS+기존 OnTriggerEnter 기반과 일관, 조준 레이캐스트보다 관행적
- 상호작용 모델: (미정, 구현 시 확정) Portal 자동발동 유지 기본 가정 / 버튼확정은 신규 Interact 입력 액션 필요 | 탐색에서 후속 결정으로 남긴 항목
