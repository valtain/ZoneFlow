using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZoneFlow.Ui
{
    /// <summary>
    /// 월드 공간에서 카메라를 항상 향하는 빌보드 UI 라벨.
    /// World Space Canvas 위에 배치하며, LateUpdate에서 카메라 회전을 추적한다.
    /// CameraService가 미준비이거나 카메라가 null이면 안전하게 스킵한다.
    /// </summary>
    public sealed class BillboardLabel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Scale Clamping")]
        [SerializeField] private float _referenceDistance = 8f;
        [SerializeField] private float _minScale = 0.4f;
        [SerializeField] private float _maxScale = 1.6f;

        [Header("Fade")]
        [SerializeField] private float _fadeStartDistance = 30f;
        [SerializeField] private float _fadeEndDistance = 45f;

        private Transform _camTransform;
        private Vector3 _baseScale;

        private void Awake()
        {
            _baseScale = transform.localScale;
            if (_canvasGroup != null)
                _canvasGroup.alpha = 1f;
        }

        private void LateUpdate()
        {
            if (!CameraService.IsReady) return;
            var cam = CameraService.Instance.MainCamera;
            if (cam == null) return;

            _camTransform = cam.transform;

            // 빌보드: 캔버스가 카메라와 평행하도록 회전
            transform.rotation = _camTransform.rotation;

            // 거리 계산
            float dist = Vector3.Distance(transform.position, _camTransform.position);

            // 거리 보정 스케일 — 멀어질수록 크게 하되 min/max로 클램프
            float scaleFactor = Mathf.Clamp(dist / _referenceDistance, _minScale, _maxScale);
            transform.localScale = _baseScale * scaleFactor;

            // 원거리 페이드아웃
            if (_canvasGroup != null)
            {
                float alpha = 1f;
                if (dist > _fadeStartDistance)
                    alpha = 1f - Mathf.Clamp01((dist - _fadeStartDistance) / (_fadeEndDistance - _fadeStartDistance));
                _canvasGroup.alpha = alpha;
            }
        }

        /// <summary>라벨 텍스트를 설정한다.</summary>
        public void SetText(string text)
        {
            Debug.Assert(_label != null, "[BillboardLabel] TextMeshProUGUI _label이 연결되지 않았습니다.");
            if (_label == null) return;
            _label.text = text;
        }

        /// <summary>현재 표시 중인 텍스트를 반환한다.</summary>
        public string GetText() => _label != null ? _label.text : string.Empty;
    }
}
