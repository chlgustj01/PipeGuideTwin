using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PipeGuideTwin.Core;
using PipeGuideTwin.Data;
using PipeGuideTwin.Interaction;
using PipeGuideTwin.UI;
using PipeGuideTwin.Validation;

namespace PipeGuideTwin.Visualization
{
    public sealed class HiFiIssueMarkers : MonoBehaviour
    {
        public PipeGuideAppManager App;
        public ValidationResultController Results;
        public Material[] Materials;
        public Transform Root;
        private readonly List<GameObject> markers=new List<GameObject>();
        private void OnEnable(){App.StageChanged+=Stage;Results.ResultsChanged+=Rebuild;Rebuild();}
        private void OnDisable(){App.StageChanged-=Stage;Results.ResultsChanged-=Rebuild;}
        private void Stage(WorkflowStage stage)=>Rebuild();
        private void Rebuild()
        {
            foreach(var marker in markers){marker.SetActive(false);Destroy(marker);}markers.Clear();
            if(App.CurrentStage!=WorkflowStage.ConsistencyCheck)return;
            var objects=UnityEngine.Object.FindObjectsByType<PipeGuideSelectable>(FindObjectsSortMode.None);
            foreach(var issue in Results.Issues)
            {
                if(issue.Status==ValidationStatus.Match||issue.Status==ValidationStatus.Missing)continue;
                var item=objects.FirstOrDefault(x=>x.ObjectId==issue.ObjectId);if(item==null)continue;
                var renderer=item.GetComponentsInChildren<Renderer>().FirstOrDefault(x=>x.enabled);if(renderer==null)continue;
                var p=renderer.bounds.center;p.y=renderer.bounds.max.y+.32f;
                var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name="IssueMarker_"+issue.IssueId;g.transform.SetParent(Root,false);g.transform.position=p;g.transform.localScale=Vector3.one*.21f;
                Destroy(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=Materials[issue.Status==ValidationStatus.Warning?0:issue.Status==ValidationStatus.Duplicate?2:1];markers.Add(g);
            }
        }
    }
}
