# Exploration HUD — 탐색 질문

> ExplorationMode OnModeIn/Out 훅 기반 HUD 연결 — 연출과 데이터 바인딩을 어떻게 설계할 것인가?

## 컨텍스트

ModeIn/Out은 원래 UI를 적확한 타이밍에 로딩하기 위해 설계된 상태이다. ExplorationMode는 현재 OnModeInAsync에서 플레이어 스폰만 처리하며 HUD가 없다. UI 시스템(UiService + 레이어 + UiPanel)은 갖춰져 있으나 HUD 전용 경로가 없다.

확정 사항:
- 연결 지점: `ExplorationMode.OnModeInAsync` → Show, `OnModeOutAsync` → Hide
- 레이어: `UiMainViewLayer` (Floating은 알림 예약)
- Sleep/Resume은 ModeOut/ModeIn을 통해 처리되므로 별도 훅 불필요
- 연출: 단순 fade 아님 — 스르릉 슬라이드인/아웃 + 스태거

## 탐색 범위

- HUD 패널 구조 설계 (UiPanel 기반 or 별도 MonoBehaviour)
- UiMainViewLayer API 확장 방식
- 플레이어 스탯 데이터 모델 설계 및 바인딩 방식
- 존/씬 정보 노출 방식 (GamePlayMode.ZoneAsset 접근)
- HUD 연출 구현 방식 (DOTween 시퀀스, UniTask 등)

Out of scope: BattleMode/StoryMode HUD, 미니맵, 인벤토리 UI

## 성공 기준

- HUD가 ExplorationMode 진입 시 다이나믹하게 나타나고 퇴장 시 사라지는 구조가 결정됨
- 플레이어 체력/스탯바와 존 정보 바인딩 방식이 결정됨
- UiMainViewLayer 확장 방향이 결정됨
- feature issue 생성 준비 완료
