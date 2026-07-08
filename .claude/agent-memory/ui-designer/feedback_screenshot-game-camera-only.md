---
name: screenshot-game-camera-only
description: unity_screenshot_game은 카메라 렌더만 캡처 — Screen Space Overlay Canvas UI는 찍히지 않음
metadata:
  type: feedback
---

`unity_screenshot_game` MCP 도구는 지정 카메라(기본 Camera.main)의 렌더 출력만 캡처한다. `RenderMode.ScreenSpaceOverlay` Canvas(패널 대부분이 이 모드)는 카메라 렌더 이후 화면에 합성되는 레이어라 이 도구로는 **찍히지 않는다** — Play Mode에서도 배경(스카이박스/지형)만 나오고 UI가 전혀 안 보이는 결과가 재현됨(BattlePanel QA 중 확인).

**Why**: 카메라 기반 캡처(RenderTexture 등)와 최종 화면 합성(overlay canvas 포함)은 다른 스테이지. 도구는 전자만 커버.

**How to apply**: ScreenSpaceOverlay 패널의 픽셀 단위 시각 검증이 필요하면 이 도구로는 불가 — `unity_screenshot_editor_window`(GameView 탭, Windows 전용·PrintWindow 기반)를 쓰거나, 사용자에게 직접 확인을 요청한다. 이 도구로 검증 가능한 것은 **World Space Canvas**(빌보드 라벨 등, [[billboard-label-cinemachine-camera]] 참고)나 실제 3D 씬 콘텐츠뿐. 패널 저작 시 시각 검증 불가 상황이면 대신 프리팹 하이러키·직렬화 필드(`m_Script` 포함)·런타임 API 호출(execute_code로 Initialize 등 직접 호출해 예외 없이 자식 오브젝트가 기대대로 생성되는지) 구조적 검증으로 대체하고, 결과 보고에 "픽셀 스크린샷 불가, 구조 검증으로 대체" 명시.
