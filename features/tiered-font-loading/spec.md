# tiered-font-loading — 설계 스펙

## 목표 / 검증 의도

WebGL 초기 다운로드에 실리는 폰트 **~25.5M(실측)** 을 **2단 폰트 로딩**으로 축소한다. **Tier-1(per-locale Static 서브셋·Local)을 먼저** 적용해 즉시 ~22M을 줄이고(원격 호스팅 무의존, 리스크 낮음), **Tier-2(Remote per-locale 전체 폰트)는 후속 phase**로 분리한다. 근거·후보 비교: [findings](../../explorations/tiered-font-loading/findings.md) (AQ-11).

## 범위

**In scope (Tier-1)**
- boot-tier **per-locale Static 서브셋 FontRef 4종**(Intro·메뉴·피커 라벨·ASCII 고정 글리프) + Asset Table `"font-boot"` 엔트리(boot-tier 카탈로그).
- **서브셋 베이크 툴(Editor)**: locale String Table + 피커 4라벨 + 메뉴 문자열 + ASCII → Static `TMP_FontAsset`(`m_AtlasPopulationMode:0`, Custom Characters). TTF 미포함.
- **티어 기구 1a**: `AddressablesFontProvider.LoadAsync(locale, tier)` + `FontEngine.BootAsync(FontTier)` — 엔트리키 티어 선택자.
- `FontService` 부팅 시 boot 티어 적용. 피커 `ApplyPickerFallbacks`가 boot-tier 카탈로그를 가리키게.
- before/after WebGL `.data` 크기 diff 측정.

**Out of scope (후속 phase = Tier-2)**
- Remote per-locale 그룹 이전 + 호스팅(CDN) 배선.
- content 티어 로드/릴리스 lifecycle(ContentServices 경계, AQ-4 사례).
- Tier-2 아틀라스 모드 **C(Dynamic) vs E(Static 베이크)** 결정.
- content 부재 시 "boot=floor" 불변식 ADR.

## 주요 컴포넌트

- **Boot-tier FontCatalog** — per-locale 서브셋 FontRef(각자 소스 폰트로 베이크한 Static 아틀라스). 공유 아틀라스 불가(피커 4스크립트 동시 표시 + Han-unification).
- **서브셋 베이크 툴** — 글리프 추출 + Static 폰트 에셋 생성(재베이크 트리거 = 해당 고정 문자열 변경 시).
- **FontEngine / AddressablesFontProvider** — `FontTier` 파라미터 추가(기존 재부팅 seam ADR-0007/0008 재사용, 프로바이더 stateless 유지).
- **FontService** — boot 티어 부팅 적용.

## 데이터 흐름 (Tier-1)

```text
부팅 → FontService.BootAsync(Tier.Boot)
     → provider.LoadAsync(locale, Boot) → Asset Table "font-boot" 엔트리(locale 서브셋 FontRef)
     → facade.Apply → Intro·메뉴·피커 즉시 렌더 (작은 Local 아틀라스, TTF 없음)
[후속 phase] 콘텐츠 진입(ContentServices) → Content 티어 로드(Remote) → 재적용
```

## 검증 방법

- **크기**: before/after WebGL `.data` diff — 목표 ~22M 절감. (탐색 실측 베이스라인: 폰트 번들 en 1.4 / ko 5.8 / ja 6.3 / zh 12M.)
- **렌더**: 피커 4스크립트 + 메뉴가 en/ko/ja/zh에서 tofu 없이 렌더(per-locale 서브셋).
- **불변식**: 서브셋 아틀라스에 소스 TTF 미포함(Static) + 컴포넌트 폰트 미직렬화 유지.
- **경계**: 기존 부팅 흐름·재부팅 seam 정상, EditMode green.
