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
    public sealed class ExplorationHudPanel : UiPanel
    {
        public const string PanelId = "exploration-hud";
        [SerializeField] private RectTransform    _healthBarContainer;
        [SerializeField] private Image            _healthFill;
        [SerializeField] private RectTransform    _bannerContainer;
        [SerializeField] private TextMeshProUGUI  _modeLabel;
        [SerializeField] private TextMeshProUGUI  _zoneNameLabel;

        private const float SlideDuration = 0.35f;
        private const float StaggerDelay  = 0.08f;
        private const float SlideOffset   = 400f;

        private Vector2 _healthBarRestPos;
        private Vector2 _modeLabelRestPos;
        private Vector2 _zoneNameLabelRestPos;
        private PlayerStats _boundStats;

        private void Awake()
        {
            _healthBarRestPos     = _healthBarContainer.anchoredPosition;
            _modeLabelRestPos     = _modeLabel.rectTransform.anchoredPosition;
            _zoneNameLabelRestPos = _zoneNameLabel.rectTransform.anchoredPosition;
        }

        /// <summary>Zone 정보를 초기화한다. OnPlayedAsync에서 생성 직후 호출한다.</summary>
        public void Initialize(ZoneAsset zone)
        {
            if (_zoneNameLabel != null)
                _zoneNameLabel.text = zone != null
                    ? $"{zone.ZoneId}@{zone.SceneName}"
                    : string.Empty;
        }

        protected override async UniTask OnShowAsync(CancellationToken ct)
        {
            _boundStats = PlayerService.Instance.Stats;
            if (_boundStats != null)
            {
                _boundStats.OnChanged += RefreshHealthBar;
                RefreshHealthBar(_boundStats);
            }

            _healthBarContainer.anchoredPosition          = _healthBarRestPos     + new Vector2(-SlideOffset, 0f);
            _modeLabel.rectTransform.anchoredPosition     = _modeLabelRestPos     + new Vector2(-SlideOffset, 0f);
            _zoneNameLabel.rectTransform.anchoredPosition = _zoneNameLabelRestPos + new Vector2( SlideOffset, 0f);

            _ = Tween.UIAnchoredPosition(_healthBarContainer, _healthBarRestPos, SlideDuration, Ease.OutBack);

            await UniTask.Delay((int)(StaggerDelay * 1000), cancellationToken: ct);

            _ = Tween.UIAnchoredPosition(_modeLabel.rectTransform, _modeLabelRestPos, SlideDuration, Ease.OutBack);
            await Tween.UIAnchoredPosition(_zoneNameLabel.rectTransform, _zoneNameLabelRestPos,
                SlideDuration, Ease.OutBack).ToUniTask(cancellationToken: ct);
        }

        protected override async UniTask OnHideAsync(CancellationToken ct)
        {
            if (_boundStats != null)
            {
                _boundStats.OnChanged -= RefreshHealthBar;
                _boundStats = null;
            }

            _ = Tween.UIAnchoredPosition(_healthBarContainer,
                _healthBarRestPos + new Vector2(-SlideOffset, 0f), SlideDuration, Ease.InBack);
            _ = Tween.UIAnchoredPosition(_modeLabel.rectTransform,
                _modeLabelRestPos + new Vector2(-SlideOffset, 0f), SlideDuration, Ease.InBack);
            await Tween.UIAnchoredPosition(_zoneNameLabel.rectTransform,
                _zoneNameLabelRestPos + new Vector2(SlideOffset, 0f),
                SlideDuration, Ease.InBack).ToUniTask(cancellationToken: ct);
        }

        private void RefreshHealthBar(PlayerStats stats)
        {
            if (_healthFill != null)
                _healthFill.fillAmount = stats.HpRatio;
        }

#if UNITY_EDITOR
        [ContextMenu("Create Exploration HUD Elements")]
        private void CreateHudElements()
        {
            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0).gameObject);

            // ── BannerContainer (상단 full-width strip) ───────────────────
            var bannerGo = new GameObject("BannerContainer");
            bannerGo.transform.SetParent(transform, false);
            var bannerRect = bannerGo.AddComponent<RectTransform>();
            bannerRect.anchorMin        = new Vector2(0f, 1f);
            bannerRect.anchorMax        = new Vector2(1f, 1f);
            bannerRect.pivot            = new Vector2(0.5f, 1f);
            bannerRect.anchoredPosition = Vector2.zero;
            bannerRect.sizeDelta        = new Vector2(0f, 70f);

            // ── ModeLabel (배너 좌측) ─────────────────────────────────────
            var modeLabelGo = new GameObject("ModeLabel");
            modeLabelGo.transform.SetParent(bannerGo.transform, false);
            var modeLabelRect = modeLabelGo.AddComponent<RectTransform>();
            modeLabelRect.anchorMin = new Vector2(0f,   0f);
            modeLabelRect.anchorMax = new Vector2(0.5f, 1f);
            modeLabelRect.offsetMin = modeLabelRect.offsetMax = Vector2.zero;
            var modeTmp = modeLabelGo.AddComponent<TextMeshProUGUI>();
            modeTmp.text      = "◆ EXPLORATION";
            modeTmp.fontSize  = 28;
            modeTmp.alignment = TextAlignmentOptions.Left;
            modeTmp.color     = new Color(1f, 0.85f, 0.3f);

            // ── ZoneNameLabel (배너 우측) ─────────────────────────────────
            var zoneLabelGo = new GameObject("ZoneNameLabel");
            zoneLabelGo.transform.SetParent(bannerGo.transform, false);
            var zoneLabelRect = zoneLabelGo.AddComponent<RectTransform>();
            zoneLabelRect.anchorMin = new Vector2(0.5f, 0f);
            zoneLabelRect.anchorMax = new Vector2(1f,   1f);
            zoneLabelRect.offsetMin = zoneLabelRect.offsetMax = Vector2.zero;
            var zoneTmp = zoneLabelGo.AddComponent<TextMeshProUGUI>();
            zoneTmp.text      = "zone@Scene";
            zoneTmp.fontSize  = 24;
            zoneTmp.alignment = TextAlignmentOptions.Right;
            zoneTmp.color     = Color.white;

            // ── HealthBarContainer (좌하단) ───────────────────────────────
            var healthBarGo = new GameObject("HealthBarContainer");
            healthBarGo.transform.SetParent(transform, false);
            var healthBarRect = healthBarGo.AddComponent<RectTransform>();
            healthBarRect.anchorMin        = Vector2.zero;
            healthBarRect.anchorMax        = Vector2.zero;
            healthBarRect.pivot            = Vector2.zero;
            healthBarRect.anchoredPosition = new Vector2(40f, 40f);
            healthBarRect.sizeDelta        = new Vector2(300f, 40f);

            var healthBgGo = new GameObject("HealthBg");
            healthBgGo.transform.SetParent(healthBarGo.transform, false);
            var healthBgRect = healthBgGo.AddComponent<RectTransform>();
            healthBgRect.anchorMin = Vector2.zero;
            healthBgRect.anchorMax = Vector2.one;
            healthBgRect.sizeDelta = Vector2.zero;
            var healthBgImg = healthBgGo.AddComponent<Image>();
            healthBgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.75f);

            var healthFillGo = new GameObject("HealthFill");
            healthFillGo.transform.SetParent(healthBarGo.transform, false);
            var healthFillRect = healthFillGo.AddComponent<RectTransform>();
            healthFillRect.anchorMin = Vector2.zero;
            healthFillRect.anchorMax = Vector2.one;
            healthFillRect.sizeDelta = Vector2.zero;
            var healthFillImg = healthFillGo.AddComponent<Image>();
            healthFillImg.color      = new Color(0.15f, 0.75f, 0.2f);
            healthFillImg.type       = Image.Type.Filled;
            healthFillImg.fillMethod = Image.FillMethod.Horizontal;
            healthFillImg.fillAmount = 1f;

            // ── SerializedField 연결 ──────────────────────────────────────
            var so = new UnityEditor.SerializedObject(this);
            so.FindProperty("_healthBarContainer").objectReferenceValue = healthBarRect;
            so.FindProperty("_healthFill").objectReferenceValue         = healthFillImg;
            so.FindProperty("_bannerContainer").objectReferenceValue    = bannerRect;
            so.FindProperty("_modeLabel").objectReferenceValue          = modeTmp;
            so.FindProperty("_zoneNameLabel").objectReferenceValue      = zoneTmp;
            so.ApplyModifiedProperties();

            UnityEditor.EditorUtility.SetDirty(gameObject);
            Debug.Log("[ExplorationHudPanel] HUD 요소 생성 완료");
        }
#endif
    }
}
