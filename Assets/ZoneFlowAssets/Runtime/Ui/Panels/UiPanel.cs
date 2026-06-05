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

        /// <summary>PanelMode 진입 시 패널 자체 진입 연출을 수행한다.</summary>
        public UniTask ShowInAsync(CancellationToken ct) => OnShowInAsync(ct);

        /// <summary>PanelMode 퇴장 시 패널 자체 퇴장 연출을 수행한다.</summary>
        public UniTask ShowOutAsync(CancellationToken ct) => OnShowOutAsync(ct);

        /// <summary>표시 시 동작 구현 훅.</summary>
        protected virtual UniTask OnShowAsync(CancellationToken ct) => UniTask.CompletedTask;

        /// <summary>숨김 시 동작 구현 훅.</summary>
        protected virtual UniTask OnHideAsync(CancellationToken ct) => UniTask.CompletedTask;

        /// <summary>진입 연출 구현 훅.</summary>
        protected virtual UniTask OnShowInAsync(CancellationToken ct) => UniTask.CompletedTask;

        /// <summary>퇴장 연출 구현 훅.</summary>
        protected virtual UniTask OnShowOutAsync(CancellationToken ct) => UniTask.CompletedTask;
    }
}
