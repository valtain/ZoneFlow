using Cysharp.Threading.Tasks;

namespace ZoneFlow
{
    /// <summary>
    /// UiTransitionFxLayer의 Begin/End 쌍을 await using으로 자동 관리한다.
    /// IAsyncDisposable 없이 duck typing으로 동작한다.
    /// <code>await using var _ = await UiService.Transition&lt;FadeScreen&gt;(ct);</code>
    /// </summary>
    public sealed class TransitionFxScope
    {
        private readonly UiTransitionFxLayer _layer;
        internal TransitionFxScope(UiTransitionFxLayer layer) => _layer = layer;
        public UniTask DisposeAsync() => _layer.EndAsync();
    }
}
