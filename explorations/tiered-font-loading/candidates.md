# 후보 방향들

> 결정 축: (1) 아틀라스 모드 Static/Dynamic, (2) Addressables Local/Remote, (3) 부팅↔콘텐츠 계층 분리 여부.
> 실측 전제: Dynamic 아틀라스라 소스 CJK TTF(~38M)가 빌드 필수 + Local 그룹이라 전량 초기 다운로드.

---

## Candidate C — 2단 폰트 로딩 (Boot static/local + Content dynamic/remote) — **추천 (사용자 제안)**

**상태**: active

- **Tier-1 Boot**: Intro·메뉴·피커 라벨 + ASCII의 **고정 글리프만** 담은 **Static 서브셋 아틀라스**, **Local**. CJK라도 수백 글리프라 <1MB. 부팅 즉시, 초기 다운로드에서 ~38MB TTF 제거. TTF 불요.
- **Tier-2 Content**: 기존 **Dynamic 전체 폰트**를 **Remote per-locale** 그룹으로. 콘텐츠 진입(`ContentServices` 로드) 시 **선택 locale만** HTTP 다운로드 + 재부팅 재적용.
- **장점**: 초기 다운로드 극소, 콘텐츠 폰트는 선택 locale만. 재부팅 seam(ADR-0007) 재사용.
- **단점**: Tier-2 **원격 호스팅 필요**(유일한 실제 비용), boot 서브셋 **재베이크 트리거**(Intro/메뉴 문자열 변경 시), 2단 전환 배선.
- **미결정→해소(2026-07-29)**: 메뉴=Tier-1 확정. Han-unification=**locale별 서브셋 4개 확정**(피커 4스크립트 동시 표시가 단일 공유 아틀라스를 배제). 티어 기구=**1a**(아래 seam 검증). 남은 미결정: Tier-2 아틀라스 모드(Dynamic=C vs Static=E), content 실패 시 floor 규약, 콘텐츠 지역화 실제 시점.

### Candidate C — 티어 분리 기구 (2026-07-29 seam 검증)

C를 코드에 얹는 방법 3안 평가 완료(discussion.md 참조):

- **1a — phase별 두 FontRef, 재부팅 재적용 → 추천.** `AddressablesFontProvider`의 `EntryKey`를 티어 선택자로(`"font-boot"` Local Static 서브셋 / `"font"` Remote Dynamic 전체), `FontEngine.BootAsync(FontTier)`로 티어를 흘림. 기존 FontRef·엔트리키·`SelectLocaleAsync` 재부팅 seam에 1:1 매핑, 프로바이더 stateless 유지, `GlobalFallback`의 "글리프 보강" 의미 보존. 비용: 콘텐츠 경계 default swap = TMP 전면 재적용(단 씬 전환과 겹쳐 숨음).
- **1b — 단일 프로바이더 async 업그레이드 → 기각.** `LoadAsync`의 단일 FontSet 계약을 깨고 재부팅 채널을 상태만 추가해 재발명.
- **1c — TMP fallback 체인, 재부팅 회피 → 비추천(유지).** boot 서브셋=default + content 전체=fallback. 재적용 churn은 없으나 `GlobalFallback`이 이미 "보강 + 피커 + per-locale"로 경합 → 티어까지 얹으면 채널 3중 오버로드.

**Tier-1 구조 확정**: 피커가 4 네이티브명 동시 표시 + Han-unification 자형 정확성 요구 → boot 티어는 **단일 공유 아틀라스 불가**, **per-locale Static 서브셋 4개**(병렬 boot-tier FontCatalog). 피커는 `ApplyPickerFallbacks` 패턴 재사용하되 서브셋 카탈로그를 가리킴.

**전환 트리거 소유권**: ContentServices 씬 부트스트랩=트리거(언제) / AddressableService=원격 다운로드(doc 주석이 이미 지정) / FontService=적용. content 티어 수명=ContentServices 씬 수명, 언로드 시 boot 서브셋 복원(대칭 생명주기, AQ-4 사례).

**새 불변식 필요**: content 티어 다운로드 실패(오프라인/CORS) 시 boot 서브셋 유지 + 오류 표면화 = "boot 티어가 기능적 바닥(floor)". 현재 `Debug.Assert(fontRef != null)`는 원격 실패 경로 미모델링.

