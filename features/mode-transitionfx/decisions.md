# Decisions: mode-transitionfx

## D1. 네이밍: ModeOutAsync / ModeInAsync

`ExitAsync`/`EnterAsync` 대신 `ModeOutAsync`/`ModeInAsync` 채택.
`ModeState` enum 및 기존 훅(`OnModeInAsync`/`OnModeOutAsync`)과 용어 통일.

## D2. TransitionFx 삽입 위치: Director 레벨

GamePlayMode의 개별 훅이 아닌 Director 레벨에서 삽입.
ModeIn/ModeOut이 기존 메서드 내부에 포함되어 있어 분리 필요.

## D3. 적용 범위: PanelMode 포함 전체

오버레이 패널 전환(PanelMode)에도 블랙 페이드 일괄 적용.
