# interaction-prompt — 검증 시나리오

## 자동화 (PlayMode — `InteractionDetectorTests`)

디텍터 로직을 자동 검증한다. 실행: Unity Editor → Window > General > Test Runner > PlayMode.

- [x] 사정권(반경 4) 진입 시 최근접 IInteractable 추적·1회 통지 — `Detector_DetectsInteractableInRange` (testcase 1 토대)
- [x] 사정권 이탈 시 추적 해제·`null` 통지 — `Detector_ClearsWhenOutOfRange` (testcase 2 토대)
- [x] 여러 대상 중 최근접 선택·이동 시 전환 — `Detector_SelectsNearest_AndSwitchesOnMove` (testcase 3)

## 수동 (Unity Editor Play 모드 — 화면 확인)

프롬프트 표시·페이드·가독성은 시각적 항목이라 Play 모드에서 직접 확인한다.
사전 준비: 프롬프트 프리팹·PanelCatalog 등록·Floating Canvas·Player에 InteractionDetector 부착·Portal 마이그레이션(`ZoneFlow/Interaction/Migrate Portals`).

- [ ] 플레이어가 Portal 사정권에 진입하면 DisplayLabel 프롬프트가 화면에 페이드 인 (testcase 1)
- [ ] 사정권을 벗어나면 프롬프트가 페이드 아웃 (testcase 2)
- [ ] 근처에 IInteractable이 여러 개면 최근접 대상의 라벨만 표시 (testcase 3)
- [ ] DisplayLabel 미설정 Portal → InteractableId로 폴백 표시 (testcase 4)
- [ ] 어떤 카메라 각도/거리에서도 텍스트가 읽기 쉽다 (testcase 5)
