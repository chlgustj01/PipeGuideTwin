using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PipeGuideTwin.Core;
using PipeGuideTwin.Data;
using PipeGuideTwin.Data.Mock;
using PipeGuideTwin.UI;

namespace PipeGuideTwin.Visualization
{
    public sealed class MockRouteVisualController : MonoBehaviour
    {
        public PipeGuideAppManager App;
        public HiFiVisualFixtures Fixtures;
        public PipeGuideTheme Theme;
        public Transform VisualRoot;
        public Material[] RouteMaterials;
        public Camera BeforeCamera;
        public Camera AfterCamera;
        public Camera ProjectCamera;
        public Image[] InitialBorders;
        public Image[] LocalBorders;
        public Button[] InitialButtons;
        public Button[] LocalButtons;
        private readonly List<GameObject> initial=new List<GameObject>();
        private readonly List<GameObject> local=new List<GameObject>();
        private GameObject original,collision;
        public int SelectedCandidate {get;private set;}
        public int VisibleRouteCount {get;private set;}
        private void Awake()
        {
            for(int i=0;i<3;i++)
            {
                initial.Add(Path(Fixtures.Initial[i],RouteMaterials[i],25+i));
                local.Add(Path(Fixtures.Local[i],RouteMaterials[i],25+i));
                int index=i;
                InitialButtons[i].onClick.AddListener(()=>SelectCandidate(index));
                LocalButtons[i].onClick.AddListener(()=>SelectCandidate(index));
            }
            original=Path(Fixtures.Original,RouteMaterials[3],28);
            collision=new GameObject("MockCollisionMarker");collision.transform.SetParent(VisualRoot,false);collision.layer=28;
            var at=EngineeringUnitConverter.PositionMmToUnity(Fixtures.CollisionPositionMm);
            for(int i=0;i<2;i++)
            {
                var g=new GameObject("CollisionCross_"+i);g.layer=28;g.transform.SetParent(collision.transform,false);
                var line=g.AddComponent<LineRenderer>();line.sharedMaterial=RouteMaterials[3];line.widthMultiplier=.08f;line.positionCount=2;
                line.SetPosition(0,at+new Vector3(-.25f,i==0?-.25f:.25f,0));line.SetPosition(1,at+new Vector3(.25f,i==0?.25f:-.25f,0));
            }
        }
        private GameObject Path(MockPathVisual data,Material material,int layer)
        {
            var go=new GameObject(data.Id);go.layer=layer;go.transform.SetParent(VisualRoot,false);
            var line=go.AddComponent<LineRenderer>();line.sharedMaterial=material;line.positionCount=data.PointsMm.Length;line.widthMultiplier=.09f;line.numCapVertices=5;line.numCornerVertices=5;
            line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            for(int j=0;j<data.PointsMm.Length;j++)line.SetPosition(j,EngineeringUnitConverter.PositionMmToUnity(data.PointsMm[j]));
            return go;
        }
        private void OnEnable()=>App.StageChanged+=StageChanged;
        private void Start()=>StageChanged(App.CurrentStage);
        private void OnDisable()=>App.StageChanged-=StageChanged;
        private void StageChanged(WorkflowStage stage)
        {
            bool init=stage==WorkflowStage.InitialRouting, reroute=stage==WorkflowStage.LocalRerouting, compare=stage==WorkflowStage.ResultCompare;
            for(int i=0;i<3;i++){initial[i].SetActive(init);local[i].SetActive(reroute || compare&&i==1);}
            original.SetActive(reroute||compare);collision.SetActive(reroute||compare);
            BeforeCamera.enabled=compare;AfterCamera.enabled=compare;ProjectCamera.enabled=stage==WorkflowStage.Project;
            VisibleRouteCount=init?3:reroute?4:compare?2:0;
            SelectCandidate(0);
        }
        public void SelectCandidate(int index)
        {
            if(index<0||index>2)return;SelectedCandidate=index;
            for(int i=0;i<3;i++)
            {
                initial[i].GetComponent<LineRenderer>().widthMultiplier=i==index?.13f:.065f;
                local[i].GetComponent<LineRenderer>().widthMultiplier=i==index?.13f:.065f;
                InitialBorders[i].color=i==index?Theme.SelectionCyan:Theme.Border;
                LocalBorders[i].color=i==index?Theme.SelectionCyan:Theme.Border;
            }
        }
    }
}
