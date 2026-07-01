---
name: zoneflow-mode-stack-push-pop-contract
description: GamePlayDirector의 stack push/pop이 Zone 스왑·재활성·위치복원을 이미 지지하나 결과 전달 채널은 없음 — P5 던전-전투 루프의 골격 기반
metadata:
  type: project
---

`GamePlayDirector` (Assets/ZoneFlowAssets/Runtime/GamePlay/GamePlayDirector.cs)의 stack switch push/pop 계약:

- `StackAsync`(151-166행): push되는 Mode의 `IsOverlay`가 false면 직전 Mode를 `SleptAsync(keepZoneActive:false)` → 직전 Zone `SetActive(false)`. push Mode가 자기 ZoneAsset을 AcquireAsync → **Zone 스왑**(오버레이 아님). PanelMode만 `IsOverlay=true`라 아래 Zone 유지.
- `PopAsync`(196-215행): 현재 Mode를 Stopped→Destroyed(Zone Release, refcount 0이면 씬 언로드) → 직전 Mode `ResumedAsync`(Zone 재활성 + `SpawnPlayer()`로 Slept 시 저장한 위치 재스폰).
- **결손**: pop에 결과를 실을 채널이 없다. `NavigationRequest.Parse`의 pop 분기(NavigationRequest.cs 63-67행)는 쿼리를 버리고 `(Pop, default, null, null)` 생성. `OnResumedAsync`(GamePlayMode.cs 143행)는 파라미터 없음.

**Why:** P5형 던전→턴제전투→복귀 루프가 코드 변경 없이 골격 위에 성립하는지의 핵심 근거. battle Zone을 별도 ZoneAsset으로 두면 stack push/pop이 그대로 루프를 만든다. 전투 결과(승/패/도주+페이로드)는 URI가 아니라 모드 간 결과 채널(BattleService 보관 → Resume된 모드가 pull)로 전달해야 훅 시그니처를 안 건드린다.

**How to apply:** BattleMode 구현·전투 복귀 계약 설계 시 이 계약을 기준선으로 삼는다. "OnResumedAsync에 result를 넘기자"는 제안은 훅 시그니처 변경을 유발하므로 pull 모델을 우선 검토. [[zoneflow-persona5-pivot]] 참조.
