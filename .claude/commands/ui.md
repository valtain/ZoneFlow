# /ui
<!-- Complexity Hint: Medium → Sonnet 4.6 (list 서브커맨드는 Low → Haiku) -->

UI/HUD/패널을 설계·저작한다. PanelCatalog·PanelMode·ShellMode·InteractionPrompt·Dialogue 표현 등 패널 레이아웃·정보설계·프리팹을 다룬다. 구현은 `ui-designer` 역할 에이전트가 수행한다.

## 사용법

```text
/ui list                 기존 패널 목록 (PanelCatalog 기준)
/ui new <panel>          신규 패널 설계 + 프리팹 스캐폴드
/ui improve <panel>      기존 패널 분석 → 정보설계·레이아웃·가독성 보강
/ui review <panel>       UI 품질 평가 (정보 위계/앵커링/가독성)
```

## 적합 범위

- ✅ 패널/HUD 정보설계·레이아웃·앵커링·프리팹 저작, PanelCatalog 등록
- ❌ UI 시스템 코드(베이스 클래스·패널 생명주기·새 `PanelMode`/`ShellMode`·**Mode↔Panel 매핑**) → `unity-specialist`(`/issue do`)
- ❌ Mode 스택과 패널 결합 패턴 판단 → `architecture-director`(`/explore`)
- ❌ 존/레벨 콘텐츠 → `/level`

## 동작

1. **위임**: complexity-hook이 `ui-designer`(sonnet) 위임을 지시한다. Agent 도구 `subagent_type='ui-designer'`로 작업을 넘긴다. (`/ui list`는 Low → haiku 서브에이전트로 카탈로그만 조회.)
2. **컨텍스트 로드**: 대상 패널 프리팹(`Runtime/Prefabs/*.prefab`)·`Runtime/Ui/Panels/*`·`PanelCatalog`를 읽어 현재 상태를 파악한다.
3. **변경 예상 출력**: 바꿀 패널/프리팹/카탈로그 항목을 표로 제시한다.
4. **승인 게이트(메인 세션)**: AskUserQuestion으로 `이 변경을 진행할까요?` (`진행`/`취소`). 승인은 메인 세션이 중재한다.
5. **저작**: `ui-designer`가 `unity_*` MCP로 패널 프리팹·캔버스를 저작한다. **인게임 텍스트는 영문**.
6. **카탈로그 베이크**: 신규 패널을 등록했으면 **메인 세션이 작업 종료 시 CatalogBaker `BakeAll`을 1회 직렬 실행**한다.
7. **시각 검증**: `unity_screenshot_game`로 결과를 캡처해 제시한다.
8. **마무리**: 변경 요약 출력 후 `/git-commit` 실행 여부를 AskUserQuestion으로 확인한다.

## 주의사항

- 베이스 클래스·Mode↔Panel 매핑 수정이 필요하면 멈추고 `unity-specialist`로 에스컬레이션한다(Mode 스택 경계 보존).
- 카탈로그 `.asset`은 손편집하지 않는다 — 베이크로만 갱신.
- 프리팹 이동 시 `.meta`를 항상 함께 옮긴다.
