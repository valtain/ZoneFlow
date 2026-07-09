using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZoneFlow.Battle;

namespace ZoneFlow
{
    /// <summary>
    /// BattleMode 전면 전투 UI(MainView). 파티/적 HP·이름, 현재 행동자, 액션·타겟 선택, 결과 연출을 담당한다.
    /// 엔진(<see cref="BattleAction"/>/<see cref="ActionResult"/>)에는 관여하지 않고 표현만 다룬다.
    /// </summary>
    public sealed class BattlePanel : UiPanel
    {
        public const string PanelId = "battle";

        /// <summary>플레이어가 고를 수 있는 단일 행동 선택지. SkillPower가 null이면 기본공격, 아니면 스킬.</summary>
        public readonly struct BattleActionOption
        {
            /// <summary>버튼 라벨(기본공격 / 스킬 DisplayName).</summary>
            public string Label { get; }

            /// <summary>null이면 Attack, 값이 있으면 해당 파워의 Skill.</summary>
            public int? SkillPower { get; }

            /// <summary>행동 선택지를 생성한다.</summary>
            /// <param name="label">버튼 라벨.</param>
            /// <param name="skillPower">스킬 파워(null이면 기본공격).</param>
            public BattleActionOption(string label, int? skillPower)
            {
                Label      = label;
                SkillPower = skillPower;
            }
        }

        [SerializeField] private CanvasGroup    _canvasGroup;
        [SerializeField] private RectTransform  _partyContainer;
        [SerializeField] private RectTransform  _enemyContainer;
        [SerializeField] private RectTransform  _combatantRowTemplate;
        [SerializeField] private TextMeshProUGUI _actorLabel;
        [SerializeField] private RectTransform  _actionButtonContainer;
        [SerializeField] private RectTransform  _actionButtonTemplate;
        [SerializeField] private TextMeshProUGUI _logText;

        private const float FadeDuration     = 0.25f;
        private const float ResultHoldDelay  = 0.5f;

        private static readonly Color DefaultRowColor      = new(0.12f, 0.12f, 0.12f, 0.75f);
        private static readonly Color ActorHighlightColor  = new(0.9f, 0.85f, 0.2f, 0.85f);
        private static readonly Color TargetSelectableColor = new(0.85f, 0.25f, 0.25f, 0.85f);

        private readonly Dictionary<int, BattleCombatantRow> _rows = new();
        private readonly List<GameObject> _spawnedButtons = new();
        private IReadOnlyDictionary<int, string> _names;

        /// <summary>
        /// 3D 캡슐 타겟 클릭을 시작하는 브리지(있으면). BattleMode가 BattleStage.BeginTargetPicking을 주입한다.
        /// BattlePanel은 BattleStage를 직접 참조하지 않는다(Ui/BattleView 레이어링 유지).
        /// </summary>
        private Func<IReadOnlyList<int>, Action<int>, IDisposable> _beginExternalTargeting;
        private IDisposable _externalTargetingScope;

        private void Awake()
        {
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
        }

        /// <summary>전투 개시 시 로스터·표시명을 주입한다(HP/이름 렌더 준비).</summary>
        public void Initialize(IReadOnlyList<Combatant> roster, IReadOnlyDictionary<int, string> names)
        {
            Debug.Assert(roster != null, "[BattlePanel] roster가 null입니다.");
            _names = names;
            ClearRows();

            foreach (var combatant in roster)
            {
                var parent = combatant.Side == BattleSide.Player ? _partyContainer : _enemyContainer;
                var rowInstance = Instantiate(_combatantRowTemplate, parent);
                rowInstance.gameObject.SetActive(true);

                var refs = rowInstance.GetComponent<BattleCombatantRow>();
                Debug.Assert(refs != null, "[BattlePanel] CombatantRow 템플릿에 BattleCombatantRow가 없습니다.");

                refs.NameLabel.text     = ResolveName(combatant.Id);
                refs.RowBg.color        = DefaultRowColor;
                refs.TargetButton.interactable = false;

                _rows[combatant.Id] = refs;
            }

            if (_actorLabel != null) _actorLabel.text = string.Empty;
            if (_logText != null) _logText.text = string.Empty;
        }

