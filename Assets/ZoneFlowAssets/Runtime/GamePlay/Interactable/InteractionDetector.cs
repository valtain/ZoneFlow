using System;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>
    /// 플레이어 주변을 주기적으로 스캔해 사정권 내 최근접 IInteractable을 추적한다.
    /// 최근접 대상이 바뀌면 NearestChanged 이벤트로 통지한다. 플레이어 GameObject에 부착해 사용한다.
    /// </summary>
    public sealed class InteractionDetector : MonoBehaviour
    {
        [SerializeField] private float _detectionRadius = 4f;
        [SerializeField] private LayerMask _interactableMask = ~0;
        [SerializeField] private float _scanInterval = 0.1f;

        /// <summary>현재 사정권 내 최근접 IInteractable. 없으면 null.</summary>
        public IInteractable Current { get; private set; }

        /// <summary>최근접 대상이 바뀔 때 발생한다. 사정권이 비면 null이 전달된다.</summary>
        public event Action<IInteractable> NearestChanged;

        private readonly Collider[] _hits = new Collider[16];
        private float _nextScanTime;

        private void Update()
        {
            if (Time.time < _nextScanTime) return;
            _nextScanTime = Time.time + _scanInterval;

            var nearest = FindNearest();
            if (ReferenceEquals(nearest, Current)) return;

            Current = nearest;
            NearestChanged?.Invoke(nearest);
        }

        private IInteractable FindNearest()
        {
            int count = Physics.OverlapSphereNonAlloc(
                transform.position, _detectionRadius, _hits,
                _interactableMask, QueryTriggerInteraction.Collide);

            IInteractable best = null;
            float bestSqr = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                if (!_hits[i].TryGetComponent<IInteractable>(out var interactable)) continue;
                float sqr = (_hits[i].transform.position - transform.position).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = interactable;
                }
            }
            return best;
        }
    }
}
