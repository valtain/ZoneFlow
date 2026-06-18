# 후보 방향들

## 공통 전제 (후보와 무관하게 필요)

**`IInteractable`에 플레이어용 표시 명칭 추가** — 기술 ID(`InteractableId`)와 분리.
예: `string DisplayLabel { get; }`. Portal에 `[SerializeField]`로 노출하고 SceneSetupTool은
`portalId` 대신 이 값을 사용. (현재 라벨이 ID 원문을 띄우는 근본 원인 해소)

---

## [A] 스크린 공간 HUD 프롬프트 (근접 트리거) — 추천

**상태**: active (추천)

플레이어에 감지기(근접 트리거/최근접 스캔)를 두고, `Floating` 레이어의 단일
`InteractionPromptPanel`(UiPanel)이 현재 사정권 내 최근접 interactable의 `DisplayLabel` +
행동 힌트("⏎ 숲 입구로 이동")를 표시. PrimeTween 페이드 인/아웃.

```
Player
  └─ InteractionDetector  → 사정권 내 최근접 IInteractable 추적
                              ↓ (변경 시)
UiService.Floating
  └─ InteractionPromptPanel  → DisplayLabel + 행동 힌트 표시 / 비면 숨김
```

장점:
- 가독성 최상 — 스크린 공간·고정 폰트, 각도/거리 무관
- 스타일 일관 — `ExplorationHudPanel` 패턴 재사용, HUD 인프라 그대로
- 추후 로컬라이즈 용이, 빌보드 수학 불필요

단점:
- 비diegetic — 대상을 공간적으로 직접 가리키지 않음
- 근처 다중 대상 시 최근접 선택 로직 필요

---

## [B] 월드 공간 빌보드 라벨 (개선판)

**상태**: active

월드 라벨 유지하되 빌보드(카메라 바라보기) + ID→DisplayLabel + 외곽선/배경 쿼드 +
거리 기반 페이드/스케일 + 사정권 내에서만 표시. 재사용 `InteractableLabel` MonoBehaviour가
SceneSetupTool의 즉석 TMP를 대체.

장점:
- diegetic·공간적 — 대상에 명확히 결속, 다중 라벨 자연 공존
- 신규 시스템 최소 — 기존 월드 라벨 구조 계승

단점:
- 거리/배경에 따라 가독성 변동
- 월드 TMP 스타일링·빌보드 작업 필요
- 로컬라이즈/연출 일관성 낮음, 인스턴스별 셋업

---

## [C] 하이브리드 — 월드 마커(아이콘) + 스크린 상세 프롬프트

**상태**: active (후속 확장 후보)

각 대상 위 작은 상시 빌보드 마커(◆ 등)로 발견성 제공 + 최근접 대상만 스크린 프롬프트(=A)로
전체 텍스트 표시.

장점:
- UX 최상 — 발견성(마커) + 가독성(스크린 프롬프트) 동시 충족
- 대상 다수에 확장적

단점:
- 작업량 최다 — 두 시스템 빌드/유지

---

## 미해결 설계 포인트 (탐색에서 좁힐 것)

1. **감지 방식**: 근접(트리거/거리 스캔) vs 조준 레이캐스트(카메라 중심).
   TPS + 기존 트리거 기반 → **근접 권장**.
2. **상호작용 모델**: 현재 `OnTriggerEnter` 자동 발동. 프롬프트 도입 시 —
   (a) 자동 발동 유지, 프롬프트는 정보 표시만 / (b) 버튼 확정(신규 Interact 입력 액션 필요).
   → Portal 기본은 (a) 가정, feature 단계에서 (b) 재확인.