        /// <summary>
        /// 플레이어 턴: 현재 행동자의 선택지·생존 타겟을 제시하고, 입력을 <see cref="BattleAction"/>으로 완성해 반환한다.
        /// </summary>
        /// <param name="beginExternalTargeting">
        /// 3D 캡슐 클릭 타겟팅을 시작하는 브리지(있으면). BattleMode가 BattleStage.BeginTargetPicking을 주입한다.
        /// null이면 UI 행 클릭만으로 타겟을 선택한다.
        /// </param>
        public UniTask<BattleAction> AwaitPlayerActionAsync(
            Combatant current,
            IReadOnlyList<BattleActionOption> options,
            IReadOnlyList<Combatant> aliveTargets,
            CancellationToken ct,
            Func<IReadOnlyList<int>, Action<int>, IDisposable> beginExternalTargeting = null)
        {
            Debug.Assert(current != null, "[BattlePanel] current가 null입니다.");
            Debug.Assert(options != null && options.Count > 0, "[BattlePanel] options가 비어 있습니다.");
            Debug.Assert(aliveTargets != null && aliveTargets.Count > 0, "[BattlePanel] aliveTargets가 비어 있습니다.");

            if (_actorLabel != null) _actorLabel.text = $"{ResolveName(current.Id)}'s turn";
            HighlightActor(current.Id);

            _beginExternalTargeting = beginExternalTargeting;

            var tcs = new UniTaskCompletionSource<BattleAction>();
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);

            BuildActionButtons(current, options, aliveTargets, tcs);

            return AwaitAndCleanupAsync(tcs, linkedCts);
        }

