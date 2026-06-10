using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>StoryMode 인게임 HUD. 스토리 모드 진입을 상단 배너로 표시한다.</summary>
    public sealed class StoryHudPanel : UiPanel
    {
        public const string PanelId = "story-hud";
        [SerializeField] private RectTransform _bannerContainer;
        [SerializeField] private TextMeshProUGUI _modeLabel;
        [SerializeField] private TextMeshProUGUI _zoneNameLabel;

        private const float SlideDuration = 0.35f;
        private const float SlideOffset = 200f;

        private Vector2 _bannerRestPos;

        private void Awake()
        {
            _bannerRestPos = _bannerContainer.anchoredPosition;
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
            _bannerContainer.anchoredPosition = _bannerRestPos + new Vector2(0f, SlideOffset);
            await Tween.UIAnchoredPosition(_bannerContainer, _bannerRestPos, SlideDuration, Ease.OutBack)
                .ToUniTask(cancellationToken: ct);
        }

        protected override async UniTask OnHideAsync(CancellationToken ct)
        {
            await Tween.UIAnchoredPosition(_bannerContainer, _bannerRestPos + new Vector2(0f, SlideOffset),
                SlideDuration, Ease.InBack)
                .ToUniTask(cancellationToken: ct);
        }

#if UNITY_EDITOR
        [ContextMenu("Create Story HUD Elements")]
        private void CreateHudElements()
        {
            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0).gameObject);

            // ── BannerContainer (상단 full-width strip) ───────────────────
            var bannerGo = new GameObject("BannerContainer");
            bannerGo.transform.SetParent(transform, false);
            var bannerRect = bannerGo.AddComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0f, 1f);
            bannerRect.anchorMax = new Vector2(1f, 1f);
            bannerRect.pivot = new Vector2(0.5f, 1f);
            bannerRect.anchoredPosition = Vector2.zero;
            bannerRect.sizeDelta = new Vector2(0f, 70f);

            // ── ModeLabel (좌측) ──────────────────────────────────────────
            var modeLabelGo = new GameObject("ModeLabel");
            modeLabelGo.transform.SetParent(bannerGo.transform, false);
            var modeLabelRect = modeLabelGo.AddComponent<RectTransform>();
            modeLabelRect.anchorMin = new Vector2(0f, 0f);
            modeLabelRect.anchorMax = new Vector2(0.5f, 1f);
            modeLabelRect.offsetMin = modeLabelRect.offsetMax = Vector2.zero;
            var modeTmp = modeLabelGo.AddComponent<TextMeshProUGUI>();
            modeTmp.text = "◆ STORY";
            modeTmp.fontSize = 28;
            modeTmp.alignment = TextAlignmentOptions.Left;
            modeTmp.color = new Color(1f, 0.85f, 0.3f);

            // ── ZoneNameLabel (우측) ──────────────────────────────────────
            var zoneLabelGo = new GameObject("ZoneNameLabel");
            zoneLabelGo.transform.SetParent(bannerGo.transform, false);
            var zoneLabelRect = zoneLabelGo.AddComponent<RectTransform>();
            zoneLabelRect.anchorMin = new Vector2(0.5f, 0f);
            zoneLabelRect.anchorMax = new Vector2(1f, 1f);
            zoneLabelRect.offsetMin = zoneLabelRect.offsetMax = Vector2.zero;
            var zoneTmp = zoneLabelGo.AddComponent<TextMeshProUGUI>();
            zoneTmp.text = "zone@Scene";
            zoneTmp.fontSize = 24;
            zoneTmp.alignment = TextAlignmentOptions.Right;
            zoneTmp.color = Color.white;

            // ── SerializedField 연결 ──────────────────────────────────────
            var so = new UnityEditor.SerializedObject(this);
            so.FindProperty("_bannerContainer").objectReferenceValue = bannerRect;
            so.FindProperty("_modeLabel").objectReferenceValue = modeTmp;
            so.FindProperty("_zoneNameLabel").objectReferenceValue = zoneTmp;
            so.ApplyModifiedProperties();

            UnityEditor.EditorUtility.SetDirty(gameObject);
            Debug.Log("[StoryHudPanel] HUD 요소 생성 완료");
        }
#endif
    }
}
