using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZoneFlow
{
    /// <summary>액션 선택 버튼 1개의 라벨·버튼 참조를 묶는 컴포넌트. <see cref="BattlePanel"/>이 런타임에 복제해 사용한다.</summary>
    internal sealed class BattleActionButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private Button _button;

        /// <summary>버튼 라벨 텍스트.</summary>
        public TextMeshProUGUI Label => _label;

        /// <summary>클릭 이벤트를 받는 버튼.</summary>
        public Button Button => _button;
    }
}
