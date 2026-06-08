# Multi-Zone-Single-Scene — 탐색 질문

> 단일 Unity 씬 파일 안에 Zone 컴포넌트를 여러 개 배치하는 것이 현재 ZoneFlow 구조에서 가능한가?

## 컨텍스트

현재 ZoneFlow는 **Zone 1개 = 씬 1개** 원칙으로 설계되어 있다 (World1.unity, World2.unity 등).
이 탐색은 그 전제를 깰 수 있는지, 어떤 비용이 있는지를 코드 레벨로 검증한다.

다수 Zone을 하나의 씬에 묶고 싶은 동기는 다음 중 하나일 수 있다:
- 씬 파일 수를 줄여 프로젝트 관리 편의성 향상
- 테스트에서 여러 Zone 시나리오를 경량으로 구성

## 탐색 범위

- `ZoneRegistry`, `ZoneAsset`, `CatalogBaker` 코드 레벨 제약 분석
- 단일 씬 멀티 Zone이 가능하려면 어떤 변경이 필요한지 비용 추산
- 테스트 목적에 한정된 대안 존재 여부

Out of scope: 실제 구현 (탐색 결과로 promote 여부 결정 후 별도 feature로 처리)

## 성공 기준

- "가능 / 불가능 / 조건부 가능" 중 하나로 명확히 결론 낼 것
- 각 결론에 대해 코드 상 근거 파일을 제시할 것
- 채택 방향이 결정되면 promote 또는 close로 마무리
