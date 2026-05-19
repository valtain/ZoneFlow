# service_locator — 설계 결정

| 결정 | 선택 | 이유 |
| --- | --- | --- |
| Service 접근 패턴 | `MonoService<T>.Instance` | 씬 배치 원칙 부합, 레퍼런스 검증된 패턴 |
| 자동 GameObject 생성 | 사용 안 함 | CLAUDE.md 원칙 — 씬 배치 책임 |
| 중복 인스턴스 처리 | `Debug.Assert` | throw 대신 Assert (CLAUDE.md 원칙) |
| ExecutionOrder | -1000 | Bootstrap(-2000)보다 늦게, 일반 컴포넌트보다 먼저 |
