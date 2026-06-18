# interaction-prompt — 설계 스펙

## 목표

플레이어에게 IInteractable(Portal 등)의 친화 명칭 + 행동 힌트를, 근접 시 스크린 공간 HUD 프롬프트로 읽기 좋게 표시한다.
현재의 정적 월드 라벨(SceneSetupTool이 portalId 원문을 띄우는 월드 TextMeshPro)을 대체한다.

## 주요 컴포넌트

### IInteractable.DisplayLabel
기술 ID(`InteractableId`)와 분리된 플레이어용 표시 명칭. Portal에 `[SerializeField]`로 노출.
SceneSetupTool은 portalId 대신 이 값을 사용.

### InteractionDetector
플레이어에 부착. 근접(트리거/최근접 스캔)으로 사정권 내 최근접 IInteractable을 추적.
최근접 대상 변경을 감지하면 InteractionPromptPanel에 통지.

### InteractionPromptPanel
`UiService.Floating` 레이어의 UiPanel.
DisplayLabel + 행동 힌트("⏎ 숲 입구로 이동") 표시, PrimeTween 페이드 인/아웃.
`ExplorationHudPanel` 패턴 재사용.

## 데이터 흐름

```
Player
  └─ InteractionDetector  → 사정권 내 최근접 IInteractable 추적
                              ↓ (변경 시)
UiService.Floating
  └─ InteractionPromptPanel  → DisplayLabel + 행동 힌트 표시 / 비면 숨김
```

Player의 InteractionDetector가 최근접 IInteractable 변경을 감지 → InteractionPromptPanel을 Floating 레이어에 표시/갱신/숨김.
사정권이 비면 프롬프트를 숨긴다.
