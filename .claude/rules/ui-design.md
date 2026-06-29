---
paths:
  - "Assets/ZoneFlowAssets/Runtime/Ui/**"
  - "Assets/ZoneFlowAssets/Runtime/Prefabs/**"
---

# Rule: UI 디자인 (패널·HUD)

`Assets/ZoneFlowAssets/Runtime/Ui/**`와 `Runtime/Prefabs/**`(**UI 패널 프리팹 한정** — `Player.prefab` 등 비-UI 프리팹은 대상 아님)를 저작하기 전에 이 규칙을 적용한다.
원문(canonical): [docs/architecture/system-layers.md](../../docs/architecture/system-layers.md), [docs/conventions/coding-style.md](../../docs/conventions/coding-style.md).

## 필수

- **runtime-code.md 동시 매칭** — 이 경로는 `Runtime/**`에도 속하므로 [runtime-code.md](runtime-code.md)가 함께 적용된다(UniTask 전용·public 필드 금지·`[SerializeField]`·`Debug.Assert`·한국어 XML doc). 이 rule은 UI 특화 항목만 추가한다.
- **PanelCatalog 손편집 금지** — PanelId→프리팹 매핑은 CatalogBaker 출력물이다. 신규 패널은 프리팹을 등록한 뒤 **`BakeAll`로 반영**(선례: InteractionPromptPanel 등록). 베이크는 작업 종료 시 **메인 세션이 1회 직렬** 실행한다.
- **패널 생명주기는 `PanelMode`/`ShellMode` 경유** — show/hide를 우회하지 않는다.
- **Mode↔Panel 매핑·베이스 클래스 수정 금지** — `Ui/Layers/`·`Ui/TransitionFx/`·`Ui/Panels/UiPanel.cs` 및 Mode와 패널을 잇는 매핑 로직은 Mode 스택 경계에 속한다. 표현(레이아웃·정보설계)만 다루고, 매핑/시스템 변경은 에스컬레이션한다.
- **인게임 UI 텍스트는 영문** — 한글 폰트 글리프 누락 회피(선례: 포탈 안내 라벨 영어화).
- **시각 검증** — `unity_screenshot_game`/`unity_play_mode`로 패널 렌더링·레이아웃을 확인한다.

## 모호하면

UI 시스템 아키텍처(베이스 클래스·새 `PanelMode`/`ShellMode`·카탈로그 플러밍)가 필요하면 멈추고 `unity-specialist`, Mode 스택과 패널 결합 패턴 판단이 필요하면 `architecture-director` 검토를 권한다.
