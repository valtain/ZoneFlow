using UnityEngine;

namespace ZoneFlow.Ui
{
    /// <summary>
    /// Portal 컴포넌트의 DisplayLabel을 읽어 BillboardLabel에 바인딩한다.
    /// Portal_Gateway 프리팹의 WorldLabel 자식에 배치한다.
    /// </summary>
    public sealed class PortalLabelBinder : MonoBehaviour
    {
        [SerializeField] private BillboardLabel _billboard;
        [SerializeField] private Portal _portal;

        private void Start()
        {
            Debug.Assert(_billboard != null, "[PortalLabelBinder] BillboardLabel이 연결되지 않았습니다.");
            Debug.Assert(_portal != null, "[PortalLabelBinder] Portal이 연결되지 않았습니다.");
            if (_billboard == null || _portal == null) return;

            var text = string.IsNullOrWhiteSpace(_portal.DisplayLabel)
                ? _portal.InteractableId
                : _portal.DisplayLabel;
            _billboard.SetText(text);
        }
    }
}