        /// <summary>제출된 액션 결과를 연출한다(데미지·처치 로그 표시. HP는 캐릭터 위 월드 HUD가 담당한다).</summary>
        public async UniTask PresentActionAsync(ActionResult result, Combatant actor, Combatant target, CancellationToken ct)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);

            if (_logText != null)
                _logText.text = result.IsKilled
                    ? $"{ResolveName(actor.Id)} hits {ResolveName(target.Id)} for {result.DamageDealt} - defeated!"
                    : $"{ResolveName(actor.Id)} hits {ResolveName(target.Id)} for {result.DamageDealt}";

            await UniTask.Delay((int)(ResultHoldDelay * 1000), cancellationToken: linked.Token);
        }

        protected override async UniTask OnShowAsync(CancellationToken ct)
        {
            await Tween.Alpha(_canvasGroup, 1f, FadeDuration).ToUniTask(cancellationToken: ct);
        }

        protected override async UniTask OnHideAsync(CancellationToken ct)
        {
            await Tween.Alpha(_canvasGroup, 0f, FadeDuration).ToUniTask(cancellationToken: ct);
        }

        // ──────────────────────────────────────────────────────────────────
        // 입력 대기 내부 구현
        // ──────────────────────────────────────────────────────────────────

        private async UniTask<BattleAction> AwaitAndCleanupAsync(
            UniTaskCompletionSource<BattleAction> tcs, CancellationTokenSource linkedCts)
        {
            try
            {
                return await tcs.Task.AttachExternalCancellation(linkedCts.Token);
            }
            finally
            {
                ClearHighlight();
                ClearActionButtons();
                ClearTargetSelection();
                linkedCts.Dispose();
            }
        }

        private void BuildActionButtons(
            Combatant current, IReadOnlyList<BattleActionOption> options,
            IReadOnlyList<Combatant> aliveTargets, UniTaskCompletionSource<BattleAction> tcs)
        {
            ClearActionButtons();

            foreach (var option in options)
            {
                var buttonInstance = Instantiate(_actionButtonTemplate, _actionButtonContainer);
                buttonInstance.gameObject.SetActive(true);
                _spawnedButtons.Add(buttonInstance.gameObject);

                var refs = buttonInstance.GetComponent<BattleActionButton>();
                Debug.Assert(refs != null, "[BattlePanel] ActionButton 템플릿에 BattleActionButton이 없습니다.");
                refs.Label.text = option.Label;

                var captured = option;
                refs.Button.onClick.AddListener(() =>
                {
                    HideActionButtons();
                    BeginTargetSelection(current, captured, aliveTargets, tcs);
                });
            }
        }

        private void BeginTargetSelection(
            Combatant current, BattleActionOption option,
            IReadOnlyList<Combatant> aliveTargets, UniTaskCompletionSource<BattleAction> tcs)
        {
            var targetIds = new List<int>(aliveTargets.Count);

            foreach (var target in aliveTargets)
            {
                targetIds.Add(target.Id);

                if (!_rows.TryGetValue(target.Id, out var row))
                {
                    Debug.Assert(false, $"[BattlePanel] 타겟 Id={target.Id}에 대응하는 row가 없습니다.");
                    continue;
                }

                row.TargetButton.interactable = true;
                row.RowBg.color = TargetSelectableColor;

                var targetId = target.Id;
                row.TargetButton.onClick.AddListener(() => CompleteTargetSelection(current, option, targetId, tcs));
            }

            // UI 행 클릭과 동등하게, 3D 캡슐 클릭으로도 같은 액션을 완성할 수 있게 브리지한다.
            if (_beginExternalTargeting != null)
                _externalTargetingScope = _beginExternalTargeting(targetIds,
                    targetId => CompleteTargetSelection(current, option, targetId, tcs));
        }

        private static void CompleteTargetSelection(
            Combatant current, BattleActionOption option, int targetId, UniTaskCompletionSource<BattleAction> tcs)
        {
            var kind = option.SkillPower.HasValue ? BattleActionKind.Skill : BattleActionKind.Attack;
            var action = new BattleAction(kind, current.Id, targetId, option.SkillPower);
            tcs.TrySetResult(action);
        }

        private void HideActionButtons()
        {
            foreach (var go in _spawnedButtons)
                go.SetActive(false);
        }

        private void ClearActionButtons()
        {
            foreach (var go in _spawnedButtons)
                Destroy(go);
            _spawnedButtons.Clear();
        }

        private void ClearTargetSelection()
        {
            foreach (var row in _rows.Values)
            {
                row.TargetButton.onClick.RemoveAllListeners();
                row.TargetButton.interactable = false;
            }

            _externalTargetingScope?.Dispose();
            _externalTargetingScope = null;
        }

        private void HighlightActor(int actorId)
        {
            foreach (var kvp in _rows)
                kvp.Value.RowBg.color = kvp.Key == actorId ? ActorHighlightColor : DefaultRowColor;
        }

        private void ClearHighlight()
        {
            foreach (var row in _rows.Values)
                row.RowBg.color = DefaultRowColor;
        }

        private void ClearRows()
        {
            foreach (var row in _rows.Values)
                if (row != null) Destroy(row.gameObject);
            _rows.Clear();
        }

        private string ResolveName(int id)
            => _names != null && _names.TryGetValue(id, out var name) ? name : $"Combatant {id}";

