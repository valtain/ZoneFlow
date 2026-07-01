# /battle
<!-- Complexity Hint: Medium → sonnet (list 서브커맨드는 Low → haiku) -->

턴제 전투를 설계·구현한다. 기존 Zone-Mode 골격 위에 `BattleMode`·`BattleService`와 전투 데이터(스킬·페르소나·적)를 저작하며, 전투 결과는 모드 간 결과 채널(ADR-0002)로 전달한다. 구현은 `combat-specialist` 역할 에이전트가 수행한다.

## 사용법

```text
/battle list               전투 요소 목록 (스킬·페르소나·적·인카운터 카탈로그)
/battle new <element>      새 전투 시스템/데이터 설계·구현 (BattleService·BattleMode·SkillAsset 등)
/battle tune <element>     밸런스·데미지·턴 순서 조정
/battle review <element>   전투 설계·구현 품질 검토 (결과 채널 계약·결정론·테스트 가능성)
```

## 적합 범위

- ✅ BattleMode 확장, BattleService(턴 로직·데미지), SkillAsset/PersonaAsset/EnemyAsset, 전투 밸런스
- ❌ 파티/스탯/시간/세이브 상태 소유·직렬화 → `systems-designer`(`/systems`)
- ❌ 전투 결과 채널·Mode 스택 경계 변경 → `architecture-director`(`/explore`)
- ❌ 전투 커맨드/HUD 패널 저작 → `ui-designer`(`/ui`), 아레나 Zone 레이아웃 → `level-designer`(`/level`)

## 동작

1. **위임**: complexity-hook이 `combat-specialist`(sonnet) 위임을 지시한다. Agent 도구 `subagent_type='combat-specialist'`로 작업을 넘긴다. (`/battle list`는 Low → haiku 서브에이전트로 카탈로그만 조회.)
2. **컨텍스트 로드**: `BattleMode.cs`·`GamePlayDirector`·관련 전투 데이터·ADR-0002를 읽어 현재 상태를 파악한다.
3. **변경 예상 출력**: 바꿀 코드/데이터와 결과 채널 배선을 제시한다.
4. **승인 게이트(메인 세션)**: AskUserQuestion으로 `이 변경을 진행할까요?` — 서브에이전트는 직접 질문할 수 없으므로 승인은 메인 세션이 중재한다.
5. **구현**: `combat-specialist`가 코드/데이터를 작성하고 `unity_*` MCP로 검증한다.
6. **카탈로그 베이크**: 전투 데이터 카탈로그를 바꿨으면 **메인 세션이 종료 시 `BakeAll`을 1회 직렬 실행**한다.
7. **검증**: `unity_play_mode`/`unity_screenshot_game`으로 턴 흐름·승패 복귀를 확인해 제시한다.
8. **마무리**: 변경 요약 후 `/git-commit` 실행 여부를 AskUserQuestion으로 확인한다.

## 주의사항

- 전투는 파티/스탯을 **읽기만** 한다 — 상태 소유·저장은 `/systems` 몫.
- 결과 페이로드를 Navigation URI에 싣지 않는다(ADR-0002).
- 카탈로그 `.asset`은 손편집하지 않는다 — 베이크로만 갱신. 자산 이동 시 `.meta`를 함께 옮긴다.
