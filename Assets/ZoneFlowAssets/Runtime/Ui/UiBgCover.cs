using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>팝업 배경 및 화면 전환 효과의 배경을 담당하는 컴포넌트. PrimeTween 기반 FadeIn/Out 지원.</summary>
    public sealed class UiBgCover : MonoBehaviour
    {
        [field: SerializeField] public CanvasGroup CanvasGroup { get; private set; } = default;
        private Tween _fadeTween;

        /// <summary>지정된 duration 동안 targetAlpha로 페이드 인한다.</summary>
        public async UniTask FadeInAsync(float duration, float targetAlpha, CancellationToken ct)
        {
            _fadeTween.Stop();
            gameObject.SetActive(true);
            CanvasGroup.blocksRaycasts = true;
            _fadeTween = Tween.Alpha(CanvasGroup, startValue: CanvasGroup.alpha, endValue: targetAlpha, duration: duration);
            using var reg = ct.Register(() => _fadeTween.Stop());
            await _fadeTween;
            ct.ThrowIfCancellationRequested();
        }

        /// <summary>지정된 duration 동안 alpha를 0으로 페이드 아웃한다.</summary>
        public async UniTask FadeOutAsync(float duration, CancellationToken ct)
        {
            _fadeTween.Stop();
            _fadeTween = Tween.Alpha(CanvasGroup, startValue: CanvasGroup.alpha, endValue: 0f, duration: duration);
            using var reg = ct.Register(() => _fadeTween.Stop());
            await _fadeTween;
            ct.ThrowIfCancellationRequested();
            CanvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        private void OnDestroy() => _fadeTween.Stop();
    }
}
