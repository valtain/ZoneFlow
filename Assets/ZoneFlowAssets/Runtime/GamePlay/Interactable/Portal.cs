using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>플레이어가 진입하면 NavigationUri로 씬을 전환하는 포털 컴포넌트.</summary>
    [RequireComponent(typeof(Collider))]
    public class Portal : MonoBehaviour, IInteractable
    {
        /// <summary>이동할 대상의 NavigationUri. ?switch= 파라미터 포함 가능.</summary>
        [field: SerializeField] public string NavigationUri { get; private set; } = default;

        /// <summary>gameplay://portal?id= 참조용 포털 고유 ID. Zone 자식에 배치해야 Zone 관리 대상이 된다.</summary>
        [field: SerializeField] public string PortalId { get; private set; } = default;

        /// <summary>IInteractable 인터페이스 구현. PortalId를 반환한다.</summary>
        public string InteractableId => PortalId;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            if (!GamePlayDirector.IsReady) return;

            OnInteractAsync(GamePlayDirector.Instance, GamePlayDirector.Instance.destroyCancellationToken).Forget();
        }

        /// <summary>포털의 NavigationUri로 이동을 요청한다.</summary>
        public UniTask OnInteractAsync(GamePlayDirector director, CancellationToken ct)
        {
            return director.NavigateAsync(NavigationUri, ct);
        }
    }
}
