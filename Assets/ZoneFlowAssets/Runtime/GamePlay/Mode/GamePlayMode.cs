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

        /// <summary>로드된 Zone 인스턴스. PlayAsync 이후에 유효하다.</summary>
        protected Zone Zone { get; private set; }

        /// <summary>ZoneAsset과 초기 스폰 포인트 ID를 주입하여 모드를 생성한다.</summary>
        protected GamePlayMode(ZoneAsset zoneAsset = null, string spawnPointId = null)
        {
            ZoneAsset = zoneAsset;
            SpawnPointId = spawnPointId;
            State = ModeState.Created;
        }

        /// <summary>모드를 시작한다. Created → Played → ModeIn → Active 순으로 전이한다.</summary>
        internal async UniTask PlayAsync(GamePlayDirector director, CancellationToken ct)
        {
            Director = director;

            State = ModeState.Created;
            await OnCreatedAsync(ct);

            State = ModeState.Played;
            if (ZoneAsset != null)
                Zone = await director.ZoneRegistry.AcquireAsync(ZoneAsset, ct);
            await OnPlayedAsync(ct);

            State = ModeState.ModeIn;
            await OnModeInAsync(ct);

            State = ModeState.Active;
        }

        /// <summary>모드를 슬립 상태로 전환한다. Active → ModeOut → Slept 순으로 전이한다.</summary>
        internal async UniTask SleepAsync(CancellationToken ct)
        {
            State = ModeState.ModeOut;
            await OnModeOutAsync(ct);

            State = ModeState.Slept;
            await OnSleptAsync(ct);
        }

        /// <summary>슬립 상태의 모드를 재개한다. Slept → Resumed → ModeIn → Active 순으로 전이한다.</summary>
        internal async UniTask ResumeAsync(CancellationToken ct)
        {
            State = ModeState.Resumed;
            await OnResumedAsync(ct);

            State = ModeState.ModeIn;
            await OnModeInAsync(ct);

            State = ModeState.Active;
        }

        /// <summary>슬립 상태의 모드를 ModeOut 없이 소멸시킨다. ReplaceAll에서 슬립 모드 정리에 사용한다.</summary>
        internal async UniTask DestroySleptAsync(CancellationToken ct)
        {
            State = ModeState.Stopped;
            await OnStoppedAsync(ct);

            if (ZoneAsset != null)
                await Director.ZoneRegistry.ReleaseAsync(ZoneAsset.ZoneId);

            State = ModeState.Destroyed;
            await OnDestroyedAsync(ct);
        }

        /// <summary>모드를 중지하고 소멸시킨다. Active → ModeOut → Stopped → Destroyed 순으로 전이한다.</summary>
        internal async UniTask StopAndDestroyAsync(CancellationToken ct)
        {
            State = ModeState.ModeOut;
            await OnModeOutAsync(ct);

            State = ModeState.Stopped;
            await OnStoppedAsync(ct);

            if (ZoneAsset != null)
                await Director.ZoneRegistry.ReleaseAsync(ZoneAsset.ZoneId);

            State = ModeState.Destroyed;
            await OnDestroyedAsync(ct);
        }

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
