# 탐색 결과

**결론**: 존 입장 텔레포트 시 Cinemachine이 워프를 인지하지 못해 카메라가 댐핑·블렌드로 슬라이드하는 것이
"부적절한 자동 카메라 연출"의 근본 원인. 표준 워프 API로 클린 컷 처리하고 가벼운 FOV 리빌을 더해 의도된 진입감을 준다.

**채택된 방향**: Candidate A — 텔레포트 워프 컷 + 가벼운 FOV 리빌
- `PlayerController.Teleport()`에서 이동 전 Follow 타깃 월드 위치를 캡처 → 이동 후 `_vcam.OnTargetObjectWarped(follow, delta)`로 컷.
- `PlayerController.PlayEntryReveal()` — `_vcam.Lens.FieldOfView`를 rest×1.08 → rest로 PrimeTween 0.7s(OutCubic).
- 트리거: `GamePlayMode.ModeInAsync()`에서 중앙화하여 호출(페이드 아웃 후 → 리빌이 완전히 노출됨). `PlayerService.IsReady` 가드.
- `needSpawn`을 `GamePlayMode` 생성자 명시 파라미터로 전환(Exploration/Story=zoneAsset!=null, Shell=false).
- 클린 컷은 `PlayerController.Teleport()` 내부 워프 통지로 처리.

**폐기된 방향**:
- Candidate B(댐핑/블렌드 데이터만 튜닝) — 이유: 일반 플레이 추적까지 딱딱해져 텔레포트와 구분 불가.
- Candidate C(컷만) — 이유: 진입감 부재. A에 리빌 포함하여 흡수.

**후속 Feature 후보**: 없음 (소규모 구현으로 직접 반영, TASK 이슈로 추적).

**CLAUDE.md 반영 필요**: 없음.

**검증 결과**: 컴파일 무에러. 플레이 모드에서 클린 컷(브레인 카메라가 텔레포트 델타만큼 한 스텝 점프)과
리빌(FOV 64.8 펀치 → rest 정착) 모두 확인.
