using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>모든 UI 패널의 추상 기반 클래스. 표시·숨김 생명주기 훅을 제공한다.</summary>
    public abstract class UiPanel : MonoBehaviour
    {
        /// <summary>레이어가 패널을 표시할 때 호출한다.</summary>
        public UniTask ShowAsync(CancellationToken ct) => OnShowAsync(ct);

        /// <summary>레이어가 패널을 숨길 때 호출한다.</summary>
        public UniTask HideAsync(CancellationToken ct) => OnHideAsync(ct);

        /// <summary>표시 시 동작 구현 훅.</summary>
        protected virtual UniTask OnShowAsync(CancellationToken ct) => UniTask.CompletedTask;

        /// <summary>숨김 시 동작 구현 훅.</summary>
        protected virtual UniTask OnHideAsync(CancellationToken ct) => UniTask.CompletedTask;
    }
}
