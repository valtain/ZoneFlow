using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZoneFlow
{
    /// <summary>상호작용 가능한 오브젝트의 친화 명칭과 행동 힌트를 표시하는 플로팅 패널.</summary>
    public sealed class InteractionPromptPanel : UiPanel
    {
        public const string PanelId = "interaction-prompt";
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _labelText;

        private const float FadeDuration = 0.2f;

        private void Awake()
        {
            _canvasGroup.alpha = 0f;
        }

        /// <summary>표시할 친화 명칭과 선택적 행동 힌트를 설정한다. DisplayLabel이 비면 fallbackId로 폴백한다.</summary>
        public void SetContent(string displayLabel, string fallbackId, string actionHint = null)
        {
            var text = string.IsNullOrWhiteSpace(displayLabel) ? fallbackId : displayLabel;
            _labelText.text = string.IsNullOrEmpty(actionHint) ? text : $"{actionHint}  {text}";
        }

        protected override async UniTask OnShowAsync(CancellationToken ct)
        {
            await Tween.Alpha(_canvasGroup, 1f, FadeDuration).ToUniTask(cancellationToken: ct);
        }

        protected override async UniTask OnHideAsync(CancellationToken ct)
        {
            await Tween.Alpha(_canvasGroup, 0f, FadeDuration).ToUniTask(cancellationToken: ct);
        }

#if UNITY_EDITOR
        [ContextMenu("Create Interaction Prompt Elements")]
        private void CreatePromptElements()
        {
            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0).gameObject);

            // ── CanvasGroup 확보 ──────────────────────────────────────────
            var cg = GetComponent<CanvasGroup>();
            if (cg == null)
                cg = gameObject.AddComponent<CanvasGroup>();

            // ── PromptContainer (하단 중심) ───────────────────────────────
            var promptContainerGo = new GameObject("PromptContainer");
            promptContainerGo.transform.SetParent(transform, false);
            var promptContainerRect = promptContainerGo.AddComponent<RectTransform>();
            promptContainerRect.anchorMin        = new Vector2(0.5f, 0f);
            promptContainerRect.anchorMax        = new Vector2(0.5f, 0f);
            promptContainerRect.pivot            = new Vector2(0.5f, 0f);
            promptContainerRect.anchoredPosition = new Vector2(0f, 160f);
            promptContainerRect.sizeDelta        = new Vector2(520f, 64f);

            // ── PromptBg (배경 이미지) ─────────────────────────────────────
            var bgGo = new GameObject("PromptBg");
            bgGo.transform.SetParent(promptContainerGo.transform, false);
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.75f);

            // ── PromptLabel (텍스트) ──────────────────────────────────────
            var labelGo = new GameObject("PromptLabel");
            labelGo.transform.SetParent(promptContainerGo.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text      = "◆ Interact";
            labelTmp.fontSize  = 26;
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.color     = Color.white;

            // ── SerializedField 연결 ──────────────────────────────────────
            var so = new UnityEditor.SerializedObject(this);
            so.FindProperty("_canvasGroup").objectReferenceValue = cg;
            so.FindProperty("_labelText").objectReferenceValue   = labelTmp;
            so.ApplyModifiedProperties();

            UnityEditor.EditorUtility.SetDirty(gameObject);
            Debug.Log("[InteractionPromptPanel] 프롬프트 요소 생성 완료");
        }
#endif
    }
}
