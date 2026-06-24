using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

namespace ZoneFlow
{
    /// <summary>
    /// YarnSpinner 대화를 표출하는 UI 패널. UiOverlayLayer에 표시되어 Zone 전환과 무관하게 유지된다.
    /// 내부 LinePresenter를 DialogueService에 연결해 라인을 렌더링한다.
    /// </summary>
    public sealed class DialoguePanel : UiPanel
    {
        public const string PanelId = "dialogue";

        [SerializeField] private LinePresenter _linePresenter;

        /// <summary>DialogueRunner에 연결할 presenter 목록.</summary>
        public IReadOnlyList<DialoguePresenterBase> Presenters => new DialoguePresenterBase[] { _linePresenter };

#if UNITY_EDITOR
        [ContextMenu("Create Dialogue Panel Elements")]
        private void CreateDialogueElements()
        {
            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0).gameObject);

            // 패널 루트를 전체화면으로 확장 (Overlay Canvas 아래 배치).
            var selfRect = transform as RectTransform;
            if (selfRect == null) selfRect = gameObject.AddComponent<RectTransform>();
            selfRect.anchorMin = Vector2.zero;
            selfRect.anchorMax = Vector2.one;
            selfRect.offsetMin = selfRect.offsetMax = Vector2.zero;

            // ── DialogueBox (하단 대화 박스, CanvasGroup으로 페이드) ──────────
            var boxGo = new GameObject("DialogueBox");
            boxGo.transform.SetParent(transform, false);
            var boxRect = boxGo.AddComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.5f, 0f);
            boxRect.anchorMax = new Vector2(0.5f, 0f);
            boxRect.pivot = new Vector2(0.5f, 0f);
            boxRect.anchoredPosition = new Vector2(0f, 40f);
            boxRect.sizeDelta = new Vector2(1200f, 220f);
            var boxImg = boxGo.AddComponent<UnityEngine.UI.Image>();
            boxImg.color = new Color(0.04f, 0.03f, 0.10f, 0.92f);
            var canvasGroup = boxGo.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            // ── LineText ─────────────────────────────────────────────────────
            var textGo = new GameObject("LineText");
            textGo.transform.SetParent(boxGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(30f, 30f);
            textRect.offsetMax = new Vector2(-30f, -30f);
            var tmp = textGo.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = string.Empty;
            tmp.fontSize = 30;
            tmp.alignment = TMPro.TextAlignmentOptions.TopLeft;
            tmp.color = Color.white;

            // ── LinePresenter 컴포넌트 부착 및 필드 와이어링 ─────────────────
            var presenter = boxGo.AddComponent<LinePresenter>();
            presenter.canvasGroup = canvasGroup;
            presenter.lineText = tmp;
            presenter.autoAdvance = true;        // 진행 버튼 없이 자동 진행 (데모용)
            presenter.autoAdvanceDelay = 1.5f;

            var so = new UnityEditor.SerializedObject(this);
            so.FindProperty("_linePresenter").objectReferenceValue = presenter;
            so.ApplyModifiedProperties();

            UnityEditor.EditorUtility.SetDirty(gameObject);
            Debug.Log("[DialoguePanel] 대화 패널 요소 생성 완료");
        }
#endif
    }
}
