# polyglot — 검증 시나리오

> `/feature plan polyglot` 및 각 task 구현 시 확정한다. 아래는 spec 기준 초안 골격.

## TC-01: 패키지 경계·컴파일
- [ ] `Polyglot` asmdef(+ TMP·Localization 참조) 컴파일 에러 0.
- [ ] `Polyglot` 어셈블리가 `Assembly-CSharp`를 참조하지 않는다(순수 엔진 유지).
- [ ] 게임 측 `FontService` 어댑터가 Polyglot API를 참조·호출한다.

## TC-02: 부팅 locale 폰트 적용
- [ ] 지정 locale로 부팅 시 해당 CJK 패밀리가 TMP 기본 폰트로 적용된다.
- [ ] 미선택 상태면 기본 locale로 적용된다.
- [ ] 부팅 후 런타임 swap이 발생하지 않는다.

## TC-03: 자형 혼입 없음
- [ ] 중국어 locale에서 일본어/한국어 지역 자형이 혼입되지 않는다.
- [ ] 잔여 글리프만 전역 fallback으로 보강된다.

## TC-04: 폰트 오염 불변식
- [ ] 컴포넌트/프리팹에 폰트 asset이 serialize되지 않는다(Style만 지정).
- [ ] 저장 가드가 폰트 필드를 자동 스트립하고, 동적 폰트를 `ClearFontAssetData()`한다.

## TC-05: 패키지 변경 격리(facade)
- [ ] `TMPro.*`·`UnityEngine.Localization.*` 직접 호출이 패키지 내부 facade 1곳에만 존재한다.
- [ ] 호출부·게임 어댑터는 TMP/Localization 타입을 직접 참조하지 않는다.

## TC-06: 에디터 프리뷰
- [ ] Game View locale 스위처로 프리뷰 시 언어별 폰트/스타일이 반영된다.
