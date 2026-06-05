using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZoneFlow.Player;

namespace ZoneFlow
{
    /// <summary>ExplorationMode 인게임 HUD. 체력바와 존 정보를 표시한다.</summary>
    public sealed class HudPanel : UiPanel
    {
        public const string PanelId = "exploration-hud";
        [SerializeField] private RectTransform _healthBarContainer;
        [SerializeField] private Image _healthFill;
        [SerializeField] private RectTransform _zoneInfoContainer;
        [SerializeField] private TextMeshProUGUI _zoneNameLabel;

        private const float SlideDuration = 0.35f;
        private const float StaggerDelay = 0.08f;
        private const float SlideOffset = 400f;

        private Vector2 _healthBarRestPos;
        private Vector2 _zoneInfoRestPos;
        private PlayerStats _boundStats;

        private void Awake()
        {
            _healthBarRestPos = _healthBarContainer.anchoredPosition;
            _zoneInfoRestPos = _zoneInfoContainer.anchoredPosition;
        }

        /// <summary>Zone 정보를 초기화한다. OnPlayedAsync에서 생성 직후 호출한다.</summary>
        public void Initialize(ZoneAsset zone)
        {
            if (_zoneNameLabel != null)
                _zoneNameLabel.text = zone?.ZoneId ?? string.Empty;
        }

        protected override async UniTask OnShowAsync(CancellationToken ct)
        {
            _boundStats = PlayerService.Instance.Stats;
            if (_boundStats != null)
            {
                _boundStats.OnChanged += RefreshHealthBar;
                RefreshHealthBar(_boundStats);
            }

            _healthBarContainer.anchoredPosition = _healthBarRestPos + new Vector2(-SlideOffset, 0f);
            _zoneInfoContainer.anchoredPosition = _zoneInfoRestPos + new Vector2(SlideOffset, 0f);

            Tween.UIAnchoredPosition(_healthBarContainer, _healthBarRestPos, SlideDuration, Ease.OutBack);
            await UniTask.Delay((int)(StaggerDelay * 1000), cancellationToken: ct);
            await Tween.UIAnchoredPosition(_zoneInfoContainer, _zoneInfoRestPos, SlideDuration, Ease.OutBack)
                .ToUniTask(cancellationToken: ct);
        }

        protected override async UniTask OnHideAsync(CancellationToken ct)
        {
            if (_boundStats != null)
            {
                _boundStats.OnChanged -= RefreshHealthBar;
                _boundStats = null;
            }

            Tween.UIAnchoredPosition(_healthBarContainer, _healthBarRestPos + new Vector2(-SlideOffset, 0f),
                SlideDuration, Ease.InBack);
            await Tween.UIAnchoredPosition(_zoneInfoContainer, _zoneInfoRestPos + new Vector2(SlideOffset, 0f),
                SlideDuration, Ease.InBack)
                .ToUniTask(cancellationToken: ct);
        }

        private void RefreshHealthBar(PlayerStats stats)
        {
            if (_healthFill != null)
                _healthFill.fillAmount = stats.HpRatio;
        }
    }
}
