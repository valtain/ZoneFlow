using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZoneFlow
{
    /// <summary>
    /// 전투원 1명의 이름·타겟 버튼 참조를 묶는 로우 컴포넌트. <see cref="BattlePanel"/>이 런타임에 복제해 사용한다.
    /// HP는 캐릭터 위 월드 HUD(<c>BattleActorView</c>)가 담당하므로 이 행은 보유하지 않는다.
    /// </summary>
    internal sealed class BattleCombatantRow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private Image _rowBg;
        [SerializeField] private Button _targetButton;

        /// <summary>전투원 이름 라벨.</summary>
        public TextMeshProUGUI NameLabel => _nameLabel;

        /// <summary>행동자 하이라이트/타겟 선택 강조에 사용하는 배경 이미지.</summary>
        public Image RowBg => _rowBg;

        /// <summary>타겟 선택 클릭을 받는 버튼.</summary>
        public Button TargetButton => _targetButton;
    }
}
