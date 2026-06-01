using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>
    /// 화면 전환 효과 레이어 (sortingOrder=500).
    /// count 기반 Begin/End 쌍으로 효과를 관리한다. count가 0이 될 때만 효과를 종료한다.
    /// </summary>
    public sealed class UiTransitionFxLayer : UiLayer
    {
        [SerializeField] private UiBgCover _bgCover = default;
        [Header("FadeScreen")]
        [SerializeField] private FadeScreen _fadeScreen = new();

        private int _count;
        private IUiTransitionEffect _activeEffect;
        private Dictionary<Type, IUiTransitionEffect> _effects;

        private void Awake()
        {
            _effects = new Dictionary<Type, IUiTransitionEffect>();
            Register(_fadeScreen);
        }

        /// <summary>효과를 등록하고 초기화한다.</summary>
        private void Register(IUiTransitionEffect effect)
        {
            effect.Initialize(_bgCover);
            _effects[effect.GetType()] = effect;
        }

        /// <summary>전환 효과를 시작한다. count가 이미 1 이상이면 즉시 반환한다.</summary>
        public async UniTask BeginAsync<T>(CancellationToken ct = default) where T : IUiTransitionEffect
        {
            _count++;
            if (_count > 1) return;

            Debug.Assert(_effects.ContainsKey(typeof(T)),
                $"[UiTransitionFxLayer] 효과 타입 {typeof(T).Name}이 등록되지 않았습니다.");

            _activeEffect = _effects[typeof(T)];
            gameObject.SetActive(true);
            await _activeEffect.ShowAsync(ct);
        }

        /// <summary>전환 효과를 종료한다. count가 0이 될 때만 HideAsync를 호출한다.</summary>
        public async UniTask EndAsync()
        {
            Debug.Assert(_count > 0, "[UiTransitionFxLayer] EndAsync가 BeginAsync 없이 호출되었습니다.");
            _count = Mathf.Max(0, _count - 1);
            if (_count > 0) return;

            await _activeEffect.HideAsync();
            gameObject.SetActive(false);
            _activeEffect = null;
        }

        /// <summary>BeginAsync를 호출하고 TransitionFxScope를 반환한다. await using으로 EndAsync를 자동 호출한다.</summary>
        public async UniTask<TransitionFxScope> Scope<T>(CancellationToken ct = default)
            where T : IUiTransitionEffect
        {
            await BeginAsync<T>(ct);
            return new TransitionFxScope(this);
        }
    }
}
