using TMPro;
using UnityEngine;

namespace Polyglot
{
    /// <summary>
    /// 스타일 하나에 대응하는 폰트·머티리얼 프리셋(예: title, button).
    /// 부팅 시 등록되어 StyleSheet의 <c>&lt;font=&gt;</c> 이름 해석 대상이 된다.
    /// </summary>
    [System.Serializable]
    public struct FontPreset
    {
        /// <summary>스타일 이름(예: "title"). StyleSheet의 style 이름과 맞춘다.</summary>
        [field: SerializeField] public string Name { get; private set; }

        /// <summary>이 스타일이 사용할 폰트.</summary>
        [field: SerializeField] public TMP_FontAsset Font { get; private set; }

        /// <summary>이 스타일이 사용할 머티리얼(선택).</summary>
        [field: SerializeField] public Material Material { get; private set; }
    }
}
