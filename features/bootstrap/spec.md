# bootstrap — 설계 스펙

## 목표

에디터 어느 씬에서 Play해도 정상 초기화 흐름이 보장되도록 ColdStartup과
개발용 DevBootstrap 씬을 구현한다.

## 주요 컴포넌트

- `ColdStartup` (MonoBehaviour, DefaultExecutionOrder(-2000)) — Zone 씬에 배치
  - `Awake()`: SceneService.IsReady 확인 → false면 Bootstrap 흐름 실행
  - 씬 내 다른 루트 GameObject 비활성화 후 Prerequisites 로드
  - `[field: SerializeField] SceneType SceneType` — Inspector 지정
- `GamePlayBootstrap` (MonoBehaviour, DefaultExecutionOrder(-2000)) — DevBootstrap 씬 전용
  - `Start()`: ColdStartup과 동일한 Prerequisites 로드 후 지정 Zone 이동
  - `[field: SerializeField] SceneSo StartZone` — Inspector 지정
- `DevBootstrap.unity` — 빌드 포함 금지, 개발 진입점

## 데이터 흐름

### ColdStartup (Zone 씬에서 직접 Play)
```text
Awake → SceneService.IsReady false
→ 다른 GO 비활성화
→ SceneService.BootstrapAsync(SceneType)
→ UnloadSceneAsync(current)
→ SceneService.LoadSceneAdditiveAsync(originalZone)
```

### GamePlayBootstrap (DevBootstrap 씬에서 Play)
```text
Start → SceneService.BootstrapAsync(Zone)
→ SceneService.LoadSceneAdditiveAsync(StartZone)
```
