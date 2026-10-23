using UnityEngine;
using UnityEngine.UI;

namespace PipeGuideTwin.UI
{
    // Local UI state only. Never opens a file or saves an approval/revision.
    public sealed class ProductShellActions : MonoBehaviour
    {
        [SerializeField] private Button fileButton;
        [SerializeField] private Text fileMessage;
        [SerializeField] private Button approveButton;
        [SerializeField] private Button rejectButton;
        [SerializeField] private Text decisionMessage;
        [SerializeField] private PipeGuideTheme theme;
        private void OnEnable()
        {
            fileButton.onClick.AddListener(ChooseFile);
            approveButton.onClick.AddListener(Approve);
            rejectButton.onClick.AddListener(Reject);
        }
        private void OnDisable()
        {
            fileButton.onClick.RemoveListener(ChooseFile);
            approveButton.onClick.RemoveListener(Approve);
            rejectButton.onClick.RemoveListener(Reject);
        }
        private void ChooseFile() => fileMessage.text="선택 대기 · 파일 입력 미연결";
        private void Approve() {decisionMessage.text="승인됨 · 화면 상태";decisionMessage.color=theme.SuccessGreen;}
        private void Reject() {decisionMessage.text="반려됨 · 화면 상태";decisionMessage.color=theme.DangerRed;}
    }
}
