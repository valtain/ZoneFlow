using System.Threading;
using Cysharp.Threading.Tasks;

namespace ZoneFlow
{
    /// <summary>플레이어가 상호작용할 수 있는 오브젝트 인터페이스.</summary>
    public interface IInteractable
    {
        /// <summary>플레이어가 이 오브젝트와 상호작용할 때 호출된다.</summary>
        UniTask OnInteractAsync(GamePlayDirector director, CancellationToken ct);
    }
}
