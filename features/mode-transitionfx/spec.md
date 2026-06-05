# Feature: mode-transitionfx

## 목표

Mode 전환 시 화면 전환 효과(TransitionFx)를 적용한다.

## 목표 시퀀스

```text
ModeOut [화면 노출] → [Fade In / 블랙] → Stop/ZoneRelease → ZoneLoad/OnPlayed → [Fade Out] → ModeIn [화면 노출]
```

- PanelMode 포함 전체 Mode 전환(Replace/Stack/Pop/ReplaceAll)에 일괄 적용
- 효과: `FadeScreen` (duration 0.3s, 기존 인프라 사용)

## 설계 요약

### 문제

`OnModeOutAsync`는 `StopAndDestroyAsync` 내부 맨 앞에,
`OnModeInAsync`는 `PlayAsync` 내부 맨 끝에 포함되어 있어,
Director 레벨 단순 래핑 불가.

### 해결: GamePlayMode 라이프사이클 4단계 분리

`GamePlayMode`에 페이즈별 public 메서드를 추가하여 Director가 순서를 직접 제어한다.

| 메서드 | 상태 전이 | 내용 |
| --- | --- | --- |
| `ModeOutAsync(ct)` | Active → ModeOut | `OnModeOutAsync` 호출만 |
| `StopCoreAsync(ct)` | ModeOut → Stopped → Destroyed | ZoneRelease 포함, ModeOut 제외 |
| `PlayCoreAsync(director, ct)` | Created → Played | ZoneAcquire + OnPlayed, ModeIn 제외 |
| `ModeInAsync(ct)` | Played → ModeIn → Active | `OnModeInAsync` 호출만 |
| `SleepCoreAsync(ct)` | ModeOut → Slept | ModeOut 제외 |
| `ResumeCoreAsync(ct)` | Slept → Resumed → Played | ModeIn 제외 |

### Director 재구성 패턴 (ReplaceAsync 기준)

```csharp
if (current != null) await current.ModeOutAsync(ct);
await using var _ = await UiService.Transition<FadeScreen>(ct);
if (current != null) await current.StopCoreAsync(ct);
await next.PlayCoreAsync(this, ct);
// 스코프 종료 → Fade Out
await next.ModeInAsync(ct);
```

## 참조

- 탐색: `explorations/mode-transitionfx/`
- TransitionFx 인프라: `Runtime/Ui/TransitionFx/`, `Runtime/Ui/Layers/UiTransitionFxLayer.cs`
- 구현 대상: `Runtime/GamePlay/Mode/GamePlayMode.cs`, `Runtime/GamePlay/GamePlayDirector.cs`
