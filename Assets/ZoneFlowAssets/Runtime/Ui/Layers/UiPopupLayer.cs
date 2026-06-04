using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>팝업 UI를 스택 기반으로 관리하는 레이어 (sortingOrder 300~320).</summary>
    public sealed class UiPopupLayer : UiLayer
    {
        [field: SerializeField] public Transform SubLayerRoot { get; private set; } = default;
        [field: SerializeField] public Transform TopLayerRoot { get; private set; } = default;
        [SerializeField] private UiBgCover _bgCover = default;

        private readonly List<UiPopup> _stack = new();
        private bool _isBusy;

        /// <summary>현재 스택에 있는 팝업의 개수.</summary>
        public int Count => _stack.Count;

        private void Update()
        {
            if (_isBusy || _stack.Count == 0) return;
            if (Input.GetKeyDown(KeyCode.Escape))
                PopAsync(destroyCancellationToken).Forget();
        }

        /// <summary>팝업을 스택에 push한다. 기존 top은 SubLayer로 이동하고 새 팝업이 TopLayer에 생성된다.</summary>
        public async UniTask<T> PushAsync<T>(T prefab, CancellationToken ct) where T : UiPopup
        {
            Debug.Assert(!_isBusy, "[UiPopupLayer] Push/Pop이 중첩 호출되었습니다.");
            Debug.Assert(prefab != null, "[UiPopupLayer] prefab이 null입니다.");
            _isBusy = true;

            try
            {
                if (_stack.Count > 0)
                    _stack[^1].transform.SetParent(SubLayerRoot, worldPositionStays: false);

                var popup = Object.Instantiate(prefab, TopLayerRoot);
                _stack.Add(popup);

                if (_stack.Count == 1)
                    await _bgCover.FadeInAsync(0.3f, 0.6f, ct);

                await popup.OnPushedAsync(ct);
                return popup;
            }
            finally
            {
                _isBusy = false;
            }
        }

        /// <summary>최상위 팝업을 스택에서 pop하고 제거한다.</summary>
        public async UniTask PopAsync(CancellationToken ct)
        {
            Debug.Assert(!_isBusy, "[UiPopupLayer] Push/Pop이 중첩 호출되었습니다.");
            Debug.Assert(_stack.Count > 0, "[UiPopupLayer] 스택이 비어 있습니다.");
            _isBusy = true;

            try
            {
                var top = _stack[^1];
                _stack.RemoveAt(_stack.Count - 1);

                await top.OnPoppedAsync(ct);
                Object.Destroy(top.gameObject);

                if (_stack.Count > 0)
                    _stack[^1].transform.SetParent(TopLayerRoot, worldPositionStays: false);
                else
                    await _bgCover.FadeOutAsync(0.3f, ct);
            }
            finally
            {
                _isBusy = false;
            }
        }

        /// <summary>스택의 모든 팝업을 순서대로 pop한다.</summary>
        public async UniTask PopAllAsync(CancellationToken ct)
        {
            while (_stack.Count > 0)
                await PopAsync(ct);
        }

    }
}
