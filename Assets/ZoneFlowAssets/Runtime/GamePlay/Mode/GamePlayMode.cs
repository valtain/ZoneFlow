using System.Threading;
using Cysharp.Threading.Tasks;

namespace ZoneFlow
{
    /// <summary>게임플레이 모드의 추상 베이스 클래스. 생명주기 상태 머신을 소유한다.</summary>
    public abstract class GamePlayMode
    {
        /// <summary>현재 모드 생명주기 상태.</summary>
        public ModeState State { get; private set; }

        /// <summary>이 모드를 소유하는 GamePlayDirector.</summary>
        protected GamePlayDirector Director { get; private set; }

        /// <summary>이 모드가 로드할 ZoneAsset. null이면 존 로드를 수행하지 않는다.</summary>
        protected ZoneAsset ZoneAsset { get; }

        /// <summary>이 모드의 초기 스폰 포인트 ID.</summary>
        protected string SpawnPointId { get; }

        /// <summary>로드된 Zone 인스턴스. PlayedAsync 이후에 유효하다.</summary>
        protected Zone Zone { get; private set; }

        /// <summary>ZoneAsset과 초기 스폰 포인트 ID를 주입하여 모드를 생성한다.</summary>
        protected GamePlayMode(ZoneAsset zoneAsset = null, string spawnPointId = null)
        {
            ZoneAsset = zoneAsset;
            SpawnPointId = spawnPointId;
            State = ModeState.Created;
        }

        // ── 상태 전이 메서드 (Director가 호출, 상태 하나당 함수 하나) ─────────────────

        /// <summary>Created 상태. Director와 모드를 초기화한다.</summary>
        internal async UniTask CreatedAsync(GamePlayDirector director, CancellationToken ct)
        {
            Director = director;
            State = ModeState.Created;
            await OnCreatedAsync(ct);
        }

        /// <summary>Played 상태. Zone을 로드하고 플레이어를 배치한다.</summary>
        internal async UniTask PlayedAsync(CancellationToken ct)
        {
            State = ModeState.Played;
            if (ZoneAsset != null)
                Zone = await Director.ZoneRegistry.AcquireAsync(ZoneAsset, ct);
            await OnPlayedAsync(ct);
        }

        /// <summary>ModeIn 상태. 진입 연출 후 Active로 전이한다.</summary>
        internal async UniTask ModeInAsync(CancellationToken ct)
        {
            State = ModeState.ModeIn;
            await OnModeInAsync(ct);
            State = ModeState.Active;
        }

        /// <summary>ModeOut 상태. 퇴장 연출을 수행한다.</summary>
        internal async UniTask ModeOutAsync(CancellationToken ct)
        {
            State = ModeState.ModeOut;
            await OnModeOutAsync(ct);
        }

        /// <summary>Slept 상태. 슬립 처리를 수행한다.</summary>
        internal async UniTask SleptAsync(CancellationToken ct)
        {
            State = ModeState.Slept;
            await OnSleptAsync(ct);
        }

        /// <summary>Resumed 상태. 재개 처리를 수행한다.</summary>
        internal async UniTask ResumedAsync(CancellationToken ct)
        {
            State = ModeState.Resumed;
            await OnResumedAsync(ct);
        }

        /// <summary>Stopped 상태. Zone을 언로드하고 정리한다.</summary>
        internal async UniTask StoppedAsync(CancellationToken ct)
        {
            State = ModeState.Stopped;
            await OnStoppedAsync(ct);
            if (ZoneAsset != null)
                await Director.ZoneRegistry.ReleaseAsync(ZoneAsset.ZoneId);
        }

        /// <summary>Destroyed 상태. 모드를 소멸시킨다.</summary>
        internal async UniTask DestroyedAsync(CancellationToken ct)
        {
            State = ModeState.Destroyed;
            await OnDestroyedAsync(ct);
        }

        // ── 훅 ────────────────────────────────────────────────────────────────────────

        /// <summary>Created 상태 진입 시 호출되는 훅.</summary>
        protected virtual UniTask OnCreatedAsync(CancellationToken ct) => UniTask.CompletedTask;

        /// <summary>Played 상태 진입 시 호출되는 훅.</summary>
        protected virtual UniTask OnPlayedAsync(CancellationToken ct) => UniTask.CompletedTask;

        /// <summary>ModeIn 상태 진입 시 호출되는 훅. 플레이어 텔레포트 등 진입 연출을 수행한다.</summary>
        protected virtual UniTask OnModeInAsync(CancellationToken ct) => UniTask.CompletedTask;

        /// <summary>ModeOut 상태 진입 시 호출되는 훅. 퇴장 연출을 수행한다.</summary>
        protected virtual UniTask OnModeOutAsync(CancellationToken ct) => UniTask.CompletedTask;

        /// <summary>Slept 상태 진입 시 호출되는 훅.</summary>
        protected virtual UniTask OnSleptAsync(CancellationToken ct) => UniTask.CompletedTask;

        /// <summary>Resumed 상태 진입 시 호출되는 훅.</summary>
        protected virtual UniTask OnResumedAsync(CancellationToken ct) => UniTask.CompletedTask;

        /// <summary>Stopped 상태 진입 시 호출되는 훅.</summary>
        protected virtual UniTask OnStoppedAsync(CancellationToken ct) => UniTask.CompletedTask;

        /// <summary>Destroyed 상태 진입 시 호출되는 훅.</summary>
        protected virtual UniTask OnDestroyedAsync(CancellationToken ct) => UniTask.CompletedTask;
    }
}
