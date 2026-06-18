# 탐색 결과

> 상태: closed (2026-06-18 확정)

**결론**: 플레이어용 표시 명칭을 IInteractable에 결합한 뒤, 스크린 공간 HUD 프롬프트로
근접 시 친화 명칭 + 행동 힌트를 표시한다.

**채택된 방향**: [A] 스크린 공간 HUD 프롬프트 (근접 트리거)
- 공통 전제: `IInteractable.DisplayLabel`(가칭) 추가 — 기술 ID와 분리, Portal에 `[SerializeField]`로 노출
- 감지: 근접(트리거/최근접 스캔) — TPS + 기존 트리거 기반과 일관
- 표시: `UiService.Floating` 레이어의 단일 `InteractionPromptPanel`(UiPanel) + PrimeTween 페이드,
  `ExplorationHudPanel` 패턴 재사용
- 근거: "보기 편하게"(가독성)가 핵심 불만 → 스크린 공간이 각도/거리 무관하게 가장 직접적으로 해결

**폐기된 방향**:
- [B] 월드 빌보드 라벨 — 이유: 거리/배경에 따라 가독성이 변동하고, 빌보드·스타일링·인스턴스별 셋업 비용이
  높으며 로컬라이즈/연출 일관성이 낮음
- [C] 하이브리드(월드 마커+스크린 프롬프트) — 즉시 채택은 작업량 과다. 단, **발견성 요구가 생기면
  후속 확장 후보**로 보존

**미해결(후속 결정)**:
- 상호작용 모델 — Portal은 현재 `OnTriggerEnter` 자동 발동. (a) 자동 유지+프롬프트 정보 표시만 /
  (b) 버튼 확정(신규 Interact 입력 액션). 기본은 (a) 가정, feature 단계에서 (b) 재확인.

**후속 Feature 후보**:
`/feature new interaction-prompt --from portal-interaction-label`
- IInteractable.DisplayLabel 추가 + Portal 노출 + SceneSetupTool 반영
- InteractionDetector(근접 최근접 추적) + InteractionPromptPanel(Floating)

**CLAUDE.md 반영 필요**: 없음
