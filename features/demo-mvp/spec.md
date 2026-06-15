# demo-mvp — 설계 스펙

## 목표

명제 A 증명 — Village→Dungeon Zone 전환 중에도 ExplorationMode가 스택 변경 없이 유지됨을 플레이로 확인. 데모 MVP의 1차 단계.

## 범위

이동/Zone 전환만. Story·Battle·Boss·연출은 후속 feature(demo-battle/demo-story/demo-boss/demo-polish)로 제외.

## 주요 컴포넌트 (대부분 기존 재사용)

| 컴포넌트 | 상태 | 설명 |
| --- | --- | --- |
| ExplorationMode | 완성 | 기존 구현, 플레이어 제어 + HUD 표시 |
| Portal.cs | 완성 | NavigationUri + OnInteractAsync 기반 상호작용 |
| ZoneRegistry | 기존 | Zone 참조 카운팅 생명주기 관리(Acquire/Release) |
| Navigation URI 시스템 | 기존 | `gameplay://exploration/dungeon` 형식 파싱 |
| ZoneAssetCatalog | 기존 | Zone→씬/프리팹 매핑 (신규 등록 필요) |
| SpawnPointCatalog | 기존 | Zone별 SpawnPoint 조회 (신규 등록 필요) |

## 신규/변경 작업

1. **ZoneAssetCatalog에 Village/Dungeon ZoneId 등록**: 기존 World1/World2 씬 재활용, ZoneId↔씬명 매핑 추가
2. **SpawnPointCatalog 등록**: SP_Entrance 등 각 Zone별 SpawnPoint 정의
3. **Portal_ToDungeon NavigationUri 설정**: `gameplay://exploration/dungeon`으로 정의 (역방향 Portal_ToVillage도 `gameplay://exploration/village` 설정)

## 데이터 흐름

```
Village 시작
  ↓
ExplorationMode Active (플레이어 제어)
  ↓
Portal_ToDungeon 진입 (OnInteractAsync 호출)
  ↓
NavigationUri 파싱 (`gameplay://exploration/dungeon`)
  ↓
GamePlayDirector가 NavigationRequest 처리:
  - Dungeon Zone 로드 (World2 씬, ZoneAssetCatalog 참조)
  - Village Zone 언로드 (World1 씬)
  ↓
Mode 스택 변화: 없음 (ExplorationMode 유지, Push/Pop/Replace 없음)
  ↓
플레이어를 SP_Entrance에 배치 (SpawnPointCatalog 참조)
  ↓
ExplorationMode 계속 실행 (명제 A 증명 완료)
```

## 작업 분담

| 담당 | 작업 |
| --- | --- |
| Claude | ZoneAssetCatalog에 Village/Dungeon 신규 등록, SpawnPointCatalog 등록, Portal_ToDungeon/Portal_ToVillage의 NavigationUri 설정 (ScriptableObject 데이터) |
| 개발자(Unity Editor) | 씬 오브젝트 배치: SpawnPoint 트리거 위치, Portal 트리거 위치/크기, 플레이어 초기 위치 설정 |