#if UNITY_EDITOR
        [ContextMenu("Build Battle UI")]
        private void BuildBattleUi()
        {
            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0).gameObject);

            var cg = GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

            // ── Background (전면 딤) ──────────────────────────────────────
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(transform, false);
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            var bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.03f, 0.03f, 0.05f, 0.55f);
            bgImg.raycastTarget = false;

            // ── ActorLabel (상단 중앙) ────────────────────────────────────
            var actorLabelGo = new GameObject("ActorLabel");
            actorLabelGo.transform.SetParent(transform, false);
            var actorLabelRect = actorLabelGo.AddComponent<RectTransform>();
            actorLabelRect.anchorMin        = new Vector2(0.5f, 1f);
            actorLabelRect.anchorMax        = new Vector2(0.5f, 1f);
            actorLabelRect.pivot            = new Vector2(0.5f, 1f);
            actorLabelRect.anchoredPosition = new Vector2(0f, -30f);
            actorLabelRect.sizeDelta        = new Vector2(700f, 50f);
            var actorLabelTmp = actorLabelGo.AddComponent<TextMeshProUGUI>();
            actorLabelTmp.text      = string.Empty;
            actorLabelTmp.fontSize  = 32;
            actorLabelTmp.alignment = TextAlignmentOptions.Center;
            actorLabelTmp.color     = new Color(1f, 0.9f, 0.4f);

            // ── PartyContainer (좌측, 세로 목록) ───────────────────────────
            var partyGo = new GameObject("PartyContainer");
            partyGo.transform.SetParent(transform, false);
            var partyRect = partyGo.AddComponent<RectTransform>();
            partyRect.anchorMin        = new Vector2(0f, 0f);
            partyRect.anchorMax        = new Vector2(0f, 1f);
            partyRect.pivot            = new Vector2(0f, 0.5f);
            partyRect.anchoredPosition = new Vector2(40f, 0f);
            partyRect.sizeDelta        = new Vector2(280f, 0f);
            var partyLayout = partyGo.AddComponent<VerticalLayoutGroup>();
            partyLayout.childAlignment      = TextAnchor.MiddleCenter;
            partyLayout.spacing             = 12f;
            partyLayout.childForceExpandWidth  = true;
            partyLayout.childForceExpandHeight = false;
            partyLayout.childControlWidth   = true;
            partyLayout.childControlHeight  = false;
            partyLayout.padding             = new RectOffset(0, 0, 160, 160);
            var partyFitter = partyGo.AddComponent<ContentSizeFitter>();
            partyFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── EnemyContainer (우측, 세로 목록) ───────────────────────────
            var enemyGo = new GameObject("EnemyContainer");
            enemyGo.transform.SetParent(transform, false);
            var enemyRect = enemyGo.AddComponent<RectTransform>();
            enemyRect.anchorMin        = new Vector2(1f, 0f);
            enemyRect.anchorMax        = new Vector2(1f, 1f);
            enemyRect.pivot            = new Vector2(1f, 0.5f);
            enemyRect.anchoredPosition = new Vector2(-40f, 0f);
            enemyRect.sizeDelta        = new Vector2(280f, 0f);
            var enemyLayout = enemyGo.AddComponent<VerticalLayoutGroup>();
            enemyLayout.childAlignment      = TextAnchor.MiddleCenter;
            enemyLayout.spacing             = 12f;
            enemyLayout.childForceExpandWidth  = true;
            enemyLayout.childForceExpandHeight = false;
            enemyLayout.childControlWidth   = true;
            enemyLayout.childControlHeight  = false;
            enemyLayout.padding             = new RectOffset(0, 0, 160, 160);
            var enemyFitter = enemyGo.AddComponent<ContentSizeFitter>();
            enemyFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── LogText (하단, 액션버튼 위) ─────────────────────────────────
            var logGo = new GameObject("LogText");
            logGo.transform.SetParent(transform, false);
            var logRect = logGo.AddComponent<RectTransform>();
            logRect.anchorMin        = new Vector2(0.5f, 0f);
            logRect.anchorMax        = new Vector2(0.5f, 0f);
            logRect.pivot            = new Vector2(0.5f, 0f);
            logRect.anchoredPosition = new Vector2(0f, 130f);
            logRect.sizeDelta        = new Vector2(900f, 40f);
            var logTmp = logGo.AddComponent<TextMeshProUGUI>();
            logTmp.text      = string.Empty;
            logTmp.fontSize  = 22;
            logTmp.alignment = TextAlignmentOptions.Center;
            logTmp.color     = Color.white;

            // ── ActionButtonContainer (하단 중앙, 가로 목록) ─────────────────
            var actionGo = new GameObject("ActionButtonContainer");
            actionGo.transform.SetParent(transform, false);
            var actionRect = actionGo.AddComponent<RectTransform>();
            actionRect.anchorMin        = new Vector2(0.5f, 0f);
            actionRect.anchorMax        = new Vector2(0.5f, 0f);
            actionRect.pivot            = new Vector2(0.5f, 0f);
            actionRect.anchoredPosition = new Vector2(0f, 40f);
            actionRect.sizeDelta        = new Vector2(900f, 64f);
            var actionLayout = actionGo.AddComponent<HorizontalLayoutGroup>();
            actionLayout.childAlignment      = TextAnchor.MiddleCenter;
            actionLayout.spacing             = 16f;
            actionLayout.childForceExpandWidth  = false;
            actionLayout.childForceExpandHeight = true;
            actionLayout.childControlWidth   = false;
            actionLayout.childControlHeight  = true;
            var actionFitter = actionGo.AddComponent<ContentSizeFitter>();
            actionFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            // ── Templates (비활성 원본) ──────────────────────────────────────
            var templatesGo = new GameObject("Templates");
            templatesGo.transform.SetParent(transform, false);
            var templatesRect = templatesGo.AddComponent<RectTransform>();
            templatesRect.anchoredPosition = Vector2.zero;

            var rowRect = BuildCombatantRowTemplate(templatesGo.transform);
            var buttonRect = BuildActionButtonTemplate(templatesGo.transform);
            templatesGo.SetActive(false);

            // ── SerializedField 연결 ──────────────────────────────────────
            var so = new UnityEditor.SerializedObject(this);
            so.FindProperty("_canvasGroup").objectReferenceValue           = cg;
            so.FindProperty("_partyContainer").objectReferenceValue        = partyRect;
            so.FindProperty("_enemyContainer").objectReferenceValue        = enemyRect;
            so.FindProperty("_combatantRowTemplate").objectReferenceValue  = rowRect;
            so.FindProperty("_actorLabel").objectReferenceValue            = actorLabelTmp;
            so.FindProperty("_actionButtonContainer").objectReferenceValue = actionRect;
            so.FindProperty("_actionButtonTemplate").objectReferenceValue  = buttonRect;
            so.FindProperty("_logText").objectReferenceValue               = logTmp;
            so.ApplyModifiedProperties();

            UnityEditor.EditorUtility.SetDirty(gameObject);
            Debug.Log("[BattlePanel] Battle UI 요소 생성 완료");
        }

        private static RectTransform BuildCombatantRowTemplate(Transform parent)
        {
            var rowGo = new GameObject("CombatantRowTemplate");
            rowGo.transform.SetParent(parent, false);
            var rowRect = rowGo.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(260f, 56f);
            var rowBg = rowGo.AddComponent<Image>();
            rowBg.color = DefaultRowColor;
            var rowButton = rowGo.AddComponent<Button>();
            rowButton.targetGraphic = rowBg;
            rowButton.transition    = Selectable.Transition.None;

            // HP는 캐릭터 위 월드 HUD(BattleActorView)가 담당하므로 행은 이름 + 타겟 클릭만 갖는다.
            var nameGo = new GameObject("NameLabel");
            nameGo.transform.SetParent(rowGo.transform, false);
            var nameRect = nameGo.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0f);
            nameRect.anchorMax = new Vector2(1f, 1f);
            nameRect.offsetMin = new Vector2(8f, 0f);
            nameRect.offsetMax = new Vector2(-8f, 0f);
            var nameTmp = nameGo.AddComponent<TextMeshProUGUI>();
            nameTmp.text      = "Name";
            nameTmp.fontSize  = 20;
            nameTmp.alignment = TextAlignmentOptions.Left;
            nameTmp.color     = Color.white;
            nameTmp.raycastTarget = false;

            var rowRefs = rowGo.AddComponent<BattleCombatantRow>();
            var rowSo = new UnityEditor.SerializedObject(rowRefs);
            rowSo.FindProperty("_nameLabel").objectReferenceValue   = nameTmp;
            rowSo.FindProperty("_rowBg").objectReferenceValue       = rowBg;
            rowSo.FindProperty("_targetButton").objectReferenceValue = rowButton;
            rowSo.ApplyModifiedProperties();

            return rowRect;
        }

        private static RectTransform BuildActionButtonTemplate(Transform parent)
        {
            var btnGo = new GameObject("ActionButtonTemplate");
            btnGo.transform.SetParent(parent, false);
            var btnRect = btnGo.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(200f, 56f);
            var btnImg = btnGo.AddComponent<Image>();
            btnImg.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(btnGo.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
            var labelTmp = labelGo.AddComponent<TextMeshProUGUI>();
            labelTmp.text      = "Action";
            labelTmp.fontSize  = 22;
            labelTmp.alignment = TextAlignmentOptions.Center;
            labelTmp.color     = Color.white;
            labelTmp.raycastTarget = false;

            var btnRefs = btnGo.AddComponent<BattleActionButton>();
            var btnSo = new UnityEditor.SerializedObject(btnRefs);
            btnSo.FindProperty("_label").objectReferenceValue  = labelTmp;
            btnSo.FindProperty("_button").objectReferenceValue = btn;
            btnSo.ApplyModifiedProperties();

            return btnRect;
        }
#endif
    }
}
