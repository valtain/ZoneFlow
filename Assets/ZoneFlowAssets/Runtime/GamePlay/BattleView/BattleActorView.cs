using System.Threading;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

namespace ZoneFlow.BattleView
{
    /// <summary>
    /// 전투원 1명의 3D 스탠드인 뷰. 결정론 엔진(<c>ZoneFlow.Battle</c>)은 이 타입을 모른다.
    /// 프리미티브(캡슐 + side 머티리얼) 기본 구현이며, <c>_renderer</c>를 비우면 실모델 서브클래스가
    /// 자체 렌더링을 담당하도록 no-op 처리한다(교체 seam).
    /// </summary>
    public class BattleActorView : MonoBehaviour
    {
        [SerializeField] private Transform _root;
        [SerializeField] private Renderer _renderer;

        private const float LungeDistanceRatio = 0.8f;
        private const float LungeOutDuration = 0.15f;
        private const float LungeBackDuration = 0.15f;

        private const float HitFlashDuration = 0.1f;
        private const float HitPunchDuration = 0.12f;
        private const float HitPunchStrength = 0.25f;

        private const float DieDuration = 0.5f;
        private const float DieSinkDistance = 0.5f;

        private Color _baseColor = Color.white;

        /// <summary>이 전투원 뷰의 월드 위치(연출 대상 좌표로 사용).</summary>
        public Vector3 Position => _root != null ? _root.position : transform.position;

        /// <summary>
        /// 진영 머티리얼을 적용한다. 실모델 서브클래스는 <c>_renderer</c>를 비워 no-op으로 만들 수 있다.
        /// </summary>
        /// <param name="sideMaterial">적용할 진영 머티리얼(아군/적).</param>
        public void ApplyMaterial(Material sideMaterial)
        {
            if (_renderer == null || sideMaterial == null) return;

            _renderer.material = sideMaterial;
            _baseColor = _renderer.material.color;
        }

        /// <summary>대상 쪽으로 80% 전진 후 복귀하는 공격 런지 연출.</summary>
        /// <param name="targetPos">런지 목표 월드 위치.</param>
        /// <param name="ct">취소 토큰.</param>
        public async UniTask LungeAsync(Vector3 targetPos, CancellationToken ct)
        {
            if (_root == null) return;

            var origin = _root.position;
            var lungePos = Vector3.Lerp(origin, targetPos, LungeDistanceRatio);

            await Tween.Position(_root, lungePos, LungeOutDuration, Ease.OutQuad)
                .ToUniTask(cancellationToken: ct);
            await Tween.Position(_root, origin, LungeBackDuration, Ease.InQuad)
                .ToUniTask(cancellationToken: ct);
        }

        /// <summary>화이트 플래시(머티리얼 컬러 요요)와 공격자 반대 방향 펀치를 동시에 재생하는 피격 연출.</summary>
        /// <param name="fromAttackerPos">공격자 월드 위치(펀치 방향 계산용).</param>
        /// <param name="ct">취소 토큰.</param>
        public async UniTask HitReactAsync(Vector3 fromAttackerPos, CancellationToken ct)
        {
            if (_root == null) return;

            var punchDir = _root.position - fromAttackerPos;
            punchDir.y = 0f;
            punchDir = punchDir.sqrMagnitude > 0.0001f ? punchDir.normalized : _root.forward;

            var flashTask = _renderer != null
                ? Tween.MaterialColor(_renderer.material, _baseColor, Color.white, HitFlashDuration,
                    cycles: 2, cycleMode: CycleMode.Yoyo).ToUniTask(cancellationToken: ct)
                : UniTask.CompletedTask;

            var punchTask = Tween.PunchLocalPosition(_root, punchDir * HitPunchStrength, HitPunchDuration)
                .ToUniTask(cancellationToken: ct);

            await UniTask.WhenAll(flashTask, punchTask);
        }

        /// <summary>기본 사망 연출: 스케일을 0으로 줄이며 살짝 침강시킨다(URP-Lit opaque는 alpha 페이드 불가).</summary>
        /// <param name="ct">취소 토큰.</param>
        public async UniTask DieAsync(CancellationToken ct)
        {
            if (_root == null) return;

            var sinkPos = _root.position + Vector3.down * DieSinkDistance;

            var scaleTask = Tween.Scale(_root, 0f, DieDuration, Ease.InQuad).ToUniTask(cancellationToken: ct);
            var sinkTask = Tween.Position(_root, sinkPos, DieDuration, Ease.InQuad).ToUniTask(cancellationToken: ct);

            await UniTask.WhenAll(scaleTask, sinkTask);
        }
    }
}
