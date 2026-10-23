using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using PipeGuideTwin.Interaction;
using PipeGuideTwin.Visualization;

namespace PipeGuideTwin.Editor
{
    // Explicitly invoked integration check. Uses the same Mouse/InputSystem Update path as users.
    [InitializeOnLoad]
    public static class ProductShellPlayVerification
    {
        private const string Key = "PipeGuideTwin.ProductShellVerify";
        public const string ReportPath = "Temp/PipeGuideTwin-Shell-Tests.json";
        private static readonly Queue<Action> Steps = new Queue<Action>();
        private static readonly List<string> Results = new List<string>();
        private static Mouse mouse;
        private static SelectionManager selection;
        private static EngineeringCameraController controller;
        private static Camera camera;
        private static Text inspector;
        private static double next;
        private static Vector2 pointer;
        private static Vector3 initialPosition, initialPivot, oldPivot;
        private static Quaternion initialRotation, oldRotation;
        private static float oldDistance;
        private static InputSettings.BackgroundBehavior previousBackground;
        private static bool previousRunInBackground;
        private static bool initialized;
        private static int stableWidth, stableHeight;
        private static double stableSince;
        private static MouseState pendingState;
        private static bool hasPendingInput;

        static ProductShellPlayVerification() => EditorApplication.update += Tick;
        [MenuItem("PipeGuide Twin/Verify Product Shell in Play Mode")]
        public static void Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Start verification from Edit Mode.");
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save the scene first.");
            if (File.Exists(ReportPath)) File.Delete(ReportPath);
            SessionState.SetBool(Key, true);
            stableWidth = stableHeight = 0;
            stableSince = 0;
            EditorWindow.GetWindow(typeof(EditorApplication).Assembly.GetType("UnityEditor.GameView")).Focus();
            EditorApplication.isPlaying = true;
        }
        private static void Tick()
        {
            if (!SessionState.GetBool(Key, false) || !EditorApplication.isPlaying || EditorApplication.isPaused) return;
            if (EditorApplication.timeSinceStartup < next) return;
            next = EditorApplication.timeSinceStartup + .35;
            try
            {
                if (!initialized)
                {
                    if (Screen.width != stableWidth || Screen.height != stableHeight)
                    {
                        stableWidth = Screen.width; stableHeight = Screen.height;
                        stableSince = EditorApplication.timeSinceStartup;
                        return;
                    }
                    if (EditorApplication.timeSinceStartup - stableSince < 1.5) return;
                }
                if (!initialized) Initialize();
                else if (Steps.Count > 0) Steps.Dequeue()();
                else Finish(null);
            }
            catch (Exception ex) { Finish(ex.ToString()); }
        }
        private static void Initialize()
        {
            initialized = true;
            Results.Clear(); Steps.Clear();
            UnityEngine.Object.FindFirstObjectByType<PipeGuideTwin.Core.PipeGuideAppManager>().SetStage(PipeGuideTwin.Data.WorkflowStage.Model3D);
            camera = Camera.main;
            selection = UnityEngine.Object.FindFirstObjectByType<SelectionManager>();
            controller = camera.GetComponent<EngineeringCameraController>();
            inspector = GameObject.Find("InspectorPanel").GetComponentInChildren<Text>();
            initialPosition = camera.transform.position; initialRotation = camera.transform.rotation; initialPivot = controller.Pivot;
            previousBackground = InputSystem.settings.backgroundBehavior;
            previousRunInBackground = Application.runInBackground;
            Application.runInBackground = true;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            mouse = InputSystem.AddDevice<Mouse>("PipeGuideVerificationMouse");
            mouse.MakeCurrent();
            InputSystem.onBeforeUpdate += FlushPendingInput;
            var app=UnityEngine.Object.FindFirstObjectByType<PipeGuideTwin.Core.PipeGuideAppManager>();
            var workflow=UnityEngine.Object.FindFirstObjectByType<PipeGuideTwin.UI.WorkflowUIController>();
            var theme=AssetDatabase.LoadAssetAtPath<PipeGuideTwin.UI.PipeGuideTheme>("Assets/PipeGuideTwin/Materials/PipeGuideTheme.asset");
            var canvas=GameObject.Find("MainCanvas");
            var data=UnityEngine.Object.FindFirstObjectByType<PipeGuideTwin.Validation.ValidationResultController>();
            var revisions=Directory.GetFiles("Assets/PipeGuideTwin/Data/Revisions").Length;
            Check("Canvas reference resolution",canvas.GetComponent<CanvasScaler>().referenceResolution==new Vector2(1920,1080));
            Check("Eight page bindings",workflow.Pages.Count==8);
            Steps.Enqueue(()=>Send(Center()));
            foreach(var binding in workflow.Pages)
            {
                var page=binding;
                Click(page.Stage.ToString(),()=> {
                    Check(page.Root.name+" active",page.Root.activeInHierarchy && app.CurrentStage==page.Stage);
                    Check(page.Root.name+" exclusive",workflow.Pages.Count(x=>x.Root.activeSelf)==1);
                    Check(page.Root.name+" orange navigation",page.ActiveIndicator.activeSelf && page.Navigation.GetComponentInChildren<Text>().color==theme.PrimaryOrange);
                    Check(page.Root.name+" four regions",new[]{"LeftPanel","CenterWorkspace","RightPanel","BottomStatus"}.All(n=>page.Root.transform.Find(n)!=null));
                    Check(page.Root.name+" card text fits",page.Root.GetComponentsInChildren<Text>().Where(t=>t.name=="CardBody").All(t=>t.preferredHeight <= t.rectTransform.rect.height+.1f));
                    Capture("Temp/PipeGuideTwin-Shell-"+page.Root.name+".png");
                });
            }
            Click("ApproveMock",()=> {
                Check("Mock approve state",GameObject.Find("DecisionMessage").GetComponent<Text>().text.Contains("승인"));
                Check("Approval does not save revision",Directory.GetFiles("Assets/PipeGuideTwin/Data/Revisions").Length==revisions && app.ProjectContext.Revision=="REV.A");
            });
            Click("RejectMock",()=>Check("Mock reject state",GameObject.Find("DecisionMessage").GetComponent<Text>().text.Contains("반려")));
            Click("DataImport",()=>Check("Data page return",app.CurrentStage==PipeGuideTwin.Data.WorkflowStage.DataImport));
            Click("ChooseExcel",()=>Check("Upload placeholder only",GameObject.Find("FileMessage").GetComponent<Text>().text.Contains("다음 단계")));
            Click("Model3D",()=>Check("Model3D return",app.CurrentStage==PipeGuideTwin.Data.WorkflowStage.Model3D && selection.enabled));
            var pipe=Find("PIPE_001");var tank=Find("EQ_TANK_001");
            Steps.Enqueue(()=>Send(VisiblePoint(pipe),1));
            Steps.Enqueue(()=>Send(pointer));
            Steps.Enqueue(()=> {AssertSelected(pipe);Check("Pipe selection highlight",pipe.GetComponent<SelectionHighlight>().IsSelected && MaterialName(pipe)=="SelectionCyan");});
            Steps.Enqueue(()=>Send(VisiblePoint(tank),1));
            Steps.Enqueue(()=>Send(pointer));
            Steps.Enqueue(()=> {AssertSelected(tank);Check("Equipment highlight",tank.GetComponent<SelectionHighlight>().IsSelected && !pipe.GetComponent<SelectionHighlight>().IsSelected);});
            Steps.Enqueue(()=>Send(EmptyPoint(),1));
            Steps.Enqueue(()=>Send(pointer));
            Steps.Enqueue(()=>Check("Empty-space deselect",selection.Current==null && !tank.GetComponent<SelectionHighlight>().IsSelected));
            Steps.Enqueue(()=>Send(VisiblePoint(pipe),1));
            Steps.Enqueue(()=>Send(pointer));
            Steps.Enqueue(()=>Send(new Vector2(Screen.width*.08f,Screen.height*.65f),1));
            Steps.Enqueue(()=>Send(pointer));
            Steps.Enqueue(()=>Check("Left panel blocks 3D selection",selection.Current==pipe && ViewerPointer.IsOverUI(pointer)));
            Steps.Enqueue(()=>Send(new Vector2(Screen.width*.9f,Screen.height*.65f),1));
            Steps.Enqueue(()=>Send(pointer));
            Steps.Enqueue(()=>Check("Inspector blocks 3D selection",selection.Current==pipe && ViewerPointer.IsOverUI(pointer)));
            Steps.Enqueue(()=>{oldRotation=camera.transform.rotation;Send(Center(),2);});
            Steps.Enqueue(()=>Send(Center()+new Vector2(60,25),2,new Vector2(60,25)));
            Steps.Enqueue(()=>{Check("Orbit",Quaternion.Angle(oldRotation,camera.transform.rotation)>1);Send(pointer);});
            Steps.Enqueue(()=>{oldPivot=controller.Pivot;Send(Center(),4);});
            Steps.Enqueue(()=>Send(Center()+new Vector2(40,20),4,new Vector2(40,20)));
            Steps.Enqueue(()=>{Check("Pan",Vector3.Distance(oldPivot,controller.Pivot)>.01f);Send(pointer);});
            Steps.Enqueue(()=>{oldDistance=controller.Distance;Send(Center(),0,Vector2.zero,120);});
            Steps.Enqueue(()=>{Check("Zoom",controller.Distance<oldDistance);Send(pointer);camera.transform.SetPositionAndRotation(initialPosition,initialRotation);controller.SetPivot(initialPivot);});
            Click("ConsistencyCheck",()=>Check("Existing validation data retained",data.Issues.Count==5 && data.ProblemCount==4));
            Click("Issue_VAL_003",()=> {
                Check("Product Shell issue selection retained",selection.Current==Find("PIPE_003") && inspector.text.Contains("150 mm") && inspector.text.Contains("불일치"));
                Check("Product Shell status plus outline retained",MaterialName(Find("PIPE_003"))=="ValidationMismatch" && Find("PIPE_003").GetComponentInChildren<LineRenderer>().enabled);
                Capture("Temp/PipeGuideTwin-Shell-ConsistencyPage.png");
            });
            Click("Issue_VAL_005",()=>Check("Product Shell missing issue retained",selection.Current==null && data.SelectedIssue.ObjectId=="PIPE_005" && inspector.text.Contains("누락")));
            Click("Model3D",()=>Check("Normal inspector mode restored",inspector.text.Contains("객체 선택")));
            Steps.Enqueue(()=> {
                Check("Korean font available",canvas.GetComponentsInChildren<Text>(true).Where(t=>!string.IsNullOrWhiteSpace(t.text)).All(t=>t.font!=null && t.font.HasCharacter('한')));
                Check("No actual PIPE_005",!UnityEngine.Object.FindObjectsByType<PipeGuideSelectable>(FindObjectsSortMode.None).Any(x=>x.ObjectId=="PIPE_005"));
            });
            Click("Project",()=>Check("Final project page",app.CurrentStage==PipeGuideTwin.Data.WorkflowStage.Project));
        }
        private static string MaterialName(PipeGuideSelectable item) => item.GetComponentInChildren<MeshRenderer>().sharedMaterial.name;
        private static void Click(string name,Action assertion)
        {
            Steps.Enqueue(()=> {
                var rect=GameObject.Find(name).GetComponent<RectTransform>();
                Send(RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center)),1);
            });
            Steps.Enqueue(()=>Send(pointer));
            Steps.Enqueue(assertion);
        }
        private static PipeGuideSelectable Find(string id) => UnityEngine.Object.FindObjectsByType<PipeGuideSelectable>(FindObjectsSortMode.None).Single(x => x.ObjectId == id);
        private static Vector2 Center() => new Vector2(Screen.width * .48f, Screen.height * .48f);
        private static void Send(Vector2 position, int buttons = 0, Vector2 delta = default, float scroll = 0)
        {
            pointer = position;
            pendingState = new MouseState { position = position, delta = delta, scroll = new Vector2(0, scroll), buttons = (ushort)buttons };
            hasPendingInput = true;
        }
        private static void FlushPendingInput()
        {
            // Editor updates have a separate input buffer. Deliver to the running player,
            // so a preceding Editor input update cannot consume the synthetic click.
            if (!hasPendingInput || InputState.currentUpdateType != InputUpdateType.Dynamic) return;
            InputSystem.QueueStateEvent(mouse, pendingState);
            hasPendingInput = false;
        }
        private static Vector2 VisiblePoint(PipeGuideSelectable item)
        {
            Physics.SyncTransforms();
            foreach (var renderer in item.GetComponentsInChildren<Renderer>())
            {
                var b = renderer.bounds;
                for (int axis = 0; axis < 3; axis++)
                    for (int k = -4; k <= 4; k++)
                    {
                        var world = b.center; world[axis] += b.extents[axis] * k / 5f;
                        var point = camera.WorldToScreenPoint(world);
                        if (point.z <= 0 || !camera.pixelRect.Contains(point) || ViewerPointer.IsOverUI(point)) continue;
                        if (Physics.Raycast(camera.ScreenPointToRay(point), out var hit) && hit.collider.GetComponentInParent<PipeGuideSelectable>() == item) return point;
                    }
            }
            throw new InvalidOperationException("No visible clickable point for " + item.ObjectId);
        }
        private static Vector2 EmptyPoint()
        {
            for (float y = .8f; y > .2f; y -= .1f)
                for (float x = .25f; x < .75f; x += .1f)
                {
                    var p = new Vector2(Screen.width * x, Screen.height * y);
                    if (ViewerPointer.IsOverUI(p)) continue;
                    if (!Physics.Raycast(camera.ScreenPointToRay(p), out var h) || h.collider.GetComponentInParent<PipeGuideSelectable>() == null) return p;
                }
            throw new InvalidOperationException("No empty viewport point.");
        }
        private static bool HighlightMaterial(PipeGuideSelectable item) => item.GetComponentsInChildren<Renderer>().All(r => r.sharedMaterials.All(m => m.name == "SelectionCyan"));
        private static void AssertSelected(PipeGuideSelectable item)
        {
            Check(item.ObjectId + " selection", selection.Current == item);
            Check(item.ObjectId + " inspector update", inspector.text.Contains(item.ObjectId));
        }
        private static void Check(string name, bool pass)
        {
            Results.Add(name + ": " + (pass ? "PASS" : "FAIL"));
            if (!pass) throw new InvalidOperationException(name + " failed.");
        }
        private static void Capture(string path) => ScreenCapture.CaptureScreenshot(Path.GetFullPath(path));
        private static void Finish(string error)
        {
            var report = new Report { passed = error == null, error = error ?? "", checks = Results.ToArray(), resolution = Screen.width + "x" + Screen.height };
            File.WriteAllText(ReportPath, JsonUtility.ToJson(report, true));
            SessionState.SetBool(Key, false);
            InputSystem.onBeforeUpdate -= FlushPendingInput;
            hasPendingInput = false;
            if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
            InputSystem.settings.backgroundBehavior = previousBackground;
            Application.runInBackground = previousRunInBackground;
            initialized = false;
            EditorApplication.isPlaying = false;
        }
        [Serializable] private class Report { public bool passed; public string error; public string[] checks; public string resolution; }
    }
}




