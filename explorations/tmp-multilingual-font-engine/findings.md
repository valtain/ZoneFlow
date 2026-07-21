# 탐색 결과

**결론**: locale이 부팅 시 1회 결정된다는 점을 이용해, 부팅 때 TMP Settings(locale별 기본 폰트 + 전역 fallback + locale 스타일시트)를 한 번 세팅하는 `FontService`를 세운다. 저작 측은 "폰트 지정 금지, Style만 선택" + 저장 가드로 폰트 직렬화·동적 글리프 오염을 원천 차단한다. Addressables·런타임 문자열 계층은 seam/후속으로 분리한다.

**채택된 방향**: Candidate A — 부팅 1회 TMP Settings 세팅 + Style-only 저작 + `IFontProvider` seam
- `FontService : MonoService<FontService>`(CoreServices): 부팅 1회 **영구 저장된 locale 선택값**을 읽어(미선택 시 기본 locale) → 폰트 로드(seam) → TMP Settings 적용(locale별 기본 폰트/전역 fallback/스타일시트). 런타임 swap 없음.
- **선택 지점·생애주기**: 폰트/locale 선택은 **Intro 씬 피커에서만** 1회, 확정 후 불변(타 지점 선택 옵션 없음, Intro 재방문에도 불변). 저장은 세이브 슬롯 이전 기기/프로파일 사전 설정 → **PlayerPrefs/설정 스토어**(ISaveable 아님) 권장, 후속 feature systems-designer 확정.
- 커버리지: 컴포넌트는 폰트 미직렬화 → locale 기본 폰트(라틴+CJK 한 패밀리) 상속, 잔여 글리프만 전역 fallback 보강.
- 언어별 `TMP_StyleSheet`로 (스타일 × 언어) 폰트 지정.
- 저작 제어: Font Asset 필드 잠금·Style만 노출 + 저장 가드가 폰트 자동 스트립(기존 18개 프리팹 1회 diff).
- 오염 방지: OnWillSaveAssets `ClearFontAssetData()` + Clear Dynamic Data on Build.
- 에디터 프리뷰: Unity Localization `LocalizedTmpFont` + Game View locale 스위처(프리뷰용 도입).
- 로딩 seam: `DirectRefFontProvider` 지금 → `AddressablesFontProvider` 나중(호출부 무변경).

**폐기된 방향**:
- Candidate B (단일 거대 fallback 체인) — 이유: TMP 첫 매칭 정지로 한자 지역 자형 혼입, 스타일·언어 지정 불가.
- Candidate C (Localization Property Variants를 폰트 배선 주 메커니즘으로) — 이유: 폰트 대량 직렬화로 "기본 폰트 외 serialize 금지" 불변식과 충돌. 단 Localization은 에디터 프리뷰 도구로는 채택.

**후속 Feature 후보**: `tmp-multilingual-font-engine` — `FontService`+`IFontProvider`(`DirectRefFontProvider`)+`FontRef`/`FontCatalog` SO 스켈레톤, 언어별 `TMP_StyleSheet` 규약+부팅 훅, **Intro 씬 폰트/locale 피커(1회·불변)+영구 저장(PlayerPrefs/설정 스토어)**, 저작 제어(폰트 필드 잠금·Style-only·저장 스트립/검증), 오염 방지 가드, `com.unity.localization` 프리뷰 도입, Noto Sans CJK 동적 SDF 자산(파일 확보 시). → `/feature new tmp-multilingual-font-engine --from tmp-multilingual-font-engine`

**CLAUDE.md 반영 필요**:
- 후속 feature 완료 시 "인게임 텍스트 영문 강제" 회피 rule(`.claude/rules/ui-design.md`, `.claude/rules/level-content.md`) 완화·갱신.
- "폰트는 Style로만 지정, 컴포넌트에 폰트 직렬화 금지" 저작 규칙을 `.claude/rules/ui-design.md`·`.claude/rules/editor-code.md`에 추가.
- (이번 탐색에서는 코드·rule 변경 없음.)
