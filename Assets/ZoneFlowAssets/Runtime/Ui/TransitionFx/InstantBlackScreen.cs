using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>화면을 즉시 검은색으로 전환하는 화면 전환 효과. FadeIn duration=0으로 alpha=1 스냅.</summary>
    [Serializable]
    public sealed class InstantBlackScreen : IUiTransitionEffect
    {
        [SerializeField] private float _hideDuration = 0.3f;
        private UiBgCover _bgCover;

        /// <summary>배경 커버를 저장한다.</summary>
        public void Initialize(UiBgCover bgCover) => _bgCover = bgCover;

        /// <summary>배경을 즉시 불투명하게 한다 (duration=0, alpha=1).</summary>
        public UniTask ShowAsync(CancellationToken ct) => _bgCover.FadeInAsync(0f, 1f, ct);

        /// <summary>배경을 페이드 아웃한다 (alpha=0).</summary>
        public UniTask HideAsync() => _bgCover.FadeOutAsync(_hideDuration, default);
    }
}
