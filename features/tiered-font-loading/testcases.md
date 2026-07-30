# tiered-font-loading — 검증 시나리오

## Tier-1 (이번 phase)

- [ ] before: WebGL 빌드 `.data` 크기 기록 (폰트 번들 ~25.5M 포함 = 베이스라인)
- [ ] locale별 서브셋 글리프 추출 (String Table + 피커 4라벨 + 메뉴 + ASCII)
- [ ] per-locale Static 서브셋 아틀라스 베이크 (4종, `m_AtlasPopulationMode:0`, 소스 TTF 미포함 확인)
- [ ] boot-tier 카탈로그 + Asset Table `"font-boot"` 엔트리 배선
- [ ] `FontEngine.BootAsync(FontTier)` + `provider.LoadAsync(locale, tier)` 티어 파라미터
- [ ] 부팅 시 boot 티어 적용 — Intro·메뉴·피커가 en/ko/ja/zh에서 tofu 없이 렌더
- [ ] 피커 `ApplyPickerFallbacks`가 boot-tier 카탈로그 참조 (4스크립트 동시 표시 정상)
- [ ] after: WebGL `.data` diff 측정 (목표 ~22M 절감)
- [ ] EditMode green + 컴포넌트 폰트 미직렬화 불변식 유지

## Tier-2 (후속 phase — 이번 범위 밖)

- [ ] content 티어 Remote 그룹 이전 + 초기 `.data` 미포함 확인
- [ ] 콘텐츠 진입 시 선택 locale 폰트 로드·재적용
- [ ] content 부재(오프라인/CORS) 시 boot 티어 유지(floor) + 오류 표면화
