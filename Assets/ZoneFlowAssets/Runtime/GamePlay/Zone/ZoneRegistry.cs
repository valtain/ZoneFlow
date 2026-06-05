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
            public bool IsPrefabBased;
        }

        private readonly Dictionary<string, ZoneHandle> _handles = new();
        private readonly Transform _prefabRoot;

        /// <summary>프리팹 기반 Zone을 인스턴스화할 부모 Transform을 지정하여 생성한다. null이면 월드 루트에 생성된다.</summary>
        internal ZoneRegistry(Transform prefabRoot = null)
        {
            _prefabRoot = prefabRoot;
        }

        /// <summary>ZoneAsset을 로드하거나 이미 로드된 Zone의 참조 카운트를 증가시켜 반환한다.</summary>
        public async UniTask<Zone> AcquireAsync(ZoneAsset asset, CancellationToken ct)
        {
            if (_handles.TryGetValue(asset.ZoneId, out var handle))
            {
                handle.RefCount++;
                _handles[asset.ZoneId] = handle;
                return handle.Zone;
            }

            Zone zone;
            bool isPrefabBased;

            if (asset.IsSceneBased)
            {
                await SceneService.Instance.LoadSceneAdditiveAsync(asset.SceneName, ct);
                zone = FindZoneInScene(asset.SceneName);
                Debug.Assert(zone != null, $"[ZoneRegistry] 씬 '{asset.SceneName}'에서 Zone 컴포넌트를 찾지 못했습니다.");
                isPrefabBased = false;
            }
            else
            {
                Debug.Assert(asset.ZonePrefab != null, $"[ZoneRegistry] ZoneAsset '{asset.ZoneId}'의 ZonePrefab이 null입니다.");
                var go = Object.Instantiate(asset.ZonePrefab, _prefabRoot);
                zone = go.GetComponentInChildren<Zone>(true);
                Debug.Assert(zone != null, $"[ZoneRegistry] 프리팹 '{asset.ZoneId}'에서 Zone 컴포넌트를 찾지 못했습니다.");
                isPrefabBased = true;
            }

            _handles[asset.ZoneId] = new ZoneHandle { Zone = zone, RefCount = 1, IsPrefabBased = isPrefabBased };
            return zone;
        }

        /// <summary>Zone 참조를 해제한다. 참조 카운트가 0이 되면 씬 또는 오브젝트를 언로드한다.</summary>
        public async UniTask ReleaseAsync(string zoneId)
        {
            if (!_handles.TryGetValue(zoneId, out var handle))
                return;

            handle.RefCount--;
            if (handle.RefCount > 0)
            {
                _handles[zoneId] = handle;
                return;
            }

            _handles.Remove(zoneId);

            if (handle.IsPrefabBased)
            {
                if (handle.Zone != null)
                    Object.Destroy(handle.Zone.gameObject);
            }
            else
            {
                var sceneName = handle.Zone != null ? handle.Zone.gameObject.scene.name : zoneId;
                await SceneService.Instance.UnloadSceneAsync(sceneName, CancellationToken.None);
            }
        }

        /// <summary>ZoneId에 해당하는 Zone이 현재 로드되어 있는지 반환한다.</summary>
        public bool IsLoaded(string zoneId) => _handles.ContainsKey(zoneId);

        private static Zone FindZoneInScene(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            foreach (var root in scene.GetRootGameObjects())
            {
                var z = root.GetComponentInChildren<Zone>(true);
                if (z != null) return z;
            }
            return null;
        }
    }
}
