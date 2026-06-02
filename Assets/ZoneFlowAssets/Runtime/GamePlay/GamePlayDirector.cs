using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

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

        /// <summary>URI 문자열로 내비게이션 요청을 처리한다.</summary>
        public UniTask NavigateAsync(string uri, CancellationToken ct)
        {
            var request = NavigationRequest.Parse(uri);
            return NavigateAsync(request, ct);
        }

        /// <summary>NavigationRequest로 내비게이션 요청을 처리한다.</summary>
        public async UniTask NavigateAsync(NavigationRequest request, CancellationToken ct)
        {
            if (request.Scheme == NavigationHost.Portal)
            {
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

            if (request.Scheme == NavigationHost.Pop)
            {
                await PopAsync(ct);
                return;
            }

            switch (request.Switch)
            {
                case ModeSwitch.Stack:
                    await StackAsync(CreateMode(request), ct);
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

            switch (request.Scheme)
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
                    Debug.Assert(false, $"[GamePlayDirector] 알 수 없는 Scheme: {request.Scheme}");
                    return new ExplorationMode(zoneAsset, request.Id);
            }
        }
    }
}
