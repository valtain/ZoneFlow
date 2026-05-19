# scene_service — 검증 시나리오

- [ ] BootstrapAsync(Shell) → CoreServices 씬만 Additive 로드됨
- [ ] BootstrapAsync(Zone) → CoreServices + GamePlayServices 씬 모두 로드됨
- [ ] LoadSceneAdditiveAsync → 씬이 Additive 모드로 로드됨
- [ ] UnloadSceneAsync → 씬이 언로드되고 SceneService.IsReady는 유지
- [ ] SceneSo.name == Unity 빌드 씬 이름과 일치
