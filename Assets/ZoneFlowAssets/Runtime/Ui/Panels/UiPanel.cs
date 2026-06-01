using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>모든 UI 패널의 추상 기반 클래스. 표시·숨김 생명주기 훅을 제공한다.</summary>
    public abstract class UiPanel : MonoBehaviour
    {
        /// <summary>패널이 표시될 때 호출되는 훅.</summary>
        protected virtual UniTask OnShowAsync(CancellationToken ct) => UniTask.CompletedTask;

        /// <summary>패널이 숨겨질 때 호출되는 훅.</summary>
        protected virtual UniTask OnHideAsync(CancellationToken ct) => UniTask.CompletedTask;
    }
}
