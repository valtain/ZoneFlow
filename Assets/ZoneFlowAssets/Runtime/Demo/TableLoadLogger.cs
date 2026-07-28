using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ZoneFlow
{
    /// <summary>
    /// Localization String Table 로드를 관찰하는 데모 전용 컴포넌트. Polyglot 폰트 엔진과는 무관한 별개
    /// 계층으로, 씬에 이미 있는 LocalizeStringEvent와 동일하게 <see cref="LocalizationSettings.StringDatabase"/>를
    /// 직접 사용한다(폰트 엔진의 Localization 격리 원칙은 폰트에만 해당하므로 위배가 아니다).
    /// OnEnable에 테이블을 프리로드하고 로그를 남기며, OnDisable에 핸들을 해제하고 로그를 남긴다
    /// (Approach A: lazy-load 관찰 — 별도 스코프드 로더는 두지 않는다).
    /// </summary>
    public class TableLoadLogger : MonoBehaviour
    {
        /// <summary>관찰할 String Table 이름(예: "IntroStrings", "MenuStrings").</summary>
        [SerializeField] private string _tableName;

        private AsyncOperationHandle _handle;
        private bool _hasHandle;

        private void OnEnable()
        {
            PreloadAsync().Forget();
        }

        private async UniTaskVoid PreloadAsync()
        {
            Debug.Assert(!string.IsNullOrEmpty(_tableName), "[TableLoadLogger] 테이블 이름이 비어 있습니다.");

            Locale locale = LocalizationSettings.SelectedLocale;
            _handle = LocalizationSettings.StringDatabase.PreloadTables(_tableName, locale);
            _hasHandle = true;

            await _handle.ToUniTask(cancellationToken: destroyCancellationToken);

            Debug.Log($"[Polyglot] loaded table {_tableName}");
        }

        private void OnDisable()
        {
            if (!_hasHandle)
            {
                return;
            }
            _hasHandle = false;

            // Localization refcount가 이미 해제했을 수 있어(다른 참조 없음) 유효할 때만 Release한다 — Approach A는
            // refcount 실동작에 얹히는 관찰 방식이므로, 무효 핸들 Release로 예외를 던지지 않는 것이 정상 흐름이다.
            if (_handle.IsValid())
            {
                _handle.Release();
            }

            Debug.Log($"[Polyglot] released table {_tableName}");
        }
    }
}
