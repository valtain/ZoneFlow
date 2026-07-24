using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using Polyglot;
using UnityEngine;
using UnityEngine.UI;
using ZoneFlow.Player;

namespace ZoneFlow
{
    /// <summary>상호작용 가능한 오브젝트의 친화 명칭과 행동 힌트를 표시하는 플로팅 패널.</summary>
    public sealed class InteractionPromptPanel : UiPanel
    {
        public const string PanelId = "interaction-prompt";
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _labelText;

        private const float FadeDuration = 0.2f;

        private InteractionDetector _detector;
        private CancellationTokenSource _fadeCts;
        private bool _visible;

        private void Awake()
        {
            _canvasGroup.alpha = 0f;
        }

        /// <summary>
        /// 플레이어의 InteractionDetector를 찾아 구독하고, 현재 상태를 즉시 반영한다.
        /// 모드 진입(플레이어 스폰 이후)에 호출한다.
        /// </summary>
        public void Bind()
        {
            var player = PlayerService.Instance.Player;
            _detector = player != null ? player.GetComponent<InteractionDetector>() : null;
            Debug.Assert(_detector != null, "[InteractionPromptPanel] Player에 InteractionDetector가 없습니다.");
            if (_detector == null) return;

            _detector.NearestChanged += OnNearestChanged;
            OnNearestChanged(_detector.Current);
        }

        /// <summary>디텍터 구독을 해제하고 진행 중인 페이드를 취소한다. 모드 퇴장에 호출한다.</summary>
        public void Unbind()
        {
            if (_detector != null)
            {
                _detector.NearestChanged -= OnNearestChanged;
                _detector = null;
            }
            CancelFade();
        }

        private void OnNearestChanged(IInteractable nearest)
        {
            if (nearest != null)
            {
                SetContent(nearest.DisplayLabel, nearest.InteractableId);
                if (!_visible)
                {
                    _visible = true;
                    Fade(ShowAsync);
                }
            }
            else if (_visible)
            {
                _visible = false;
                Fade(HideAsync);
            }
        }

        private void Fade(System.Func<CancellationToken, UniTask> body)
        {
            CancelFade();
            _fadeCts = new CancellationTokenSource();
            body(_fadeCts.Token).Forget();
        }

        private void CancelFade()
        {
            if (_fadeCts == null) return;
            _fadeCts.Cancel();
            _fadeCts.Dispose();
            _fadeCts = null;
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
            var labelTmp = labelGo.AddComponent<PolyglotText>();
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
