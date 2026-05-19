# service_locator — 설계 스펙

## 목표

`MonoService<T>` 제네릭 베이스 클래스를 구현하여 씬 배치 기반의 정적 서비스 접근 패턴을 확립한다.

## 주요 컴포넌트

- `MonoService<T>` — 제네릭 추상 베이스 클래스 (DefaultExecutionOrder(-1000))
  - `static T Instance { get; private set; }`
  - `static bool IsReady`
  - `Awake()`: Instance 등록 + 중복 Assert
  - `OnDestroy()`: Instance 해제

## 데이터 흐름

씬 배치 MonoBehaviour(Awake) → Instance 등록 → 다른 서비스·컴포넌트에서 `T.Instance` 접근
