using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;

namespace ZoneFlow.Player
{
    /// <summary>
    /// 플레이어 시스템의 진입점. Unity 생명주기를 소유하고 <see cref="PlayerContext"/>를 생성한다.
    /// 카메라 등 씬 공유 참조는 여기서 직렬화해 <see cref="PlayerContext"/>를 통해 배포한다.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerDirectionResolver))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerRotation))]
    [RequireComponent(typeof(PlayerAnimator))]
    public sealed class PlayerController : MonoBehaviour
    {
        /// <summary>씬의 메인 카메라. null이면 Awake에서 Camera.main으로 대체된다.</summary>
        [field: SerializeField] public Camera MainCamera { get; private set; }

        /// <summary>플레이어를 추적하는 Cinemachine 카메라. 비우면 Awake에서 자식에서 자동 탐색한다.</summary>
        [SerializeField] private CinemachineCamera _vcam;

        // 존 진입 리빌 연출 파라미터.
        private const float RevealZoomFactor = 1.08f; // rest FOV 대비 시작 화각 배율(살짝 당겨진 상태에서 정착).
        private const float RevealDuration   = 0.7f;

        private PlayerContext      _ctx;
        private PlayerStateMachine _stateMachine;
        private float              _restFov;
        private Tween              _revealTween;

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            // 워프 통지에 쓸 Follow 타깃의 이동 전 월드 위치를 캡처한다(회전·오프셋까지 정확히 반영).
            var follow = _vcam != null ? _vcam.Follow : null;
            var beforeFollowPos = follow != null ? follow.position : transform.position;

            var cc = GetComponent<CharacterController>();
            cc.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            cc.enabled = true;
            _ctx.Resolver.Reset();

            // 텔레포트를 Cinemachine에 워프로 통지하여 댐핑 슬라이드 없이 카메라가 깔끔하게 컷되도록 한다.
            if (_vcam != null && follow != null)
                _vcam.OnTargetObjectWarped(follow, follow.position - beforeFollowPos);

        }

        /// <summary>존 진입 시 짧은 FOV 줌 정착으로 가벼운 리빌 연출을 재생한다.</summary>
        public void PlayEntryReveal()
        {
            if (_vcam == null) return;
            _revealTween.Stop();
            _revealTween = Tween.Custom(
                startValue: _restFov * RevealZoomFactor, endValue: _restFov, duration: RevealDuration,
                onValueChange: fov => _vcam.Lens.FieldOfView = fov, ease: Ease.OutCubic);
        }

        private void Awake()
        {
            if (MainCamera == null)
                MainCamera = CameraService.Instance.MainCamera;

            Debug.Assert(MainCamera != null, "[PlayerController] MainCamera를 찾을 수 없습니다.");

            if (_vcam == null)
                _vcam = GetComponentInChildren<CinemachineCamera>(true);
            Debug.Assert(_vcam != null, "[PlayerController] CinemachineCamera를 찾을 수 없습니다.");
            if (_vcam != null)
                _restFov = _vcam.Lens.FieldOfView; // 리빌 기준 화각을 트윈 이전에 캐싱한다.

            var inputHandler = GetComponent<PlayerInputHandler>();
            var resolver     = GetComponent<PlayerDirectionResolver>();
            var movement     = GetComponent<PlayerMovement>();
            var rotation     = GetComponent<PlayerRotation>();
            var animator     = GetComponent<PlayerAnimator>();

            Debug.Assert(inputHandler != null, "[PlayerController] PlayerInputHandler 없음");
            Debug.Assert(resolver     != null, "[PlayerController] PlayerDirectionResolver 없음");
            Debug.Assert(movement     != null, "[PlayerController] PlayerMovement 없음");
            Debug.Assert(rotation     != null, "[PlayerController] PlayerRotation 없음");
            Debug.Assert(animator     != null, "[PlayerController] PlayerAnimator 없음");

            _ctx = new PlayerContext(inputHandler, resolver, movement, rotation, animator, transform, MainCamera);

            resolver.Initialize(MainCamera);

            _stateMachine     = new PlayerStateMachine();
            _ctx.StateMachine = _stateMachine;

            _stateMachine.ChangeState(new IdleState(_ctx));
        }

        private void Update()
        {
            // Resolver를 먼저 갱신하여 StateMachine이 최신 방향을 읽을 수 있도록 한다.
            _ctx.Resolver.Tick(_ctx.Input.MoveInput, Time.deltaTime);
        }

        private void LateUpdate()
        {
            // Cinemachine이 LateUpdate에서 카메라를 갱신하므로 StateMachine은 그 이후에 실행한다.
            _stateMachine.Update(Time.deltaTime);
        }

        private void OnDestroy() => _revealTween.Stop();
    }
}
