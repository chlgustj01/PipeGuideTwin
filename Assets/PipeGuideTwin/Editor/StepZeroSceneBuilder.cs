using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using PipeGuideTwin.Core;
using PipeGuideTwin.Data;
using PipeGuideTwin.UI;

namespace PipeGuideTwin.Editor
{
    public static class StepZeroSceneBuilder
    {
        public const string ScenePath = "Assets/PipeGuideTwin/Scenes/PipeGuide_Main.unity";
        private static readonly Color Navy = new Color32(15, 23, 38, 255);
        private static readonly Color Panel = new Color32(24, 36, 55, 255);
        private static readonly Color Orange = new Color32(245, 146, 56, 255);

        [MenuItem("PipeGuide Twin/Build STEP 0 Scene")]
        public static void Build()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
                throw new InvalidOperationException("Main scene already exists; refusing to overwrite.");
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save existing scene changes before building.");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
            var system = Node("_SYSTEM");
            var manager = Node("AppManager", system).AddComponent<PipeGuideAppManager>();
            var context = Node("ProjectContext", system).AddComponent<PipeGuideProjectContext>();
            var serialized = new SerializedObject(manager);
            serialized.FindProperty("projectContext").objectReferenceValue = context;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Node("DebugRoot", system);
            var world = Node("_WORLD");
            foreach (var name in new[] { "ShipModelRoot", "PipeModelRoot", "EquipmentRoot", "StructureRoot", "RoutingVisualizationRoot" }) Node(name, world);
            var cameraRoot = Node("_CAMERA");
            var lighting = Node("_LIGHTING");
            // Clone the supplied URP defaults, preserving SampleScene and its volume profile.
            var sample = UnityEngine.SceneManagement.SceneManager.GetSceneByPath("Assets/Scenes/SampleScene.unity");
            bool opened = !sample.IsValid() || !sample.isLoaded;
            if (opened) sample = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Additive);
            foreach (var original in sample.GetRootGameObjects())
            {
                if (original.name != "Main Camera" && original.name != "Directional Light" && original.name != "Global Volume") continue;
                var clone = UnityEngine.Object.Instantiate(original);
                clone.name = original.name;
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(clone, scene);
                clone.transform.SetParent(original.name == "Main Camera" ? cameraRoot.transform : lighting.transform, false);
            }
            var camera = cameraRoot.GetComponentInChildren<Camera>();
            if (camera == null) throw new InvalidOperationException("SampleScene camera missing.");
            camera.transform.position = new Vector3(8, 6, -10);
            camera.transform.LookAt(Vector3.zero);
            var ui = Node("_UI");
            var canvasGo = new GameObject("MainCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(ui.transform, false);
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            var top = Box("TopBar", canvasGo.transform, 0, 0.91f, 1, 1, Panel);
            Label("Brand", top, "PIPEGUIDE TWIN", 28, Orange, 28, -18, 420, 52);
            Label("ProjectInfo", top, "DEMO VESSEL   /   ENGINE ROOM   /   REV.A", 20, Color.white, 750, -22, 950, 48);
            var nav = Box("NavigationPanel", canvasGo.transform, 0, 0.055f, 0.16f, 0.91f, Panel);
            string[] labels = { "프로젝트", "데이터 불러오기", "3D 모델", "정합성 검사", "경로 추천", "국소 재설계", "결과 비교", "승인 / 이력" };
            for (int i = 0; i < labels.Length; i++)
            {
                var item = Box(((WorkflowStage)i).ToString(), nav, 0.05f, 0.89f - i * 0.083f, 0.95f, 0.96f - i * 0.083f, i == 0 ? new Color32(65, 48, 40, 255) : Panel);
                item.gameObject.AddComponent<WorkflowPlaceholder>().Stage = (WorkflowStage)i;
                Label("Label", item, labels[i], 23, i == 0 ? Orange : Color.white, 16, -5, 245, 50);
            }
            var main = Box("MainContent", canvasGo.transform, 0.165f, 0.06f, 0.795f, 0.905f, Navy);
            Label("Stage", main, "PROJECT", 21, Orange, 45, -35, 800, 40);
            Label("Title", main, "PipeGuide Twin", 56, Color.white, 45, -160, 1040, 90);
            Label("Description", main, "Ship Piping Design Validation\n& Local Rerouting Digital Twin", 30, new Color32(173, 189, 210, 255), 48, -265, 1040, 110);
            Label("Status", main, "READY", 24, Orange, 48, -420, 800, 50);
            Box("InspectorPanel", canvasGo.transform, 0.80f, 0.06f, 1, 0.905f, Panel);
            var status = Box("StatusBar", canvasGo.transform, 0, 0, 1, 0.05f, Panel);
            Label("Status", status, "READY     |     Data Source: MOCK     |     Engineering: mm     /     Unity: m", 18, Color.white, 26, -3, 1750, 46);
            var debug = Box("DebugPanel", canvasGo.transform, 0.80f, 0.06f, 1, 0.31f, new Color32(31, 45, 66, 255));
            Label("DebugInfo", debug, "PIPEGUIDE TWIN\n\nProject: DEMO\nStage: Project\nData Source: MOCK\nStatus: READY", 20, Color.white, 20, -12, 345, 245);
            canvasGo.AddComponent<ShellFont>();
            var events = Node("EventSystem", ui);
            events.AddComponent<EventSystem>();
            events.AddComponent<InputSystemUIInputModule>();
            if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException("Scene save failed.");
            AssetDatabase.SaveAssets();
            // Only the newly saved main scene remains open; the original asset stays untouched.
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        private static GameObject Node(string name, GameObject parent = null)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent.transform, false);
            return go;
        }
        private static RectTransform Box(string name, Transform parent, float x0, float y0, float x1, float y1, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return rect;
        }
        private static void Label(string name, Transform parent, string value, int size, Color color, float x, float y, float width, float height)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, y); rect.sizeDelta = new Vector2(width, height);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value; text.fontSize = size; text.color = color; text.raycastTarget = false;
        }
    }
}
