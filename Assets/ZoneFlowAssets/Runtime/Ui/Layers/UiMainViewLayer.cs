using System.Threading;
using Cysharp.Threading.Tasks;

namespace ZoneFlow
{
    /// <summary>인게임 뷰(HUD 등)를 표시하는 레이어. 단일 패널 인스턴스를 소유하며 Show/Hide로 가시성을 제어한다.</summary>
    public sealed class UiMainViewLayer : UiLayer
    {
        private UiPanel _current;

        /// <summary>지정한 프리팹을 이 레이어에 인스턴스화한다. 패널은 숨김 상태로 생성된다.</summary>
        public UniTask<T> SetAsync<T>(T prefab, CancellationToken ct) where T : UiPanel
        {
            var panel = Instantiate(prefab, transform);
            _current = panel;
            return UniTask.FromResult(panel);
        }

        /// <summary>현재 패널을 표시한다.</summary>
        public async UniTask ShowAsync(CancellationToken ct)
        {
            if (_current == null) return;
            await _current.ShowAsync(ct);
        }

        /// <summary>현재 패널을 숨긴다.</summary>
        public async UniTask HideAsync(CancellationToken ct)
        {
            if (_current == null) return;
            await _current.HideAsync(ct);
        }

        /// <summary>현재 패널을 파괴한다. 호출 전에 HideAsync로 퇴장 연출을 완료해야 한다.</summary>
        public void Clear()
        {
            if (_current == null) return;
            Destroy(_current.gameObject);
            _current = null;
        }

        /// <summary>
        /// panel이 현재 패널이면 Clear()와 동일하게 동작한다.
        /// panel이 이미 다른 패널로 교체된 경우에는 panel만 직접 파괴하고 _current는 건드리지 않는다.
        /// </summary>
        public void ClearIfIs(UiPanel panel)
        {
            if (panel == null) return;
            if (_current == panel)
            {
                Destroy(_current.gameObject);
                _current = null;
            }
            else
            {
                Destroy(panel.gameObject);
            }
        }
    }
}
