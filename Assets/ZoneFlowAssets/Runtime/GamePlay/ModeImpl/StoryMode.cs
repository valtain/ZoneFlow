using System.Threading;
using Cysharp.Threading.Tasks;

namespace ZoneFlow
{
    /// <summary>스토리 연출 모드. SpawnPoint에 플레이어를 배치하고 HUD·내러티브 대화를 표시한다.</summary>
    public sealed class StoryMode : GamePlayMode
    {
        // 첫 스토리 진입 시 시작할 Yarn 노드. 이후 Zone 전환에서는 재시작하지 않는다(진행 상태 유지).
        private const string EntryDialogueNode = "intro";

        private StoryHudPanel _hud;

        /// <summary>ZoneAsset과 스폰 포인트 ID로 스토리 모드를 생성한다.</summary>
        public StoryMode(ZoneAsset zoneAsset = null, string spawnPointId = null)
            : base(zoneAsset, spawnPointId, zoneAsset != null) { }

        /// <summary>Zone 로드 직후 HUD를 숨김 상태로 생성하고, 콘텐츠 세션이 있으면 대화를 셋업한다.</summary>
        protected override async UniTask OnPlayedAsync(CancellationToken ct)
        {
            if (UiService.Instance.Panels == null) return;

            await SetupHudAsync(ct);
            await SetupDialogueAsync(ct);
        }

        private async UniTask SetupHudAsync(CancellationToken ct)
        {
            if (!UiService.Instance.Panels.TryGetPanel(StoryHudPanel.PanelId, out var prefab)) return;
            if (prefab.asset is not StoryHudPanel hudPrefab) return;

            _hud = await UiService.Instance.SetMainViewAsync(hudPrefab, ct);
            _hud.Initialize(ZoneAsset);
        }

        /// <summary>
        /// 대화 패널을 Overlay에 띄우고 DialogueService에 연결한다. Overlay·ContentServices는 Zone 전환과
        /// 무관한 수명이므로, 한 번 시작한 대화는 Zone을 오가도 진행 상태가 유지된다(AQ-2/AQ-3).
        /// </summary>
        private async UniTask SetupDialogueAsync(CancellationToken ct)
        {
            // ContentServices가 로드돼 있지 않으면(콘텐츠 세션 밖) 대화를 셋업하지 않는다.
            if (!DialogueService.IsReady) return;
            // 이미 이 세션에서 대화를 시작했다면(= Zone 전환 중) 패널·진행 상태를 그대로 유지한다.
            if (DialogueService.Instance.HasStarted) return;
            if (!UiService.Instance.Panels.TryGetPanel(DialoguePanel.PanelId, out var prefab)) return;
            if (prefab.asset is not DialoguePanel dialoguePrefab) return;

            var panel = await UiService.Instance.SetOverlayAsync(dialoguePrefab, ct);
            DialogueService.Instance.BindPresenters(panel.Presenters);
            DialogueService.Instance.StartDialogue(EntryDialogueNode);
        }

        /// <summary>모드 진입 시 플레이어를 배치하고 HUD를 슬라이드인한다.</summary>
        protected override async UniTask OnModeInAsync(CancellationToken ct)
        {
            await UiService.Instance.ShowMainViewAsync(ct);
        }

        /// <summary>모드 퇴장 시 HUD를 슬라이드아웃한다.</summary>
        protected override UniTask OnModeOutAsync(CancellationToken ct)
            => UiService.Instance.HideMainViewAsync(ct);

        /// <summary>모드 종료 시 HUD 인스턴스를 파괴한다.</summary>
        protected override UniTask OnStoppedAsync(CancellationToken ct)
        {
            UiService.Instance.ClearMainViewIfIs(_hud);
            _hud = null;
            return UniTask.CompletedTask;
        }
    }
}
