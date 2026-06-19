# 탐색 결과

**결론**: demo-mvp 씬 구조를 **C3 → C2 단계결합**으로 개선한다. 명제 A 증명의 핵심인 *씬 단위
관찰 가능한 Zone 전환*을 보존하기 위해 **village·dungeon은 별도 씬으로 유지**하고, "단절"감은
1단계에서 연출·리테마로, 2단계에서 허브 구조로 단계적으로 해소한다.

**채택된 방향**:

- **1단계 — C3 (즉시 적용)**: 2씬 구조 유지 + 리네임 + 전환연출 + 리테마.
  - **리네임** `World1→Village`, `World2→Dungeon` — 변경 지점:
    - `.unity` 파일명 (+ `.meta`는 그대로 이동 → GUID 보존, 참조 안 깨짐)
    - `ProjectSettings/EditorBuildSettings.asset` 의 path 문자열
    - `ZoneAssetCatalog.SceneName` (`World1`→`Village`, `World2`→`Dungeon`)
    - `SceneSetupTool.cs` 하드코딩 `"World1"/"World2"` 문자열 및 메뉴 항목명
  - **전환연출**: `mode-transitionfx`의 `UiService.Transition<FadeScreen>` 인프라를 Zone/Portal
    내비게이션 경로(GamePlayDirector의 NavigationRequest 처리)에 연결 → 암전→이동→복귀로 끊김 흡수.
  - **리테마**: 원시 Plane/Cube를 시나리오 에셋(Low Poly Dungeon 등)으로 교체, 환경 스케일 확대.
  - **레거시 정리**: 씬에서 레거시 Zone 루트(`world1`,`world1_b`,`story_w1`,`world2`,`world2_b`)
    제거 후 `CatalogBaker` re-bake → 3개 카탈로그 동기 정리. **`intro`는 부트 진입점이므로 보존**.

- **2단계 — C2 (demo-boss에서 증설)**: `Overworld` 허브 씬을 추가해 village/dungeon/BossRoom을
  포털 갈림길로 연결. 본 exploration 범위 밖, 후속 데모 단계에서 도입.

**폐기된 방향**:

- **C1 (단일 연결 1씬)** — 보류. 같은 씬 내 Zone 전환은 `ZoneRegistry`상 SetActive 토글뿐(씬
  언로드 X)이라 명제 A의 전환 관찰력이 약화됨.
- **C4 (연속 지형 + 논리 Zone)** — 폐기. 전 항목 항상-로드라 Zone load/unload 자체가 사라져 명제 A
  증명 소실.

**작업 분담**: Claude = 리네임·코드 문자열·`FadeScreen` 연결·레거시 정리(씬 Zone 제거 + re-bake) /
개발자 = 시나리오 에셋 임포트·리테마 배치·조명.

**후속 Feature 후보**:

1. **demo-mvp-scene (1차/C3)** — 리네임 + Portal 전환 FadeScreen 연결 + 레거시 ZoneId 정리.
   리테마는 개발자 배치 가이드로 동반.
2. demo-boss 단계에서 **C2 Overworld 허브** 증설.

**CLAUDE.md 반영 필요**: 없음. (아키텍처 원칙 변경 없음 — 별도 씬=Zone 전환 관찰 보장 제약은 기존
scene-hierarchy/constraints 문서로 충분히 커버됨)