---

## Candidate E — Static 서브셋 양 티어 (content 아틀라스 = String Table 베이크)

**상태**: active (신규 — Candidate A 기각 전제 재검토에서 도출)

- **전제 재검토**: A를 기각한 근거 "콘텐츠 문자열 무한"은 부분적으로 틀림. ZoneFlow 콘텐츠는 **저작된 내러티브/대사(String Table)** = 빌드 시점 유한 집합. 진짜 무한은 유저 입력(이름/채팅)뿐.
- **구성**: Tier-1 boot = per-locale Static 서브셋(C와 동일, Local). Tier-2 content = **String Table 스캔으로 베이크한 Static content 아틀라스**, Remote per-locale. → TTF 0 + 런타임 FreeType 0.
- **장점**: WebGL Dynamic FreeType 메인스레드 래스터화 비용 제거(C의 리스크 ③ 해소), TTF 완전 제거. 1a 재부팅 seam 그대로 사용(전체가 Static일 뿐).
- **단점**: **콘텐츠 증가마다 content 아틀라스 재베이크** — AQ-6(CatalogBaker `BakeAll` 전량 재스캔) 병목과 동일 패턴이 폰트로 확장. 유저 입력 텍스트(세이브명 등)는 못 덮음 → 해당 필드만 Dynamic fallback 필요(소규모).
- **C와의 관계**: C(Tier-2 Dynamic)와 E(Tier-2 Static)는 **Tier-2 아틀라스 모드만 다른 형제안**. Tier-1·전환 트리거·소유권 설계는 공유. 측정 프로토타입(atlas-mode 델타)이 둘 다의 Tier-1 전제를 먼저 검증하며, Tier-2 선택은 콘텐츠 유한성/재베이크 비용(AQ-6) 대 런타임 렌더 비용의 트레이드오프로 후속 결정.

---

## Candidate A — Static 서브셋 전면 (모든 폰트 고정 글리프)

**상태**: active (Tier-1의 근간이자, 콘텐츠가 고정이라면 단독 가능)

- 전 폰트를 String Table 글리프 서브셋 Static 아틀라스로. TTF 불요, 전 locale Local이어도 저렴.
- **장점**: Remote 호스팅 불요, 런타임 글리프 렌더 비용 0, 가장 단순한 배포.
- **단점**: **가변/유저 입력/미래 동적 텍스트 불가**(아틀라스 밖 글리프 미표시). 문자열 변경마다 재베이크.
- 사용자 판단: 콘텐츠 문자열은 무한이라 전면 A는 불가 → A는 **Tier-1에 국한**.

---

## Candidate B — Remote per-locale 전면 (전 폰트 Remote)

**상태**: active (Tier-2의 근간)

- 전 폰트 그룹을 Local→Remote. 선택 locale만 온디맨드.
- **장점**: 초기 다운로드 최소, 콘텐츠 가변 문자열도 전체 폰트로 커버.
- **단점**: **부팅조차 네트워크 의존**(피커 라벨 표시 전 폰트 다운로드 대기), 호스팅 필수.
- 사용자 판단: 부팅은 즉시여야 하므로 전면 B는 부적합 → B는 **Tier-2에 국한**.

---

## Candidate D — 별도 Intro 전용 폰트 (경량)

**상태**: eliminated

- Intro에 작은 별도 폰트를 써 큰 CJK 로드를 피한다.
- **폐기 이유**: Intro 태그라인이 **지역화된 CJK**라 해당 글리프가 필요 → 작은 Latin 폰트로는 렌더 불가 → 용량 절감 없음. (Intro를 Latin/브랜드 전용으로 바꾸면 가능하나 지역화 Intro 포기 + Candidate C의 Tier-1 서브셋이면 불필요.)

---

## Baseline — 현행 유지 (Dynamic · Local 전량)

**상태**: active (비교 기준)

- 변경 없음. 전 locale Dynamic 폰트 Local → 초기 ~38MB.
- **단점**: WebGL 초기 다운로드 과대. 개선의 측정 기준선.
