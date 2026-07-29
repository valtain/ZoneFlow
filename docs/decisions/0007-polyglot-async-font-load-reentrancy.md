# ADR-0007: Polyglot 부팅 폰트 로드 async 전환 (재진입 방지)

- **상태**: 채택
- **날짜**: 2026-07-29
- **관련 AQ**: AQ-10 (BACKLOG.md / docs/project-goals.md) — 본 ADR로 Answered

## 맥락

[ADR-0006](0006-polyglot-font-loading-localization-asset-table.md)이 폰트 로딩을 Localization `Asset Table`(`AddressablesFontProvider`)로 결정한 뒤, 실적용에서 부팅 시 재진입 예외가 드러났다:

```
Exception: Reentering the Update method is not allowed.
  ResourceManager.Update → InitializationOperation 완료 콜백
  → AddressableService.EnsureInitializedAsync continuation (동기 재개)
  → FontService.BootAsync → AddressablesFontProvider.LoadAsync
  → GetLocalizedAsset(동기) → WaitForCompletion → ResourceManager.Update 재진입
```

`AddressableService.EnsureInitializedAsync`가 `await LocalizationSettings.InitializationOperation`한 continuation은 **Addressables `ResourceManager.Update` 콜백 스택 안에서 동기 재개**된다. 그 스택에서 provider가 또 동기 `GetLocalizedAsset`(내부 `WaitForCompletion`)을 호출하면 Update를 재진입한다. 콜드스타트(Zone 씬 직접 Play)에서 특히 재현됐고, 심하면 에디터가 행(hang)까지 갔다. Zone-Mode 분리 자체가 아니라 **서비스 부팅 체인의 async 실행 규율**에 닿는 결정이다.

## 결정

**부팅 폰트 로드를 async로 전환한다.** `AddressablesFontProvider.LoadAsync`가 동기 `GetLocalizedAsset` 대신 **`GetLocalizedAssetAsync(...).ToUniTask(ct)`** 를 await한다.

이유: 블로킹 `WaitForCompletion` 호출 자체를 제거하면 어떤 콜백 컨텍스트에서 호출돼도 재진입이 **구조적으로 불가능**하다(우회가 아니라 원인 제거). `Polyglot.asmdef`에 `Unity.ResourceManager`·`UniTask.Addressables` 참조를 추가한다.

## 고려한 대안

| 대안 | 장점 | 단점 / 탈락 이유 |
| --- | --- | --- |
| A (채택) provider async 전환 | `WaitForCompletion` 제거 = 재진입 원인 소거, 모든 호출 컨텍스트에서 안전 | 잠긴 엔진(`PolyglotAssets/`) 수정 + asmdef 참조 2개 추가 |
| B `EnsureInitializedAsync`에 `UniTask.Yield` | 게임 측 1줄, 엔진 무변경, 모든 Addressables 소비자 보호 | 동기 `WaitForCompletion`이 잔존 — 미래 다른 콜백 경로에서 재발 여지(증상 완화) |
| C 동기 로드 유지 | 단순 | 재진입 예외·행 재발 = 버그 존치 |

## 결과

- **강제**: Polyglot 부팅 폰트 로드는 async(`GetLocalizedAssetAsync` await)로 한다. Localization/Addressables 초기화 완료 **콜백 컨텍스트에서 동기 `WaitForCompletion`을 호출하지 않는다**(constraints의 "비동기는 UniTask 전용"의 구체 사례).
- **부팅 순서 불변**: 여전히 `AddressableService.EnsureInitializedAsync` 이후 1회 로드 — swap 없음. async는 블로킹만 제거할 뿐 "부팅 1회" 의미를 바꾸지 않는다.
- **검증**: 정상 부팅(DevBootstrap)·콜드스타트(Intro) 모두 재진입 예외 0, EditMode 5/5. 커밋 `c807f3d`.
- **인접**: AQ-4(Addressable 전환 시 Zone 생명주기)와 같은 로딩 계약 영역이나 별건. [ADR-0006](0006-polyglot-font-loading-localization-asset-table.md)의 후속 정리(FontCatalog↔AssetTable 이중화)는 여전히 미해결.
