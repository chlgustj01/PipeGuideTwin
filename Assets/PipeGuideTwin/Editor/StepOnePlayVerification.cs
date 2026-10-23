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
    public static class StepOnePlayVerification
    {
        private const string Key = "PipeGuideTwin.StepOneVerify";
        public const string ReportPath = "Temp/PipeGuideTwin-Step1-Tests.json";
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

        static StepOnePlayVerification() => EditorApplication.update += Tick;
        [MenuItem("PipeGuide Twin/Verify STEP 1 in Play Mode")]
        public static void Start()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Start verification from Edit Mode.");
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save the scene first.");
            if (File.Exists(ReportPath)) File.Delete(ReportPath);
            SessionState.SetBool(Key, true);
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
            var pipe1 = Find("PIPE_001"); var pipe2 = Find("PIPE_002"); var pump = Find("EQ_PUMP_001");
            Steps.Enqueue(() => Send(VisiblePoint(pipe1), 1));
            Steps.Enqueue(() => { AssertSelected(pipe1); Check("PIPE_001 highlight", pipe1.GetComponent<SelectionHighlight>().IsSelected && HighlightMaterial(pipe1)); Check("Pipe inspector properties", inspector.text.Contains("200 mm") && inspector.text.Contains("탄소강")); Capture("Temp/PipeGuideTwin-Step1-Selected.png"); Send(pointer); });
            Steps.Enqueue(() => Send(VisiblePoint(pipe2), 1));
            Steps.Enqueue(() => { AssertSelected(pipe2); Check("Previous pipe highlight restored", !pipe1.GetComponent<SelectionHighlight>().IsSelected && pipe1.GetComponentInChildren<Renderer>().sharedMaterial.name == "DemoPipe"); Check("PIPE_002 highlight", HighlightMaterial(pipe2)); Send(pointer); });
            Steps.Enqueue(() => Send(VisiblePoint(pump), 1));
            Steps.Enqueue(() => { AssertSelected(pump); Check("Equipment highlight", HighlightMaterial(pump)); Check("Equipment inspector", inspector.text.Contains("장비") && inspector.text.Contains("냉각수 펌프")); Send(pointer); });
            Steps.Enqueue(() => Send(EmptyPoint(), 1));
            Steps.Enqueue(() => { Check("Empty-space deselect", selection.Current == null && !pump.GetComponent<SelectionHighlight>().IsSelected); Check("Inspector reset", inspector.text.Contains("객체 선택")); Send(pointer); });
            Steps.Enqueue(() => Send(VisiblePoint(pipe1), 1));
            Steps.Enqueue(() => { AssertSelected(pipe1); Send(pointer); });
            Steps.Enqueue(() => Send(new Vector2(Screen.width * .08f, Screen.height * .65f), 1));
            Steps.Enqueue(() => { Check("Navigation UI blocks selection", selection.Current == pipe1 && ViewerPointer.IsOverUI(pointer)); Send(pointer); });
            Steps.Enqueue(() => Send(new Vector2(Screen.width * .9f, Screen.height * .65f), 1));
            Steps.Enqueue(() => { Check("Inspector UI blocks selection", selection.Current == pipe1 && ViewerPointer.IsOverUI(pointer)); Send(pointer); });
            Steps.Enqueue(() => { oldRotation = camera.transform.rotation; Send(Center(), 2); });
            Steps.Enqueue(() => Send(Center() + new Vector2(60, 25), 2, new Vector2(60, 25)));
            Steps.Enqueue(() => { Check("Orbit", Quaternion.Angle(oldRotation, camera.transform.rotation) > 1); Send(pointer); });
            Steps.Enqueue(() => { oldPivot = controller.Pivot; Send(Center(), 4); });
            Steps.Enqueue(() => Send(Center() + new Vector2(40, 20), 4, new Vector2(40, 20)));
            Steps.Enqueue(() => { Check("Pan", Vector3.Distance(oldPivot, controller.Pivot) > .01f); Send(pointer); });
            Steps.Enqueue(() => { oldDistance = controller.Distance; Send(Center(), 0, Vector2.zero, 120); });
            Steps.Enqueue(() => { Check("Zoom", controller.Distance < oldDistance); Send(pointer); });
            Steps.Enqueue(() => { oldRotation = camera.transform.rotation; oldPivot = controller.Pivot; oldDistance = controller.Distance; Send(new Vector2(Screen.width * .08f, Screen.height * .6f), 2); });
            Steps.Enqueue(() => Send(pointer + new Vector2(15, 15), 2, new Vector2(15, 15), 120));
            Steps.Enqueue(() => { Check("UI blocks camera orbit and zoom", Quaternion.Angle(oldRotation, camera.transform.rotation) < .001f && Vector3.Distance(oldPivot, controller.Pivot) < .001f && Mathf.Abs(oldDistance - controller.Distance) < .001f); Send(pointer); });
            Steps.Enqueue(() => { camera.transform.SetPositionAndRotation(initialPosition, initialRotation); controller.SetPivot(initialPivot); Send(EmptyPoint(), 1); });
            Steps.Enqueue(() => { Check("Final deselection", selection.Current == null); Send(pointer); Capture("Temp/PipeGuideTwin-Step1-Viewer.png"); });
        }
        private static PipeGuideSelectable Find(string id) => UnityEngine.Object.FindObjectsByType<PipeGuideSelectable>(FindObjectsSortMode.None).Single(x => x.ObjectId == id);
        private static Vector2 Center() => new Vector2(Screen.width * .48f, Screen.height * .48f);
        private static void Send(Vector2 position, int buttons = 0, Vector2 delta = default, float scroll = 0)
        {
            pointer = position;
            InputSystem.QueueStateEvent(mouse, new MouseState { position = position, delta = delta, scroll = new Vector2(0, scroll), buttons = (ushort)buttons });
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
            if (mouse != null && mouse.added) InputSystem.RemoveDevice(mouse);
            InputSystem.settings.backgroundBehavior = previousBackground;
            Application.runInBackground = previousRunInBackground;
            initialized = false;
            EditorApplication.isPlaying = false;
        }
        [Serializable] private class Report { public bool passed; public string error; public string[] checks; public string resolution; }
    }
}
