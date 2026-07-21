# 후보 방향들

## Candidate A: 부팅 1회 TMP Settings 세팅 + Style-only 저작 + seam
**상태**: promoted

locale은 부팅 시 1회 결정되고 런타임 swap이 없다는 점을 이용해, 부팅 때 TMP Settings(기본 폰트/스타일시트/전역 fallback)를 활성 locale에 맞게 한 번 세팅한다. 저작 측은 "폰트 지정 금지, Style만 선택" 규칙 + 저장 가드로 폰트 직렬화를 원천 차단한다.

**구성:**
- `FontService : MonoService<FontService>` (CoreServices) — 부팅 1회 **영구 저장된 locale 선택값**(미선택 시 기본) → 폰트 로드(seam) → TMP Settings 적용(**locale별 기본 폰트** + 전역 fallback `[localeCjkFont, symbolFont]` + locale 스타일시트).
- **선택 지점·생애주기**: 폰트/locale 선택은 **Intro 씬 피커에서만** 1회, 확정 후 불변(타 지점 선택 없음, Intro 재방문에도 불변). 저장은 PlayerPrefs/설정 스토어(ISaveable 아님).
- **커버리지 계층**: 컴포넌트는 폰트를 serialize하지 않고 locale 기본 폰트(Noto Sans CJK KR/JP/SC/TC = 라틴+CJK 한 패밀리) 상속 → 일관 메트릭. 잔여 글리프만 전역 fallback 보강.
- **언어별 `TMP_StyleSheet`**: 같은 스타일 이름 세트를 언어마다 저작, 부팅 시 활성 locale 스타일시트 선택.
- **`IFontProvider` seam**: 지금 `DirectRefFontProvider`(직접 SO 참조/`LazyLoadReference`/Resources), 나중에 `AddressablesFontProvider`로 교체. 호출부 무변경.
- **저작 제어(오염 방지 ②)**: Inspector에서 Font Asset 필드 잠금·Style만 노출. 저장 가드가 컴포넌트 폰트를 **자동 스트립**(기본 상속) → 기존 18개 하드코딩 프리팹도 정리(1회 diff). 불변식: 어떤 폰트 asset도 serialize되지 않음.
- **오염 방지 ①**: `AssetModificationProcessor.OnWillSaveAssets` → 동적 폰트 `ClearFontAssetData()` + 폰트별 Clear Dynamic Data on Build.
- **에디터 프리뷰**: Unity Localization(지금 프리뷰용 도입) `LocalizedTmpFont` + Game View locale 스위처.

**장점:**
- 런타임 swap 인프라 불필요(locale 1회 결정).
- locale 기본 폰트 상속으로 라틴+CJK 일관 타이포, 한자 지역 자형 혼입 없음.
- 저작 제어 + 저장 가드로 폰트 오염(글리프 베이킹·폰트 직렬화) 원천 차단.
- Addressables는 seam으로 확장(선례 lazyload-catalog 답습), 지금 미도입.

**단점:**
- 저작 헬퍼·저장 가드·프리뷰 등 에디터 코드 비중 있음.
- 기존 프리팹 폰트 스트립으로 1회 diff 발생.
- Localization을 프리뷰용으로 지금 도입(런타임 문자열은 후속).

---

## Candidate B: 단일 거대 fallback 체인
**상태**: eliminated — 이유: TMP는 첫 매칭 글리프에서 정지 → 중국어 문장에 일본어 자형 등 **한자 지역 자형 혼입**. 스타일별·언어별 폰트 지정 불가 — 요구와 상충.

하나의 마스터 폰트에 KO/JP/CN을 순서대로 fallback으로 연결.

**장점:** 구성 단순, locale 결정 불필요.
**단점:** 지역 자형 혼입, 스타일·언어 지정 불가, 메트릭 통제 불가.

---

## Candidate C: Localization Property Variants로 폰트를 컴포넌트마다 인스펙터 배선(주 메커니즘)
**상태**: eliminated — 이유: 18개+ 프리팹마다 폰트를 serialize하게 되어 **"기본 폰트 외 serialize 금지" 불변식과 정면 충돌** + "전역 설정" 방침과 상충. 단 Localization은 **에디터 프리뷰 도구로는 채택**(Candidate A §프리뷰).

각 TMP 컴포넌트에 Localized Property Variant로 locale별 폰트를 인스펙터에서 배선.

**장점:** Localization 네이티브, 컴포넌트 단위 제어.
**단점:** 폰트 직렬화 대량 발생(오염), 저작 부담 큼, 프리팹 다수 파괴적 변경.
