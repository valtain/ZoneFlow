# 후보 방향들

## Candidate A: GamePlayMode 라이프사이클 4단계 분리

**상태**: promoted

`ModeState` enum 및 기존 훅(`OnModeInAsync`/`OnModeOutAsync`)과 용어를 통일하여,
페이즈별 명시적 public 메서드를 추가하고 Director에서 순서를 직접 제어한다.

### GamePlayMode 추가 메서드

| 메서드 | 상태 전이 | 내용 |
| --- | --- | --- |
| `ModeOutAsync(ct)` | Active → ModeOut | `OnModeOutAsync` 호출만 (ZoneRelease 없음) |
| `StopCoreAsync(ct)` | ModeOut → Stopped → Destroyed | ZoneRelease 포함, ModeOut 제외 |
| `PlayCoreAsync(director, ct)` | Created → Played | ZoneAcquire + OnPlayed, ModeIn 제외 |
| `ModeInAsync(ct)` | Played → ModeIn → Active | `OnModeInAsync` 호출만 |

SleepAsync / ResumeAsync 동일 패턴:

- `SleepCoreAsync(ct)` — ModeOut 제외한 Sleep 처리
- `ResumeCoreAsync(ct)` — ModeIn 제외한 Resume 처리

### Director ReplaceAsync 재구성

```csharp
if (current != null) await current.ModeOutAsync(ct);
await using var _ = await UiService.Transition<FadeScreen>(ct);
if (current != null) await current.StopCoreAsync(ct);
await next.PlayCoreAsync(this, ct);
// 스코프 종료 → Fade Out
await next.ModeInAsync(ct);
```

StackAsync, PopAsync, ReplaceAllAsync도 동일 패턴으로 재구성.

**장점**:
- 의도 명확, 상태 전이 투명
- HUD 슬라이드인이 화면 공개 후 실행됨 (ExplorationMode.OnModeInAsync 타이밍 자연스러움)
- count 기반 중첩 처리 자동 (`UiTransitionFxLayer`)

**단점**:
- GamePlayMode 수정 범위 큼 (4개 메서드 추가 + StopAndDestroyAsync/PlayAsync 분리)

---

## Candidate B: skipModeOut/skipModeIn bool 파라미터

**상태**: eliminated — 이유: bool 파라미터 설계 냄새, 책임 분리 불명확, 실수 가능성. Candidate A 채택으로 폐기.
