# ADR-0006: Polyglot 폰트 로딩 = Localization Asset Table (AddressablesFontProvider)

- **상태**: 채택
- **날짜**: 2026-07-24
- **관련 AQ**: AQ-9 (BACKLOG.md) — 본 ADR로 Answered

## 맥락

Polyglot 폰트 엔진의 `IFontProvider` seam은 처음부터 로딩 구현 교체(`DirectRefFontProvider` → Addressables)를 위해 설계됐다(features/polyglot, [ADR-0005](0005-first-asmdef-package-boundary-polyglot.md)). 실적용 시점에 **AQ-9**가 남긴 결정을 내려야 했다: `AddressablesFontProvider`가 locale→폰트 로딩을

- **(X)** Unity Localization `Asset Table`(`GetLocalizedAsset<FontRef>`)로 할지 — refcount·preload·per-locale 로딩·에디트 모드 프리뷰를 Localization이 공짜로 제공, "TMP/Localization 최대활용" 원칙 부합.
- **(Y)** raw Addressables + `AssetReference` 자체 카탈로그로 할지 — "Localization 접점 facade 1곳"(ADR-0005 문구) 유지.

두 사용자-지정 원칙("최대활용" vs "접점 1곳")이 여기서 충돌했다.

## 결정

**X — Localization Asset Table을 통해 로드한다.** `AddressablesFontProvider`가 `LocalizationSettings.AssetDatabase.GetLocalizedAsset<FontRef>("Fonts", "font", locale)`로 locale별 `FontRef`를 로드한다.

이유: refcount·preload를 자체 구현하는 것은 회피 대상인 "중복 재구현"이고, "접점 1곳"의 **실질 의도는 호출부·게임 어댑터가 TMP/Localization을 직접 만지지 않는 것**인데, provider는 Polyglot 내부 seam이라 그 의도가 그대로 유지된다(FontService·IntroScreen 등 게임 코드는 여전히 Localization 미접촉).

## 고려한 대안

| 대안 | 장점 | 단점 / 탈락 이유 |
| --- | --- | --- |
| X (채택) Localization Asset Table | refcount·preload·에디트 프리뷰 공짜, "최대활용" 원칙 부합, provider가 seam이라 격리 유지 | Localization 접점이 facade + provider 2곳(문구 리프레임 필요) |
| Y raw Addressables + AssetReference 카탈로그 | 접점이 facade 1곳으로 문자 그대로 유지 | refcount/preload/로딩을 재구현 = "중복 재구현", 코드↑ |
| DirectRef 유지 | 단순 | 전 폰트 eager-load, 빌드 스트립·per-locale 로딩 불가 |

## 결과

- **강제**: 폰트 로딩은 Localization `Asset Table`("Fonts" 컬렉션, 키 `"font"`, locale별 `FontRef`) 경유. 부팅 provider = `AddressablesFontProvider`.
- **리프레임(ADR-0005 갱신)**: "TMP·Localization 접점 = facade **1곳**" → "**facade + provider 두 지정 seam**". 불변식의 본질은 유지 — **게임 코드·호출부**는 TMP/Localization 타입 직접 미접촉, Polyglot 내부 seam(facade=TMP Settings·locale 조회, provider=폰트 로딩)만 접촉.
- **seam 청결 유지**: `FontEngine`·facade·`FontSet`은 `IFontProvider`/`FontSet`만 의존 → provider 교체가 통째 교체로 끝남(교체비용 0). #105 테스트는 `DirectRefFontProvider` 직접 사용이라 무영향.
- **존치(후속 정리)**: `DirectRefFontProvider`(테스트)·`FontCatalog`(피커 fallback `AllFonts()`)는 유지. FontCatalog↔AssetTable 이중화 통일은 별도 후속.
- **관련**: Localization은 Asset Table을 Addressables로 적재 → [ADR? / 메모리 localization-pulls-addressables]. AQ-4(Addressable 전환 시 Zone 생명주기)와 인접하나 별건.
