# Story-YarnSpinner — 탐색 질문

> YarnSpinner를 ZoneFlow Story 모드에 통합할 때, DialogueRunner(MonoBehaviour) 생명주기를 어디에 두어야 ZoneFlow 제약(DontDestroyOnLoad 금지 / 씬=서비스 생성 책임)을 지키면서 AQ-2·AQ-3를 실제 코드로 검증할 수 있는가?

## 컨텍스트

ZoneFlow Story 모드는 현재 Zone 로드 + HUD 배너 표시만 수행하며, 실제 내러티브
진행 시스템이 없다. YarnSpinner로 대화/내러티브 레이어를 추가하는 과정 자체가
다음 두 Architectural Question을 코드로 검증하는 **stress test** 역할을 한다.

- **AQ-2**: Zone 전환 후 Story 진행 상태(`$story_progress` 등 Yarn 변수)가 지속되는가
- **AQ-3**: Mode 스택(stack switch)과 Yarn 내러티브가 공존 가능한가

이 작업은 portfolio-demo 데모와 **별개 트랙**이다. 데모는 Story를 DialogueData
최소 구현(S1)으로 처리하고, 본 exploration은 아키텍처 검증을 목적으로 한다.

복잡도: **High** (외부 시스템 + Zone-Mode 생명주기 교차 + 새 패턴 도입)

## 핵심 긴장 (미해결)

원본 검토 계획(`declarative-sniffing-lake.md`)에 내부 모순이 있다:
- **핵심 설계**: DialogueRunner를 StoryMode 스코프에 두어, Zone 전환 시 파괴되지
  않게 하고 OnStopped에서 정리 → DontDestroyOnLoad 회피
- **주의사항**: "DialogueRunner는 MonoBehaviour라 씬에 없으면 Update 불가 →
  MonoService root에 Attach 또는 DontDestroyOnLoad 예외 처리 검토 필요"

즉 "DontDestroyOnLoad 금지" 제약과 "DialogueRunner가 Zone 전환 중 살아있어야
한다"는 요구가 충돌하며, 어느 GameObject에 Attach할지가 미결이다.
**이 생명주기 결정이 본 exploration의 1차 산출물.**

## 탐색 범위

- DialogueRunner GameObject를 소유/Attach할 위치 결정 (StoryMode 스코프 vs
  MonoService root vs 별도 비-Zone 씬)
- Zone 전환 시 Pause/Resume vs 상태 유지 메커니즘 (OnSlept/OnResumed 훅 활용)
- Yarn 변수 저장소(InMemoryVariableStorage) 수명과 Mode 스택 Pop 시 정리 시점
- AQ-2/AQ-3를 증명할 검증용 Yarn 스크립트 설계

Out of scope: Addressable 전환 시 생명주기(AQ-4, 후속), Save/Load(AQ-5),
portfolio-demo 데모 범위

## 성공 기준

- DialogueRunner 생명주기 위치가 ZoneFlow 제약과 충돌 없이 확정됨
- Zone 전환 전후 Yarn 변수 지속성(AQ-2) 검증 방법이 정의됨
- Mode 스택과 내러티브 공존(AQ-3) 방식이 정의됨
- `/feature`로 승격 가능한 구현 스펙 초안 확보
