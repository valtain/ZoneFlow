using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZoneFlow
{
    /// <summary>게임플레이 모드 스택과 존 내비게이션을 총괄하는 서비스.</summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class GamePlayDirector : MonoService<GamePlayDirector>
    {
        /// <summary>ZoneId로 ZoneAsset을 조회하는 레지스트리.</summary>
        [field: SerializeField] public ZoneAssetRegistry ZoneAssets { get; private set; } = default;

        /// <summary>SpawnPointId로 소속 ZoneId를 역조회하는 레지스트리.</summary>
        [field: SerializeField] public SpawnPointRegistry SpawnPoints { get; private set; } = default;

        /// <summary>IInteractable 조회 레지스트리. Zone 씬 로드 여부와 무관하게 InteractableId로 검색한다.</summary>
        [field: SerializeField] public InteractableRegistry Interactables { get; private set; } = default;

        /// <summary>프리팹 기반 Zone 인스턴스를 생성할 부모 Transform. null이면 월드 루트에 생성된다.</summary>
        [field: SerializeField] public Transform ZonePrefabRoot { get; private set; } = default;

        /// <summary>현재 활성 모드. 스택이 비어 있으면 null.</summary>
        public GamePlayMode ActiveMode => _stack.Count > 0 ? _stack[^1] : null;

        /// <summary>현재 모드 스택 전체 (읽기 전용).</summary>
        public IReadOnlyList<GamePlayMode> ModeStack => _stack;

        private readonly List<GamePlayMode> _stack = new();

        /// <summary>Zone 인스턴스 생명주기를 관리하는 레지스트리.</summary>
        internal ZoneRegistry ZoneRegistry { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            ZoneRegistry = new ZoneRegistry(ZonePrefabRoot);
        }

        /// <summary>
        /// Bootstrap 씬에서 진입할 때 호출한다. CoreServices를 로드하고 Bootstrap 씬을 언로드한 뒤 내비게이션을 실행한다.
        /// ColdStartup / Bootstrap / DevBootstrap 에서만 호출한다.
        /// </summary>
        public static async UniTask BootstrapAsync(string sceneToUnload, NavigationConfig navigation)
        {
            await SceneService.EnsureCoreServicesLoaded();
            await SceneManager.UnloadSceneAsync(sceneToUnload).ToUniTask();
            await Instance.NavigateAsync(navigation.BuildUri(), CancellationToken.None);
        }

        /// <summary>URI 문자열로 내비게이션 요청을 처리한다.</summary>
        public UniTask NavigateAsync(string uri, CancellationToken ct)
        {
            var request = NavigationRequest.Parse(uri);
            return NavigateAsync(request, ct);
        }

        /// <summary>NavigationRequest로 내비게이션 요청을 처리한다.</summary>
        public async UniTask NavigateAsync(NavigationRequest request, CancellationToken ct)
        {
            if (request.Host == NavigationHost.Portal)
            {
                // InteractableRegistry 우선 조회 — Zone 씬 로드 여부와 무관
                if (Interactables != null && Interactables.TryGetNavigationUri(request.Id, out var registeredUri))
                {
                    await NavigateAsync(registeredUri, ct);
                    return;
                }

                // Fallback: 현재 로드된 씬에서 Portal 검색 (Registry 미설정 시)
                var portals = FindObjectsByType<Portal>(FindObjectsSortMode.None);
                foreach (var portal in portals)
                {
                    if (portal.PortalId == request.Id)
                    {
                        await NavigateAsync(portal.NavigationUri, ct);
                        return;
                    }
                }
                Debug.Assert(false, $"[GamePlayDirector] PortalId '{request.Id}'에 해당하는 Portal을 찾지 못했습니다.");
                return;
            }

            if (request.Host == NavigationHost.Pop)
            {
                await PopAsync(ct);
                return;
            }

            switch (request.Switch)
            {
                case ModeSwitch.Stack:
                    await StackAsync(CreateMode(request), ct);
                    break;
                case ModeSwitch.ReplaceAll:
                    await ReplaceAllAsync(CreateMode(request), ct);
                    break;
                default:
                    await ReplaceAsync(CreateMode(request), ct);
                    break;
            }
        }

        private async UniTask ReplaceAsync(GamePlayMode next, CancellationToken ct)
        {
            if (ActiveMode != null)
            {
                var current = ActiveMode;
                _stack.RemoveAt(_stack.Count - 1);
                await current.StopAndDestroyAsync(ct);
            }

            _stack.Add(next);
            await next.PlayAsync(this, ct);
        }

        private async UniTask StackAsync(GamePlayMode next, CancellationToken ct)
        {
            if (ActiveMode != null)
                await ActiveMode.SleepAsync(ct);

            _stack.Add(next);
            await next.PlayAsync(this, ct);
        }

        private async UniTask ReplaceAllAsync(GamePlayMode next, CancellationToken ct)
        {
            // ① Active 모드 ModeOut 포함 정리
            if (ActiveMode != null)
            {
                var current = ActiveMode;
                _stack.RemoveAt(_stack.Count - 1);
                await current.StopAndDestroyAsync(ct);
            }

            // ② Slept 모드들 ModeOut 없이 정리 (이미 sleep 시 ModeOut 완료)
            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                await _stack[i].DestroySleptAsync(ct);
            }
            _stack.Clear();

            _stack.Add(next);
            await next.PlayAsync(this, ct);
        }

        private async UniTask PopAsync(CancellationToken ct)
        {
            Debug.Assert(_stack.Count > 0, "[GamePlayDirector] PopAsync: 모드 스택이 비어 있습니다.");
            if (_stack.Count == 0) return;

            var current = ActiveMode;
            _stack.RemoveAt(_stack.Count - 1);
            await current.StopAndDestroyAsync(ct);

            if (ActiveMode != null)
                await ActiveMode.ResumeAsync(ct);
        }

        private GamePlayMode CreateMode(NavigationRequest request)
        {
            ZoneAsset zoneAsset = null;

            if (!string.IsNullOrEmpty(request.ZoneId))
            {
                ZoneAssets.TryGetZone(request.ZoneId, out zoneAsset);
            }
            else if (!string.IsNullOrEmpty(request.Id))
            {
                if (SpawnPoints.TryGetZoneId(request.Id, out var zoneId))
                    ZoneAssets.TryGetZone(zoneId, out zoneAsset);
            }

            switch (request.Host)
            {
                case NavigationHost.Exploration:
                    return new ExplorationMode(zoneAsset, request.Id);
                case NavigationHost.Battle:
                    return new BattleMode(zoneAsset, request.Id);
                case NavigationHost.Story:
                    return new StoryMode(zoneAsset, request.Id);
                case NavigationHost.Shell:
                    return new ShellMode(request.Id, zoneAsset, spawnPointId: null);
                default:
                    Debug.Assert(false, $"[GamePlayDirector] 알 수 없는 Scheme: {request.Host}");
                    return new ExplorationMode(zoneAsset, request.Id);
            }
        }
    }
}
