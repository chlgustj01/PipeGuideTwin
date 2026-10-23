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
    public static class StepTwoPlayVerification
    {
        private const string Key = "PipeGuideTwin.StepTwoVerify";
        public const string ReportPath = "Temp/PipeGuideTwin-Step2-Tests.json";
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

        static StepTwoPlayVerification() => EditorApplication.update += Tick;
        [MenuItem("PipeGuide Twin/Verify STEP 2 in Play Mode")]
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
            var data=UnityEngine.Object.FindFirstObjectByType<PipeGuideTwin.Validation.ValidationResultController>();
            var visual=UnityEngine.Object.FindFirstObjectByType<ValidationVisualController>();
            var pipe1=Find("PIPE_001"); var pipe2=Find("PIPE_002"); var pipe3=Find("PIPE_003"); var pipe4=Find("PIPE_004");
            Check("Initial Model3D stage",app.CurrentStage == PipeGuideTwin.Data.WorkflowStage.Model3D);
            // Give the virtual mouse a neutral frame before its first click.
            Steps.Enqueue(()=>Send(Center()));
            Click("ConsistencyCheck",()=> {
                Check("ConsistencyCheck stage",app.CurrentStage == PipeGuideTwin.Data.WorkflowStage.ConsistencyCheck);
                Check("Issue list visible",GameObject.Find("IssuePanel") != null);
                Check("Five reported results / four problems",data.Issues.Count == 5 && data.ProblemCount == 4 && data.HighCount == 2);
                Check("Four issue buttons, Match excluded",UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Count(b=>b.name.StartsWith("Issue_VAL_")) == 4 && GameObject.Find("Issue_VAL_001") == null);
                Check("Summary derived from results",GameObject.Find("Summary").GetComponent<Text>().text.Contains("검사 대상: 5"));
                Check("Warning visual",MaterialName(pipe2) == "ValidationWarning");
                Check("Mismatch visual",MaterialName(pipe3) == "ValidationMismatch");
                Check("Duplicate visual",MaterialName(pipe4) == "ValidationDuplicate");
                Check("Match retains base",MaterialName(pipe1) == "DemoPipe");
                Check("Missing marker",visual.MarkerFor("VAL_005") != null && visual.MarkerFor("VAL_005").gameObject.activeInHierarchy);
                Capture("Temp/PipeGuideTwin-Step2-Overview.png");
            });
            Click("Issue_VAL_003",()=> {
                AssertSelected(pipe3);
                Check("Mismatch inspector values",inspector.text.Contains("200 mm") && inspector.text.Contains("150 mm") && inspector.text.Contains("불일치") && inspector.text.Contains("높음"));
                Check("Selection preserves mismatch surface",MaterialName(pipe3) == "ValidationMismatch" && pipe3.GetComponent<SelectionHighlight>().IsSelected && pipe3.GetComponentInChildren<LineRenderer>().enabled);
                Capture("Temp/PipeGuideTwin-Step2-Mismatch.png");
            });
            Click("Issue_VAL_004",()=> {
                AssertSelected(pipe4);
                Check("Duplicate inspector",inspector.text.Contains("중복") && inspector.text.Contains("배관 1개") && inspector.text.Contains("배관 2개"));
                Check("Deselected mismatch remains amber",MaterialName(pipe3) == "ValidationMismatch" && !pipe3.GetComponentInChildren<LineRenderer>().enabled);
                Check("Selected duplicate remains orange",MaterialName(pipe4) == "ValidationDuplicate");
            });
            Click("Issue_VAL_002",()=> {
                AssertSelected(pipe2);
                Check("Warning inspector",inspector.text.Contains("확인 필요") && inspector.text.Contains("탄소강") && inspector.text.Contains("스테인리스강"));
                Check("Selected warning remains yellow",MaterialName(pipe2) == "ValidationWarning");
            });
            Click("Issue_VAL_005",()=> {
                Check("No fabricated PIPE_005",GameObject.Find("PIPE_005") == null && !UnityEngine.Object.FindObjectsByType<PipeGuideSelectable>(FindObjectsSortMode.None).Any(x=>x.ObjectId == "PIPE_005"));
                Check("Missing selects issue without pipe",selection.Current == null && data.SelectedIssue.IssueId == "VAL_005");
                Check("Missing marker highlighted",visual.MarkerFor("VAL_005").IsSelected && visual.MarkerFor("VAL_005").GetComponentInChildren<LineRenderer>().enabled);
                Check("Missing inspector",inspector.text.Contains("누락") && inspector.text.Contains("존재") && inspector.text.Contains("없음"));
                Check("Missing position converted from mm",Vector3.Distance(visual.MarkerFor("VAL_005").transform.position,new Vector3(-1.8f,1.7f,-2.4f)) < .001f);
                Capture("Temp/PipeGuideTwin-Step2-Missing.png");
            });
            Steps.Enqueue(()=>Send(VisiblePoint(pipe3),1));
            Steps.Enqueue(()=>Send(pointer));
            Steps.Enqueue(()=> {
                AssertSelected(pipe3);
                Check("3D click resolves reported issue",data.SelectedIssue.IssueId == "VAL_003" && inspector.text.Contains("불일치"));
                Check("Old marker deselected",!visual.MarkerFor("VAL_005").IsSelected);
            });
            Steps.Enqueue(()=>Send(EmptyPoint(),1));
            Steps.Enqueue(()=>Send(pointer));
            Steps.Enqueue(()=> {
                Check("Empty deselect retains validation",selection.Current == null && data.SelectedIssue == null && MaterialName(pipe3) == "ValidationMismatch");
                Check("Selection outline removed",!pipe3.GetComponentInChildren<LineRenderer>().enabled);
            });
            Steps.Enqueue(()=>Send(VisiblePoint(pipe1),1));
            Steps.Enqueue(()=>Send(pointer));
            Steps.Enqueue(()=>Check("Match inspector",inspector.text.Contains("정상") && MaterialName(pipe1) == "DemoPipe"));
            Click("Model3D",()=> {
                Check("Model3D return",app.CurrentStage == PipeGuideTwin.Data.WorkflowStage.Model3D && GameObject.Find("IssuePanel") == null);
                Check("Normal object inspector restored",inspector.text.Contains("선택한 객체") && !inspector.text.Contains("설계값"));
                Check("Validation data retained",data.Issues.Count == 5 && data.ProblemCount == 4);
                Check("Base pipe material restored",MaterialName(pipe3) == "DemoPipe");
            });
            Steps.Enqueue(()=> {oldRotation=camera.transform.rotation; Send(Center(),2);});
            Steps.Enqueue(()=>Send(Center()+new Vector2(60,25),2,new Vector2(60,25)));
            Steps.Enqueue(()=> {Check("Orbit regression",Quaternion.Angle(oldRotation,camera.transform.rotation)>1);Send(pointer);});
            Steps.Enqueue(()=> {oldPivot=controller.Pivot;Send(Center(),4);});
            Steps.Enqueue(()=>Send(Center()+new Vector2(40,20),4,new Vector2(40,20)));
            Steps.Enqueue(()=> {Check("Pan regression",Vector3.Distance(oldPivot,controller.Pivot)>.01f);Send(pointer);});
            Steps.Enqueue(()=> {oldDistance=controller.Distance;Send(Center(),0,Vector2.zero,120);});
            Steps.Enqueue(()=> {Check("Zoom regression",controller.Distance<oldDistance);Send(pointer);camera.transform.SetPositionAndRotation(initialPosition,initialRotation);controller.SetPivot(initialPivot);});
            Click("ConsistencyCheck",()=>Check("Validation state restored after return",MaterialName(pipe3) == "ValidationMismatch" && visual.MarkerFor("VAL_005") != null));
            Steps.Enqueue(()=> {
                var replacement=PipeGuideTwin.Data.Mock.MockValidationDataProvider.GetDemoIssues();
                replacement[0].Status=PipeGuideTwin.Data.ValidationStatus.Warning;
                replacement[0].Title="교체된 데모 결과";
                replacement[1].Status=PipeGuideTwin.Data.ValidationStatus.Match;
                data.ReplaceResults(replacement);
            });
            Steps.Enqueue(()=> {
                Check("Replacement result drives visuals",MaterialName(pipe1) == "ValidationWarning" && MaterialName(pipe2) == "DemoPipe");
                Check("Replacement result drives list",GameObject.Find("Issue_VAL_001") != null && GameObject.Find("Issue_VAL_002") == null);
                data.ReplaceResults(PipeGuideTwin.Data.Mock.MockValidationDataProvider.GetDemoIssues());
            });
            Click("Issue_VAL_003",()=>Capture("Temp/PipeGuideTwin-Step2-Mismatch.png"));
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


