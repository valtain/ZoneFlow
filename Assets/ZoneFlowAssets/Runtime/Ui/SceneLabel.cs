using UnityEngine;

namespace ZoneFlow
{
    /// <summary>씬 식별용 레이블. 개발·검증 목적으로 화면 좌상단에 씬 이름을 표시한다.</summary>
    public class SceneLabel : MonoBehaviour
    {
        [field: SerializeField] public string Label { get; private set; }

        private GUIStyle _style;

        private void Awake()
        {
            _style = new GUIStyle
            {
                fontSize = 36,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
            };
        }

        private void OnGUI()
        {
            // 그림자 효과
            GUI.color = new Color(0, 0, 0, 0.6f);
            GUI.Label(new Rect(12, 12, 500, 60), Label, _style);
            GUI.color = Color.white;
            GUI.Label(new Rect(10, 10, 500, 60), Label, _style);
        }
    }
}
