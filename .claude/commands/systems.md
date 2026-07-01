# /systems
<!-- Complexity Hint: Medium → sonnet (list 서브커맨드는 Low → haiku) -->

시뮬 시스템(시간·파티·세이브·인벤)을 설계·구현한다. Zone-Mode 전환에 직교하는 전역 상태를 CoreServices 상주 Service + 데이터 모델로 짓는다(ADR-0001). Save/Load는 ISaveable 순회 + 안정 상태 세이브 + 부분 복원(ADR-0003)을 따른다. 구현은 `systems-designer` 역할 에이전트가 수행한다.

## 사용법

```text
/systems list                시뮬 서비스·데이터 목록 (Time·Party·Save·Inventory)
/systems new <system>        새 시뮬 서비스/데이터 모델 설계·구현 (TimeService·SaveService 등)
/systems improve <system>    기존 시스템 확장·리팩터링 (스탯 확장·ISaveable 편입 등)
/systems review <system>     시스템 설계 품질 검토 (Service 경계·시간 진행/세이브 계약 준수)
```

## 적합 범위

- ✅ TimeService·PartyService·SaveService·InventoryService, 스탯/저장 데이터 모델, ISaveable 편입
- ❌ 전투 턴 로직·데미지·BattleService → `combat-specialist`(`/battle`)
- ❌ 새 Service 계층 패턴·저장 복원 정책 변경 → `architecture-director`(`/explore`)
- ❌ 캘린더/상태/일과-선택 패널 저작 → `ui-designer`(`/ui`)

## 동작

1. **위임**: complexity-hook이 `systems-designer`(sonnet) 위임을 지시한다. Agent 도구 `subagent_type='systems-designer'`로 작업을 넘긴다. (`/systems list`는 Low → haiku 서브에이전트로 목록만 조회.)
2. **컨텍스트 로드**: 대상 서비스·`PlayerStats`·관련 데이터·ADR-0001/0003을 읽어 현재 상태를 파악한다.
3. **변경 예상 출력**: 바꿀 서비스/데이터 모델과 ISaveable 편입 범위를 제시한다.
4. **승인 게이트(메인 세션)**: AskUserQuestion으로 `이 변경을 진행할까요?` — 승인은 메인 세션이 중재한다.
5. **구현**: `systems-designer`가 서비스/데이터를 작성하고 EditMode/PlayMode 테스트로 검증한다.
6. **카탈로그 베이크**: 데이터 카탈로그를 바꿨으면 **메인 세션이 종료 시 `BakeAll`을 1회 직렬 실행**한다.
7. **검증**: 날짜 진행·스냅샷/복원 테스트 결과를 제시한다.
8. **마무리**: 변경 요약 후 `/git-commit` 실행 여부를 AskUserQuestion으로 확인한다.

## 주의사항

- 시간 진행은 **명시적 커밋 액션에서만**(일과-선택 패널) — Mode 훅에서 호출 금지(ADR-0001).
- 세이브는 **안정 상태에서만**(아지트·일과선택), 복원은 진입 URI 기준 부분 복원(ADR-0003).
- 카탈로그 `.asset`은 손편집하지 않는다 — 베이크로만 갱신. 자산 이동 시 `.meta`를 함께 옮긴다.
