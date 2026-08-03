# tiered-font-loading — 검증 시나리오

## Tier-1 (이번 phase)

- [x] before: WebGL 빌드 `.data` 크기 기록 (폰트 번들 ~25.5M 포함 = 베이스라인) — #109
- [x] locale별 서브셋 글리프 추출 (String Table + 피커 4라벨 + 메뉴 + ASCII) — #111
- [x] per-locale Static 서브셋 아틀라스 베이크 (4종, `m_AtlasPopulationMode:0`, 소스 TTF 미포함 확인) — #112
- [x] boot-tier 카탈로그 + Asset Table `"font-boot"` 엔트리 배선 — #112·#113
- [x] `FontEngine.BootAsync(FontTier)` + `provider.LoadAsync(locale, tier)` 티어 파라미터 — #110
- [x] 부팅 시 boot 티어 적용 — Intro·메뉴·피커가 en/ko/ja/zh에서 tofu 없이 렌더
- [x] 피커 `ApplyPickerFallbacks`가 boot-tier 카탈로그 참조 (4스크립트 동시 표시 정상)
- [x] after: 번들 diff 측정 — **측정 완료, 목표 미달**(아래 결과 참조, ~22M은 Tier-2로 이월)
- [x] EditMode green (9/9) / **컴포넌트 폰트 미직렬화 불변식 위반 잔존** → #116으로 분리

## Tier-1 검증 결과 (#115)

측정 = WebGL 타겟 Addressables 콘텐츠 빌드 산출물(`Library/com.unity.addressables/aa/WebGL/WebGL/*.bundle`). WebGL 압축이 Gzip이므로 전송량 프록시로 `gzip -9`를 함께 기록. 전체 플레이어 빌드는 하지 않아 `.data` 총량은 측정 범위 밖.

### 크기 — Tier-1 단독으로는 절감 없음(증가)

| 번들 | before raw | before gzip | after raw | after gzip | Δgzip |
| --- | ---: | ---: | ---: | ---: | ---: |
| localization-assets-en | 1,400,891 | 1,227,516 | 2,189,839 | 1,920,395 | +692,879 |
| localization-assets-ko | 6,002,670 | 5,456,190 | 6,267,624 | 5,694,012 | +237,822 |
| localization-assets-ja | 6,532,979 | 5,973,364 | 6,838,741 | 6,248,431 | +275,067 |
| localization-assets-zh-hans | 12,490,343 | 11,447,709 | 12,782,503 | 11,713,329 | +265,620 |
| **합계** | **26,426,883** | **24,104,779** | **28,078,707** | **25,576,167** | **+1,471,388** |

원인: `Localization-Assets-<locale>` 그룹에 content `FontRef_*`가 **Local + `IncludeInBuild:1`** 로 남아 있어 TTF 번들이 전량 유지된 채 boot 서브셋이 **추가**된다. content 티어를 초기 다운로드에서 빼는 것 = Remote 이전 = Tier-2(이 phase의 out of scope).

### Tier-2 잠재치 스파이크 (측정 후 되돌림)

content `FontRef_*` 4개만 `IncludeInBuild:0` 그룹으로 이동 후 재빌드:

| | after raw | after gzip | 스파이크 raw | 스파이크 gzip | Δgzip |
| --- | ---: | ---: | ---: | ---: | ---: |
| **합계(4 번들)** | 28,078,707 | 25,576,167 | 1,178,464 | 1,029,029 | **−24,547,138** |

→ Tier-2에서 실현 가능한 절감 **≈24.5M(gzip)** 으로 목표 ~22M을 **상회**함을 확정. Tier-1이 만든 boot 티어가 그 절감의 전제(콘텐츠 폰트 없이도 부팅 UI가 렌더됨)이며, 절감 자체는 Tier-2에서 실현된다.

### 렌더 / 불변식 / 경계

- **렌더 통과** — DevBootstrap→Play 정상 부팅 경로. 피커에 `English/한국어/日本語/简体中文` 4스크립트 동시 렌더, 4 locale 메뉴·태그라인 모두 tofu 없음(스크린샷 5장 확인).
- **불변식 일부 실패** — boot 서브셋 4종은 Static + `sourceFontFile == null` 통과(`BootFontTableTests`). 반면 컴포넌트 폰트 미직렬화는 `MenuPanel.prefab`(6) / `Intro.unity`(3) / `LocalizationDemo.unity`(10) 19건 위반 잔존. 수동 스트립 후 저장해도 디스크에서 원본 폰트로 복원되는 저장 파이프라인 문제 → **#116**.
- **경계 통과** — EditMode 9/9 green, 부팅·locale 전환 흐름 정상.

## Tier-2 (후속 phase — 이번 범위 밖)

- [ ] content 티어 Remote 그룹 이전 + 초기 `.data` 미포함 확인
- [ ] 콘텐츠 진입 시 선택 locale 폰트 로드·재적용
- [ ] content 부재(오프라인/CORS) 시 boot 티어 유지(floor) + 오류 표면화
