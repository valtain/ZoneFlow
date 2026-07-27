using Cysharp.Threading.Tasks;
using UnityEngine.Localization.Settings;

namespace ZoneFlow
{
    /// <summary>
    /// Addressables·Localization 시스템 초기화를 소유하는 서비스. 폰트(<see cref="FontService"/>)·문자열 등
    /// Addressables를 경유해 로드하는 시스템은 로드 전에 <see cref="EnsureInitializedAsync"/>를 await해야 한다.
    /// (원격 콘텐츠 도입 시 카탈로그 업데이트·프리로드·다운로드도 이 서비스가 소유할 자리다 — 현재 미구현.)
    /// </summary>
    public sealed class AddressableService : MonoService<AddressableService>
    {
        private bool _initialized;

        /// <summary>Localization·Addressables 시스템을 1회 초기화한다(멱등). 초기화가 끝나면 즉시 반환한다.</summary>
        public async UniTask EnsureInitializedAsync()
        {
            if (_initialized)
            {
                return;
            }

            // Localization 초기화는 내부적으로 Addressables 초기화를 포함한다.
            await LocalizationSettings.InitializationOperation.ToUniTask();
            _initialized = true;
        }
    }
}
