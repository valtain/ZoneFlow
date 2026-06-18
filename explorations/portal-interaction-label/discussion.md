# 탐색 로그

- [2026-06-18 | start] 탐색 시작. 질문: 플레이어에게 IInteractable(Portal 등) Label을 보기 편하게 보여주는 근접/조준 상호작용 프롬프트 시스템.

- [2026-06-18 | brainstorm] 코드베이스 조사 결과:
  현재 Portal "Label"은 `SceneSetupTool.cs:159-168`에서 에디터 툴이 만든 월드 공간 `TextMeshPro` 자식.
  `tmp.text = portalId`(기술 ID 원문), 빌보드 없음, fontSize 8 흰색 단색 → 각도/거리 가독성 취약.
  Portal은 `OnTriggerEnter` 자동 발동(`Portal.cs:21-28`) — "다가가서 본다" 흐름 부재.
  `IInteractable`은 `InteractableId` + `OnInteractAsync` 뿐 → 표시 명칭 멤버 없음(공통 전제로 추가 필요).
  UI 인프라: `UiService.Floating` 레이어가 프롬프트에 최적, `UiPanel`+PrimeTween+TMP, 참고 패턴 `ExplorationHudPanel`.
  입력은 Move/Sprint만(Interact/Look 없음), 카메라는 TPS(Cinemachine) → 근접 감지가 관행적. 로컬라이즈 패키지 없음.
  도출 후보: [A] 스크린 HUD 프롬프트(추천) / [B] 월드 빌보드 라벨 개선 / [C] 하이브리드(월드 마커+스크린 프롬프트).

- [2026-06-18 | decision] 사용자 확정: 대상=플레이어(인게임), 범위=근접/조준 상호작용 프롬프트 시스템.
  추천: [A] 스크린 HUD 프롬프트 + `IInteractable.DisplayLabel` 전제 + 근접 감지.
  근거: "보기 편하게"(가독성)가 핵심 불만 → 스크린 공간이 가장 직접적으로 해결. [C]는 발견성 필요 시 후속 확장.

- [2026-06-18 | close] 탐색 완료.
