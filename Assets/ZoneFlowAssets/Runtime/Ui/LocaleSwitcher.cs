using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace ZoneFlow
{
    /// <summary>
    /// 런타임 언어 스위처. 드롭다운 선택을 <see cref="FontService.SelectLocaleAsync"/> 호출로 연결하고,
    /// 활성화 시 현재 locale로 드롭다운 값을 이벤트 발생 없이 동기화한다.
    /// </summary>
    [RequireComponent(typeof(TMP_Dropdown))]
    public sealed class LocaleSwitcher : MonoBehaviour
    {
        /// <summary>드롭다운 옵션 순서에 대응하는 locale 코드 목록.</summary>
        [SerializeField] private string[] _localeCodes = { "en", "ko", "ja", "zh-Hans" };

        [SerializeField] private TMP_Dropdown _dropdown;

        private void Reset()
        {
            _dropdown = GetComponent<TMP_Dropdown>();
        }

        private void OnEnable()
        {
            Debug.Assert(FontService.IsReady, "[LocaleSwitcher] CoreServices에 FontService가 없습니다.");
            if (_dropdown == null)
            {
                _dropdown = GetComponent<TMP_Dropdown>();
            }

            SyncToCurrentLocale();
            _dropdown.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable()
        {
            _dropdown.onValueChanged.RemoveListener(OnValueChanged);
        }

        /// <summary>현재 활성 locale에 맞춰 드롭다운 값을 이벤트 발생 없이 동기화한다.</summary>
        private void SyncToCurrentLocale()
        {
            if (!FontService.IsReady)
            {
                return;
            }

            string current = FontService.Instance.CurrentLocaleCode;
            int index = Array.IndexOf(_localeCodes, current);
            if (index >= 0)
            {
                _dropdown.SetValueWithoutNotify(index);
            }
        }

        private void OnValueChanged(int index)
        {
            Debug.Assert(FontService.IsReady, "[LocaleSwitcher] CoreServices에 FontService가 없습니다.");
            FontService.Instance.SelectLocaleAsync(_localeCodes[index]).Forget();
        }
    }
}
