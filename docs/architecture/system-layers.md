# 시스템 계층

ZoneFlow의 런타임 시스템은 네 계층으로 구성된다.

| 계층 | 역할 |
| --- | --- |
| **Service** | 영속적 시스템 (ServiceLocator로 등록) |
| **Scene** | additive 씬 로드·언로드 오케스트레이션 |
| **Zone** | 게임플레이 컨텍스트 단위; 씬 그룹 소유 |
| **Mode** | Zone 내부의 게임플레이 행동 단위 |

## 패키지 레이아웃

- `ZoneFlow/Assets/ZoneFlowAssets/` — 메인 패키지 (`Runtime/`, `Editor/`, `Tests/`, `.asmdef`)
- 독립 패키지: `{PackageName}Assets/` 동일 레벨

## 주요 의존성

| 패키지 | 비고 |
| --- | --- |
| UniTask | Async/await; git UPM |
| URP | 렌더 파이프라인 |
