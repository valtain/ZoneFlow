using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>화면을 검은색(또는 지정 색상)으로 페이드 인/아웃하는 화면 전환 효과.</summary>
    [Serializable]
    public sealed class FadeScreen : IUiTransitionEffect
    {
        [SerializeField] private float _duration = 0.3f;
        private UiBgCover _bgCover;

        /// <summary>배경 커버를 저장한다.</summary>
        public void Initialize(UiBgCover bgCover) => _bgCover = bgCover;

        /// <summary>배경을 페이드 인한다 (alpha = 1.0).</summary>
        public UniTask ShowAsync(CancellationToken ct) => _bgCover.FadeInAsync(_duration, 1f, ct);

        /// <summary>배경을 페이드 아웃한다 (alpha = 0).</summary>
        public UniTask HideAsync() => _bgCover.FadeOutAsync(_duration, default);
    }
}
