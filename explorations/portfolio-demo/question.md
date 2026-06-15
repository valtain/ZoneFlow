# Portfolio-Demo — 탐색 질문

> ZoneFlow 아키텍처의 핵심 가치(Zone·Mode 독립성)를 5~7분 플레이로 증명하는 포트폴리오 데모를, 기존 코드베이스 위에서 어떤 범위·구조·작업 분담으로 구현할 것인가?

## 컨텍스트

ZoneFlow는 Navigation URI / GamePlayDirector / ZoneRegistry / Mode 스택이 이미
구현된 상태다. 포트폴리오 심사자에게 "왜 이 구조인가"를 증명하려면 다음 두 명제가
플레이 중 자연스럽게 드러나야 한다.

```
명제 A.  Zone이 바뀌어도 Mode는 영향 받지 않는다
         → ExplorationMode가 Village → Dungeon 전환 중에도 유지

명제 B.  Mode가 바뀌어도 Zone은 영향 받지 않는다
         → Dungeon 안에서 Exploration ↔ Battle 전환 시 Zone 상태 보존
```

입력 자료 2종:
1. **시나리오 스펙** — Zone 3개(Village/Dungeon/BossRoom) + Mode 3종
   (Exploration/Story/Battle), 전환 흐름 전체 (Push/Pop/Replace)
2. **아트 에셋 셋업 계획** — Low Poly Dungeon Lite 1팩 + Primitive 혼합 전략,
   적은 Capsule Primitive로 대체 후 나중에 모델 교체

## 탐색 범위

- **구현 범위 확정**: 시나리오 풀스펙 vs MVP 우선순위 (어디까지가 데모 필수인가)
- **미구현 갭 해소 방식**:
  - StoryMode 대화 시스템 (현재 StoryHudPanel만 존재, DialogueData 없음)
  - BattleMode 게임 로직 (현재 스타터만, Enemy 판정·공격 입력 없음)
  - Enemy 접촉 → BattleMode 진입 연결 방식
- **기존 자산과의 정합성**: Portal.cs / IInteractable / ZoneAssetCatalog 재사용
- **작업 분담**: Claude Code(스크립트·프리팹) vs 개발자(Unity Editor 씬 배치)

Out of scope: 전투 밸런싱, Save/Load(AQ-5), 인벤토리, 멀티플레이, 멀티 Zone
동시 로드(AQ-1), 아트 퀄리티 향상

## 성공 기준

- 데모 구현 범위(MVP / 확장)가 우선순위와 함께 확정됨
- StoryMode·BattleMode 미구현 갭의 최소 구현 방식이 후보로 정리됨
- EnemyController/NpcInteractable의 Navigation 연결 패턴이 결정됨
- Claude Code 담당 작업과 개발자 수동 작업의 경계가 명확해짐
- 후속 `/feature` 승격이 가능한 상태
