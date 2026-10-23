using UnityEngine;
using PipeGuideTwin.Data;

namespace PipeGuideTwin.Core
{
    public sealed class PipeGuideAppManager : MonoBehaviour
    {
        [SerializeField] private PipeGuideProjectContext projectContext;
        public PipeGuideProjectContext ProjectContext => projectContext;
        public WorkflowStage CurrentStage => projectContext != null ? projectContext.CurrentStage : WorkflowStage.Project;
        public event System.Action<WorkflowStage> StageChanged;
        public void SetStage(WorkflowStage stage)
        {
            if (projectContext == null || CurrentStage == stage) return;
            projectContext.SetStage(stage);
            StageChanged?.Invoke(stage);
        }
    }
}
