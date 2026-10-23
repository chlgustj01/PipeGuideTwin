using UnityEngine;
using PipeGuideTwin.Data;

namespace PipeGuideTwin.Core
{
    public sealed class PipeGuideProjectContext : MonoBehaviour
    {
        public string ProjectName = "PipeGuide Twin Demo";
        public string VesselName = "DEMO VESSEL";
        public string ZoneName = "ENGINE ROOM";
        public string Revision = "REV.A";
        [SerializeField] private WorkflowStage currentStage = WorkflowStage.Project;
        public WorkflowStage CurrentStage => currentStage;
        internal void SetStage(WorkflowStage stage) => currentStage = stage;
        // Original inputs/models are read-only. Future edits belong to a working copy,
        // followed by a separate revision; STEP 0 never writes design data.
    }
}
