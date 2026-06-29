# /level
<!-- Complexity Hint: Medium → Sonnet 4.6 (list 서브커맨드는 Low → Haiku) -->

존/레벨 콘텐츠를 설계·저작한다. 기존 Zone-Mode·Portal·SpawnPoint·NavigationUri·Yarn 시스템을 **활용**해 존 레이아웃·연결성·페이싱·상호작용/내러티브를 풍부하게 만든다. 구현은 `level-designer` 역할 에이전트가 수행한다.

## 사용법

```text
/level list                기존 존 목록 (ZoneAssetCatalog 기준)
/level new <zone>          신규 존 설계 + 스캐폴드
/level improve <zone>      기존 존 분석 → 레이아웃·연결성·상호작용·페이싱 보강
/level review <zone>       레벨 품질 평가 (연결성/막다른길/페이싱/스폰 배치)
```

## 적합 범위

- ✅ 존 구성·랜드마크·포탈 연결·스폰포인트·내러티브 배치, 가독성/무드 머티리얼·라이팅
- ❌ 새 런타임 시스템·새 `IInteractable` 타입(NPC·아이템·트리거) 구현 → `unity-specialist`(`/issue do`)
- ❌ 새 패턴·Zone-Mode 경계 판단 → `architecture-director`(`/explore`)
- ❌ UI 패널/HUD → `/ui`

## 동작

1. **위임**: complexity-hook이 `level-designer`(sonnet) 위임을 지시한다. Agent 도구 `subagent_type='level-designer'`로 작업을 넘긴다. (`/level list`는 Low → haiku 서브에이전트로 카탈로그만 조회.)
2. **컨텍스트 로드**: 대상 존 씬·`ZoneAssetCatalog`·관련 카탈로그를 읽어 현재 상태를 파악한다.
3. **변경 예상 출력**: 바꿀 존/포탈/스폰/머티리얼과 NavigationUri 연결을 표로 제시한다.
4. **승인 게이트(메인 세션)**: AskUserQuestion으로 `이 변경을 진행할까요?` (`진행`/`취소`). 서브에이전트는 직접 질문할 수 없으므로 승인은 메인 세션이 중재한다.
5. **저작**: `level-designer`가 `unity_*` MCP로 씬을 저작한다.
6. **카탈로그 베이크**: Zone/Interactable/SpawnPoint를 바꿨으면 **메인 세션이 작업 종료 시 CatalogBaker `BakeAll`을 1회 직렬 실행**한다(개별 베이크 금지).
7. **시각 검증**: `unity_graphics_scene_capture`로 결과를 캡처해 제시한다.
8. **마무리**: 변경 요약 출력 후 `/git-commit` 실행 여부를 AskUserQuestion으로 확인한다.

## 주의사항

- 범위를 벗어난 작업이면 먼저 알리고 적합한 커맨드(`/issue do`·`/explore`·`/ui`)를 제안한다.
- 카탈로그 `.asset`은 손편집하지 않는다 — 베이크로만 갱신.
- 씬/자산 이동 시 `.meta`를 항상 함께 옮긴다.
