using System.Threading;
using Cysharp.Threading.Tasks;

namespace ZoneFlow
{
    /// <summary>플레이어가 상호작용할 수 있는 오브젝트 인터페이스.</summary>
    public interface IInteractable
    {
        /// <summary>
        /// InteractableRegistry에서 조회하는 고유 ID. 빈 문자열이면 레지스트리에 등록되지 않는다.
        /// Zone GameObject의 자식에 배치해야 Zone 관리 대상이 된다.
        /// </summary>
        string InteractableId { get; }

        /// <summary>플레이어가 이 오브젝트와 상호작용할 때 호출된다.</summary>
        UniTask OnInteractAsync(GamePlayDirector director, CancellationToken ct);
    }
}
