using System.Threading;
using Cysharp.Threading.Tasks;

namespace ZoneFlow
{
    /// <summary>
    /// 상호작용 프롬프트 등 플로팅 UI를 표시하는 레이어. 단일 패널 인스턴스를 소유한다.
    /// 가시성(페이드)은 패널이 자체 이벤트로 구동하므로 이 레이어는 생성·파괴만 담당한다.
    /// </summary>
    public sealed class UiFloatingLayer : UiLayer
    {
        private UiPanel _current;

        /// <summary>지정한 프리팹을 이 레이어에 인스턴스화한다. 패널은 숨김 상태로 생성된다.</summary>
        public UniTask<T> SetAsync<T>(T prefab, CancellationToken ct) where T : UiPanel
        {
            var panel = Instantiate(prefab, transform);
            _current = panel;
            return UniTask.FromResult(panel);
        }

        /// <summary>
        /// panel이 현재 패널이면 파괴하고 _current를 비운다.
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
