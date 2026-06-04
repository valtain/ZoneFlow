using System.Threading;
using Cysharp.Threading.Tasks;

namespace ZoneFlow
{
    /// <summary>Shell 컨텐츠 패널을 표시하는 레이어. 스택 없이 단일 패널을 교체한다.</summary>
    public sealed class UiOverlayLayer : UiLayer
    {
        private UiPanel _current;

        /// <summary>지정한 프리팹을 이 레이어에 인스턴스화하고 이전 패널을 제거한다.</summary>
        public async UniTask<T> SetAsync<T>(T prefab, CancellationToken ct) where T : UiPanel
        {
            await ClearAsync(ct);
            var panel = Instantiate(prefab, transform);
            _current = panel;
            await panel.ShowAsync(ct);
            return panel;
        }

        /// <summary>현재 패널을 제거한다.</summary>
        public async UniTask ClearAsync(CancellationToken ct)
        {
            if (_current == null) return;
            await _current.HideAsync(ct);
            Destroy(_current.gameObject);
            _current = null;
        }
    }
}
