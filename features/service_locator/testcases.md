# service_locator — 검증 시나리오

- [ ] CoreServices 씬 로드 후 SceneService.IsReady == true
- [ ] 동일 씬에 MonoService 두 개 배치 시 Assert 발생
- [ ] CoreServices 씬 언로드 후 SceneService.IsReady == false
- [ ] MonoService 상속 구체 클래스가 씬에 배치되면 Instance 자동 등록
