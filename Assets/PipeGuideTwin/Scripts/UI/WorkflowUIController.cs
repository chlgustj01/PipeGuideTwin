using UnityEngine;
using UnityEngine.UI;
using PipeGuideTwin.Core;
using PipeGuideTwin.Data;

namespace PipeGuideTwin.UI
{
    public sealed class WorkflowUIController : MonoBehaviour
    {
        [SerializeField] private PipeGuideAppManager app;
        [System.Serializable] public sealed class PageBinding
        {
            public WorkflowStage Stage;
            public GameObject Root;
            public Transform RightPanel;
            public Button Navigation;
            public GameObject ActiveIndicator;
            public bool ShowsWorld;
        }
        [SerializeField] private PageBinding[] pages;
        [SerializeField] private PipeGuideTheme theme;
        [SerializeField] private Transform inspectorRoot;
        [SerializeField] private Transform debugRoot;
        [SerializeField] private Text status;
        [SerializeField] private Interaction.SelectionManager selection;
        [SerializeField] private Interaction.EngineeringCameraController cameraController;
        private UnityEngine.Events.UnityAction[] listeners;
        public System.Collections.Generic.IReadOnlyList<PageBinding> Pages => pages;
        private void OnEnable()
        {
            app.StageChanged += Refresh;
            listeners = new UnityEngine.Events.UnityAction[pages.Length];
            for (int i=0;i<pages.Length;i++)
            {
                var stage=pages[i].Stage;
                listeners[i]=()=>app.SetStage(stage);
                pages[i].Navigation.onClick.AddListener(listeners[i]);
            }
            Refresh(app.CurrentStage);
        }
        private void OnDisable()
        {
            app.StageChanged -= Refresh;
            for (int i=0;i<pages.Length;i++) pages[i].Navigation.onClick.RemoveListener(listeners[i]);
        }
        private void Refresh(WorkflowStage stage)
        {
            PageBinding active=null;
            foreach (var page in pages) if(page.Stage==stage) active=page;
            if(active==null) return;
            bool interactive=stage==WorkflowStage.Model3D || stage==WorkflowStage.ConsistencyCheck;
            // Keep the STEP 1 inspector instance and its subscriptions; move it between viewer pages.
            if (interactive)
            {
                inspectorRoot.SetParent(active.RightPanel,false);
                debugRoot.SetParent(active.RightPanel,false);
            }
            foreach(var page in pages)
            {
                bool selected=page==active;
                page.Root.SetActive(selected);
                page.ActiveIndicator.SetActive(selected);
                page.Navigation.GetComponentInChildren<Text>().color=selected ? theme.PrimaryOrange : theme.TextSecondary;
            }
            selection.enabled=interactive;
            cameraController.enabled=active.ShowsWorld;
            if(!interactive) status.text=active.Navigation.GetComponentInChildren<Text>().text+"  |  준비 완료";
        }
    }
}
