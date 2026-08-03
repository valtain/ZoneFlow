# tiered-font-loading — 태스크 목록

의존 순서 = 타입/기구 → 툴 → 콘텐츠 → 배선 → 통합 → 검증. 각 task는 독립 커밋 단위.

| # | 태스크 | 상태 |
| --- | --- | --- |
| 1 | before 베이스라인 + 서브셋 스파이크 — 현재 WebGL `.data` 폰트 크기 기록 + 1개 locale 서브셋 수동 베이크로 실제 절감치(~22M) 확증 | #109 closed |
| 2 | `FontTier` 타입 + 프로바이더 티어 파라미터 — `FontTier{Boot,Content}`, `IFontProvider.LoadAsync`에 tier 추가, `AddressablesFontProvider` 엔트리키 선택자(`"font"`/`"font-boot"`), `DirectRefFontProvider`·`FontEngine.BootAsync(FontTier)` 반영 + 깨지는 테스트 동반 갱신 | #110 closed |
| 3 | 서브셋 베이크 툴(Editor) — locale String Table + 피커 4라벨 + 메뉴 문자열 + ASCII 글리프 추출 → per-locale Static `TMP_FontAsset` 생성(`m_AtlasPopulationMode:0`, Custom Characters, 소스 TTF 미포함) | #111 closed |
| 4 | boot-tier 서브셋 에셋 4종 베이크 + FontRef/boot-tier FontCatalog 생성 — task 3 툴로 en/ko/ja/zh Static 서브셋 아틀라스 + boot `FontRef` 4종 + boot-tier `FontCatalog` SO | #112 closed |
| 5 | Asset Table `"Fonts"` `"font-boot"` 엔트리 배선 — 새 shared 엔트리키 `"font-boot"` + locale별 boot `FontRef` 매핑 | #113 closed |
| 6 | FontService boot 티어 적용 — `BootAsync`가 `FontTier.Boot`로 부팅, `ApplyPickerFallbacks`가 boot-tier 카탈로그 참조 | #114 closed |
| 7 | after diff + 검증 — after WebGL `.data` diff(목표 ~22M 절감), en/ko/ja/zh tofu 없이 렌더, 불변식(TTF 미포함·컴포넌트 폰트 미직렬화) + EditMode green | #115 closed |
