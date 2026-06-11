# Test Cases: Story HUD

## TC-01: HUD 생성 타이밍

- StoryMode OnPlayedAsync 후 StoryHudPanel이 MainView에 생성됨
- 생성 직후 HUD는 숨김 상태 (슬라이드아웃 위치)

## TC-02: Zone 정보 표시

- Initialize(zone) 호출 후 ZoneNameLabel에 "ZoneId@SceneName" 형식으로 표시됨

## TC-03: 슬라이드인 연출

- OnModeInAsync 호출 시 배너가 상단에서 슬라이드인됨
- 연출 완료 후 배너가 정위치에 표시됨

## TC-04: 슬라이드아웃 연출

- OnModeOutAsync 호출 시 배너가 슬라이드아웃됨
- 연출 완료 후 배너가 화면에서 사라짐

## TC-05: HUD 파괴 타이밍

- OnStoppedAsync 호출 후 StoryHudPanel 인스턴스가 파괴됨
- MainView 레이어가 비어 있음

## TC-06: Sleep/Resume 사이클

- Mode Sleep(ModeOut) → Resume(ModeIn) 시 HUD 슬라이드아웃 후 슬라이드인 정상 동작
