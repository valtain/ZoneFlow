# Feature: polyglot — 다국어 TMP 폰트 엔진

## 목표 / 검증 의도

인게임 텍스트를 여러 언어(KO/JP/CN/EN)로 **자형 혼입·폰트 오염 없이** 렌더한다. locale은 부팅 시 1회 결정되고 런타임 swap이 없다는 점을 이용해, 부팅 때 TMP Settings(locale별 기본 폰트 + 전역 fallback + locale 스타일시트)를 한 번 세팅한다. 이번 슬라이스가 확립할 명제:

- **패키지 경계**: 폰트 엔진을 `ZoneFlowAssets`와 분리된 **순수 엔진 패키지 `Polyglot`**(`Assets/PolyglotAssets/`)로 세운다. 게임 측은 얇은 어댑터로만 부팅 시 호출한다.
- **패키지 변경 유연성**: TextMeshPro·Unity Localization의 API 접점을 패키지 내부 **어댑터/facade 한 곳에 격리**해, 두 패키지 버전 변경 시 수정 범위를 1지점으로 좁힌다.
- **오염 차단**: 컴포넌트는 폰트를 serialize하지 않는다(locale 기본 폰트 상속 + Style만 지정). 저작 제어·저장 가드로 폰트 직렬화·동적 글리프 오염을 원천 차단한다.

런타임 문자열 지역화(대사·UI 텍스트 번역)와 Addressables 실적용은 이 feature 범위 밖 — seam/후속으로 분리한다.

## 관련 배경

- **Source 탐색**: [tmp-multilingual-font-engine](../../explorations/tmp-multilingual-font-engine/findings.md) — Candidate A 채택.
- **첫 asmdef 경계**: 프로젝트 최초로 프로젝트 소유 asmdef를 도입한다(ADR-0005 후보). 현재는 전부 `Assembly-CSharp` 단일 어셈블리.

## 패키지 경계 (핵심)

```text
Assets/PolyglotAssets/  (asmdef: Polyglot — 순수 엔진, MonoBehaviour 서비스 무의존)
   ▲  참조
   │  (Assembly-CSharp은 모든 asmdef를 자동 참조)
Assets/ZoneFlowAssets/  (Assembly-CSharp)
   └ FontService : MonoService<FontService>   ← 얇은 게임 측 어댑터, 부팅 1회 Polyglot 호출
```

- **제약**: asmdef 어셈블리는 `Assembly-CSharp`(→ `MonoService`/`CoreServices`)를 **역참조할 수 없다**. 따라서 `Polyglot`은 `MonoService`에 의존하지 않고, 부팅 진입점만 게임 측 어댑터가 소유한다.

## 범위

**In scope**
- `Polyglot` 패키지 스켈레톤: UPM 레이아웃(`Runtime`/`Editor`/`Tests`) + asmdef 4종 + `package.json`.
- 부팅 엔진 API: 영구 locale → 폰트 로드(seam) → TMP Settings(locale별 기본 폰트 + 전역 fallback + 스타일시트) 적용. 런타임 swap 없음.
- seam: `IFontProvider`(지금 `DirectRefFontProvider`, 나중 `AddressablesFontProvider`) + TMP·Localization을 감싸는 facade.
- 데이터: `FontRef` / `FontCatalog`(SO) 스켈레톤, 언어별 `TMP_StyleSheet` 규약.
- 게임 측 얇은 어댑터 `FontService`(ZoneFlowAssets) — 부팅 훅에서 Polyglot 호출.
- 저작 제어: Font Asset 필드 잠금·Style-only + 저장 가드(폰트 자동 스트립·검증), 오염 방지(`OnWillSaveAssets` `ClearFontAssetData()` + Clear Dynamic Data on Build).
- 에디터 프리뷰: Unity Localization(`LocalizedTmpFont` + Game View locale 스위처).

**Out of scope (후속)**
- Addressables 실적용(seam만).
- 런타임 문자열 지역화(대사·UI 번역 텍스트).
- **Candidate C(컴포넌트별 Localization Property Variants로 폰트 배선)** — 폐기, 재도입 금지.
- 영구 저장 스토어 최종 확정(→ systems-designer, Localization 내장 우선).

## 구현 원칙: TMP/Localization 최대 활용 + 변경 격리

- **Localization 활용**: `LocalizationSettings`·Locale 선택·**내장 selected-locale 영속화**(StartupLocaleSelector) → 탐색이 미뤄둔 커스텀 PlayerPrefs 스토어를 대체(systems-designer 최종 확인). 프리뷰도 Localization 네이티브.
- **TMP 활용**: `TMP_Settings` 기본 폰트/전역 fallback·언어별 `TMP_StyleSheet`·`TMP_FontAsset` — 폰트 배선 실체는 TMP 네이티브로 처리.
- **변경 격리**: `TMPro.*`·`UnityEngine.Localization.*` 직접 호출을 패키지 내부 facade 1곳에 가둔다. 호출부·게임 어댑터는 TMP/Localization 타입을 직접 만지지 않는다.
- **가드레일**: "최대 활용"이 Candidate C를 재도입하지 않는다 — 컴포넌트 폰트 미직렬화 불변식 유지.

## 데이터 흐름 (부팅 1회)

```text
Localization selected-locale (영구)
        │  (미선택 시 기본 locale)
        ▼
IFontProvider.Load(locale)  ── seam ── DirectRefFontProvider (지금) / AddressablesFontProvider (나중)
        │
        ▼
TMP facade: TMP_Settings 기본 폰트 = locale CJK 패밀리, 전역 fallback = [localeCjkFont, symbolFont], 활성 스타일시트 = locale StyleSheet
        │
        ▼
이후 모든 TMP 컴포넌트는 폰트 미지정 → locale 기본 폰트 상속, Style만 적용
```

## 작업 분해 (tasks)

`/feature plan polyglot`이 `tasks.md`에 채운다. 권장 순서: 패키지 스켈레톤/asmdef → seam 인터페이스(`IFontProvider`·TMP/Localization facade) → `FontRef`/`FontCatalog` SO → 부팅 엔진 API → 게임 측 `FontService` 어댑터·부팅 배선 → 저작 제어·저장 가드(Editor) → 오염 방지 가드 → Localization 프리뷰 → 테스트.

## 검증 방법

- **컴파일**: `unity_get_compilation_errors` 에러 0 — Polyglot asmdef(+ TMP·Localization 참조) 및 게임 어댑터.
- **부팅**: 지정 locale로 부팅 시 해당 CJK 패밀리가 기본 폰트로 적용되고, 다른 locale 자형이 혼입되지 않음(Game View 프리뷰 스위처로 확인).
- **오염 불변식**: 프리팹/컴포넌트에 폰트 asset이 serialize되지 않음(저장 가드 후 diff 확인).
- **패키지 경계**: `Polyglot` 어셈블리가 `Assembly-CSharp`를 참조하지 않음(순수 엔진 유지) — asmdef 참조 그래프로 확인.
