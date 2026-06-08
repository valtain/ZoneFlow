# 후보 방향들

## A: 전면 제거 **상태: promoted**

ZonePrefab 필드, ZonePrefabRoot 필드, 프리팹 분기 로직을 모두 제거한다.
ZoneAsset은 SceneName + ZoneId만 보유하는 단순한 구조가 된다.

**근거**:
- 현재 완전히 미사용 (dead code)
- Addressables 도입 시 ZoneRegistry 전체를 재설계해야 하므로 프리팹 경로를 지금 유지해도 미래 가치 없음
- YAGNI — 필요할 때 추가가 더 용이

**제거 대상**:
- `ZoneAsset.cs`: ZonePrefab 필드, IsSceneBased 프로퍼티
- `ZoneRegistry.cs`: 프리팹 분기 (IsPrefabBased, _prefabRoot, else 블록), Transform 생성자 파라미터
- `GamePlayDirector.cs`: ZonePrefabRoot 필드, ZoneRegistry(ZonePrefabRoot) → ZoneRegistry()
- `CatalogBaker.cs`: ZonePrefab null 초기화 로직

---

## B: 현상 유지 **상태: eliminated**

미사용이지만 forward-compatible 구조로 보존.

**폐기 이유**: Addressables 도입 시 어차피 재설계 필요. 지금 유지해봐야 혼란만 가중.
