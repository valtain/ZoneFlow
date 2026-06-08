using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZoneFlow
{
    /// <summary>ZoneAsset 기준으로 Zone 인스턴스의 참조 카운팅 및 생명주기를 관리한다.</summary>
    internal sealed class ZoneRegistry
    {
        private struct ZoneHandle
        {
            public Zone Zone;
            public int RefCount;
        }

        private readonly Dictionary<string, ZoneHandle> _handles = new();

        /// <summary>Zone.Awake()에서 호출한다. ZoneRegistry가 초기화된 뒤에만 유효하다.</summary>
        internal static System.Action<Zone> Register;

        internal ZoneRegistry()
        {
            Register = zone =>
            {
                if (!_handles.ContainsKey(zone.ZoneId))
                    _handles[zone.ZoneId] = new ZoneHandle { Zone = zone, RefCount = 0 };
            };
        }

        internal void Teardown() => Register = null;

        /// <summary>Zone을 획득한다. 씬이 미로드 상태면 로드하고 Zone을 활성화한 뒤 반환한다. 이미 획득 중이면 참조 카운트만 증가시킨다.</summary>
        public async UniTask<Zone> AcquireAsync(ZoneAsset asset, CancellationToken ct)
        {
            if (_handles.TryGetValue(asset.ZoneId, out var handle) && handle.RefCount > 0)
            {
                handle.RefCount++;
                _handles[asset.ZoneId] = handle;
                return handle.Zone;
            }

            // 씬 로드 → Zone.Awake 실행 → Register 호출 → _handles에 자동 등록
            if (!SceneManager.GetSceneByName(asset.SceneName).isLoaded)
                await SceneService.Instance.LoadSceneAdditiveAsync(asset.SceneName, ct);

            Debug.Assert(_handles.ContainsKey(asset.ZoneId),
                $"[ZoneRegistry] Zone '{asset.ZoneId}'이 씬 '{asset.SceneName}' 로드 후에도 등록되지 않았습니다.");

            handle = _handles[asset.ZoneId];
            handle.RefCount = 1;
            _handles[asset.ZoneId] = handle;
            handle.Zone.gameObject.SetActive(true);
            return handle.Zone;
        }

        /// <summary>Zone 참조를 해제하고 비활성화한다. 씬 내 모든 Zone의 참조 카운트가 0이 되면 씬을 언로드한다.</summary>
        public async UniTask ReleaseAsync(string zoneId)
        {
            if (!_handles.TryGetValue(zoneId, out var handle))
                return;

            handle.RefCount--;
            _handles[zoneId] = handle;

            if (handle.RefCount > 0)
                return;

            handle.Zone.gameObject.SetActive(false);

            var sceneName = handle.Zone.gameObject.scene.name;

            foreach (var h in _handles.Values)
            {
                if (h.Zone != null && h.Zone.gameObject.scene.name == sceneName && h.RefCount > 0)
                    return;
            }

            // 씬 내 모든 Zone이 해제됨 → _handles 정리 후 언로드
            var toRemove = new List<string>();
            foreach (var kv in _handles)
                if (kv.Value.Zone != null && kv.Value.Zone.gameObject.scene.name == sceneName)
                    toRemove.Add(kv.Key);
            foreach (var key in toRemove)
                _handles.Remove(key);

            await SceneService.Instance.UnloadSceneAsync(sceneName, CancellationToken.None);
        }

        /// <summary>ZoneId에 해당하는 Zone이 현재 획득(활성화) 상태인지 반환한다.</summary>
        public bool IsLoaded(string zoneId) => _handles.TryGetValue(zoneId, out var h) && h.RefCount > 0;
    }
}
