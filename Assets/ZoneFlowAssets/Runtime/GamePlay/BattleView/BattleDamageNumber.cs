using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;

namespace ZoneFlow.BattleView
{
    /// <summary>
    /// 월드 공간에 떠오르는 플로팅 데미지 숫자. World Space Canvas + TMP + CanvasGroup으로 구성되며,
    /// 재생이 끝나면 스스로 파괴된다. LateUpdate에서 메인 카메라 회전을 따라 빌보드 처리한다.
    /// </summary>
    public sealed class BattleDamageNumber : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private CanvasGroup _canvasGroup;

        private const float RiseHeight = 1f;
        private const float PlayDuration = 0.6f;

        private void Awake()
        {
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
        }

        /// <summary>데미지 값을 표시하고 상승·페이드 후 자신을 파괴한다.</summary>
        /// <param name="amount">표시할 데미지 값.</param>
        /// <param name="ct">취소 토큰.</param>
        public async UniTask PlayAsync(int amount, CancellationToken ct)
        {
            Debug.Assert(_label != null, "[BattleDamageNumber] _label이 할당되지 않았다.");
            if (_label != null) _label.text = amount.ToString();

            try
            {
                var riseTarget = transform.position + Vector3.up * RiseHeight;
                var riseTask = Tween.Position(transform, riseTarget, PlayDuration, Ease.OutQuad)
                    .ToUniTask(cancellationToken: ct);
                var fadeTask = _canvasGroup != null
                    ? Tween.Alpha(_canvasGroup, 0f, PlayDuration).ToUniTask(cancellationToken: ct)
                    : UniTask.CompletedTask;

                await UniTask.WhenAll(riseTask, fadeTask);
            }
            finally
            {
                if (this != null) Destroy(gameObject);
            }
        }

        private void LateUpdate()
        {
            if (!CameraService.IsReady) return;
            var cam = CameraService.Instance.MainCamera;
            if (cam == null) return;

            transform.rotation = cam.transform.rotation;
        }
    }
}
