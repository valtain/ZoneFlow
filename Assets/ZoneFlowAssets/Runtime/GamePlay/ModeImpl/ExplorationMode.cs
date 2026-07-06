using System.Threading;
using Cysharp.Threading.Tasks;
using ZoneFlow.Battle;

namespace ZoneFlow
{
    /// <summary>자유 탐색 모드. SpawnPoint에 플레이어를 배치하고 HUD를 표시한다.</summary>
    public sealed class ExplorationMode : GamePlayMode
    {
        private ExplorationHudPanel _hud;
        private InteractionPromptPanel _prompt;

        /// <summary>ZoneAsset과 스폰 포인트 ID로 탐색 모드를 생성한다.</summary>
        public ExplorationMode(ZoneAsset zoneAsset = null, string spawnPointId = null)
            : base(zoneAsset, spawnPointId, zoneAsset != null) { }

        /// <summary>Zone 로드 직후 HUD와 상호작용 프롬프트를 숨김 상태로 생성한다.</summary>
        protected override async UniTask OnPlayedAsync(CancellationToken ct)
        {
            if (UiService.Instance.Panels == null) return;
            if (!UiService.Instance.Panels.TryGetPanel(ExplorationHudPanel.PanelId, out var prefab)) return;
            if (prefab.asset is not ExplorationHudPanel hudPrefab) return;

            _hud = await UiService.Instance.SetMainViewAsync(hudPrefab, ct);
            _hud.Initialize(ZoneAsset);

            if (UiService.Instance.Panels.TryGetPanel(InteractionPromptPanel.PanelId, out var promptRef)
                && promptRef.asset is InteractionPromptPanel promptPrefab)
            {
                _prompt = await UiService.Instance.SetFloatingAsync(promptPrefab, ct);
            }
        }

        /// <summary>모드 진입 시 플레이어를 배치하고 HUD를 슬라이드인한 뒤 프롬프트를 연결한다.</summary>
        protected override async UniTask OnModeInAsync(CancellationToken ct)
        {
            await UiService.Instance.ShowMainViewAsync(ct);
            _prompt?.Bind();
        }

        /// <summary>모드 퇴장 시 프롬프트 구독을 해제하고 HUD를 슬라이드아웃한다.</summary>
        protected override UniTask OnModeOutAsync(CancellationToken ct)
        {
            _prompt?.Unbind();
            return UiService.Instance.HideMainViewAsync(ct);
        }

        /// <summary>
        /// 전투 모드 pop 후 Resume 시 전투 결과를 pull한다.
        /// 패배(<see cref="BattleResult.Lose"/>)이면 Village(허브)로 ReplaceAll(게임오버 복귀).
        /// 승리·도주·결과 없음이면 탐색을 계속한다(ADR-0002).
        /// </summary>
        protected override async UniTask OnResumedAsync(CancellationToken ct)
        {
            var outcome = BattleService.IsReady
                ? BattleService.Instance.ConsumeOutcome()
                : null;

            if (outcome != null && outcome.Result == BattleResult.Lose)
            {
                // 패배: Village(허브)로 ReplaceAll — 게임오버 복귀
                // "gameplay://exploration/village?switch=replaceall" 는
                // GamePlayNavigationTests·포털 사용처에서 확인한 실 허브 진입 URI다.
                await Director.NavigateAsync(
                    "gameplay://exploration/village?switch=replaceall", ct);
                return;
            }

            // 승리·도주·결과 없음: 탐색 계속 (base는 Zone 재활성화·SpawnPlayer를 이미 수행함)
        }

        /// <summary>모드 종료 시 HUD와 프롬프트 인스턴스를 파괴한다.</summary>
        protected override UniTask OnStoppedAsync(CancellationToken ct)
        {
            UiService.Instance.ClearMainViewIfIs(_hud);
            _hud = null;
            UiService.Instance.ClearFloatingIfIs(_prompt);
            _prompt = null;
            return UniTask.CompletedTask;
        }
    }
}
