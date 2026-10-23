using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using PipeGuideTwin.Core;
using PipeGuideTwin.Data;
using PipeGuideTwin.Interaction;
using PipeGuideTwin.Visualization;
using PipeGuideTwin.UI;

namespace PipeGuideTwin.Editor
{
    public static class StepOneSceneSetup
    {
        [MenuItem("PipeGuide Twin/Build STEP 1 Demo")]
        public static void Build()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != StepZeroSceneBuilder.ScenePath || scene.isDirty)
                throw new InvalidOperationException("Open the saved main scene before STEP 1 setup.");
            var equipment = GameObject.Find("EquipmentRoot").transform;
            var pipes = GameObject.Find("PipeModelRoot").transform;
            var structures = GameObject.Find("StructureRoot").transform;
            if (equipment.childCount != 0 || pipes.childCount != 0 || structures.childCount != 0)
                throw new InvalidOperationException("World roots are populated; refusing to overwrite existing work.");

            var pipeMaterial = MakeMaterial("DemoPipe", new Color32(53, 174, 170, 255), 0.2f);
            var pumpMaterial = MakeMaterial("DemoPump", new Color32(74, 128, 189, 255), 0.25f);
            var tankMaterial = MakeMaterial("DemoTank", new Color32(146, 167, 182, 255), 0.35f);
            var heaterMaterial = MakeMaterial("DemoHeater", new Color32(142, 121, 164, 255), 0.25f);
            var floorMaterial = MakeMaterial("DemoFloor", new Color32(99, 113, 129, 255), 0);
            var wallMaterial = MakeMaterial("DemoStructure", new Color32(137, 152, 168, 255), 0);
            var selectionMaterial = MakeMaterial("SelectionCyan", new Color32(63, 225, 255, 255), 0.1f);
            selectionMaterial.EnableKeyword("_EMISSION");
            selectionMaterial.SetColor("_EmissionColor", new Color(0.05f, 0.45f, 0.65f));

            Equipment("EQ_PUMP_001", "냉각수 펌프", equipment, PrimitiveType.Cube, new Vector3(-3, .75f, 1), new Vector3(1.6f, 1.5f, 1.2f), pumpMaterial, selectionMaterial);
            Equipment("EQ_TANK_001", "냉각수 탱크", equipment, PrimitiveType.Cylinder, new Vector3(3, 1.5f, 2), new Vector3(2, 1.5f, 2), tankMaterial, selectionMaterial);
            Equipment("EQ_HEATER_001", "보조 히터", equipment, PrimitiveType.Cube, new Vector3(2, 1, -3), new Vector3(1.5f, 2, 1.1f), heaterMaterial, selectionMaterial);
            Pipe("PIPE_001", "주 냉각수 배관", pipes, 200, pipeMaterial, selectionMaterial,
                new Vector3(-3, .75f, 1), new Vector3(-3, 2.4f, 1), new Vector3(0, 2.4f, 1));
            Pipe("PIPE_002", "탱크 공급 배관", pipes, 200, pipeMaterial, selectionMaterial,
                new Vector3(0, 2.4f, 1), new Vector3(3, 2.4f, 1), new Vector3(3, 2.4f, 2));
            Pipe("PIPE_003", "히터 공급 배관", pipes, 150, pipeMaterial, selectionMaterial,
                new Vector3(2, 1, -3), new Vector3(0, 1, -3), new Vector3(0, 2.4f, -3), new Vector3(0, 2.4f, 1));
            Pipe("PIPE_004", "냉각수 회수 배관", pipes, 150, pipeMaterial, selectionMaterial,
                new Vector3(-3, .5f, 1), new Vector3(-3, .5f, -2), new Vector3(2, .5f, -2), new Vector3(2, .5f, -3));
            Primitive("Floor", structures, PrimitiveType.Cube, new Vector3(0, -.12f, 0), new Vector3(12, .24f, 10), floorMaterial);
            Primitive("Wall_A", structures, PrimitiveType.Cube, new Vector3(0, .45f, 5), new Vector3(12, .9f, .2f), wallMaterial);
            Primitive("Wall_B", structures, PrimitiveType.Cube, new Vector3(-6, .45f, 0), new Vector3(.2f, .9f, 10), wallMaterial);

            var camera = Camera.main;
            camera.transform.position = new Vector3(10, 9, -13);
            camera.transform.LookAt(new Vector3(0, 1, 0));
            camera.fieldOfView = 48;
            camera.nearClipPlane = .05f;
            camera.farClipPlane = 150;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(30, 42, 58, 255);
            camera.gameObject.AddComponent<EngineeringCameraController>();
            var managerGo = new GameObject("SelectionManager");
            managerGo.transform.SetParent(GameObject.Find("_SYSTEM").transform, false);
            var selection = managerGo.AddComponent<SelectionManager>();
            SetReferences(selection, ("viewerCamera", camera));
            var context = UnityEngine.Object.FindFirstObjectByType<PipeGuideProjectContext>();
            var contextSO = new SerializedObject(context);
            contextSO.FindProperty("currentStage").enumValueIndex = (int)WorkflowStage.Model3D;
            contextSO.ApplyModifiedPropertiesWithoutUndo();

            var canvas = GameObject.Find("MainCanvas");
            canvas.transform.Find("TopBar/Brand").GetComponent<Text>().text = "파이프가이드 트윈";
            canvas.transform.Find("TopBar/ProjectInfo").GetComponent<Text>().text = "데모 선박   /   기관실   /   개정 A";
            var main = canvas.transform.Find("MainContent");
            main.GetComponent<Image>().enabled = false;
            foreach (var name in new[] { "Title", "Description", "Status" }) main.Find(name).gameObject.SetActive(false);
            main.Find("Stage").GetComponent<Text>().text = "3D 모델   /   데모 기관실";
            var stageRect = (RectTransform)main.Find("Stage");
            stageRect.anchoredPosition = new Vector2(25, -15);
            stageRect.sizeDelta = new Vector2(1050, 40);
            foreach (var placeholder in canvas.GetComponentsInChildren<WorkflowPlaceholder>())
            {
                var image = placeholder.GetComponent<Image>();
                image.color = placeholder.Stage == WorkflowStage.Model3D ? new Color32(65, 48, 40, 255) : new Color32(24, 36, 55, 255);
                placeholder.GetComponentInChildren<Text>().color = placeholder.Stage == WorkflowStage.Model3D ? new Color32(245, 146, 56, 255) : Color.white;
            }
            var inspector = canvas.transform.Find("InspectorPanel");
            var inspectorText = MakeLabel("SelectionInfo", inspector, "객체 선택\n\n배관 또는 장비를\n선택하세요.", 21, new Vector2(20, -22), new Vector2(345, 620));
            var help = MakeLabel("ViewerControls", main, "오른쪽 드래그: 회전   |   가운데 드래그: 이동   |   휠: 확대/축소   |   왼쪽 클릭: 선택", 18, new Vector2(25, 45), new Vector2(1100, 35));
            var helpRect = (RectTransform)help.transform;
            helpRect.anchorMin = helpRect.anchorMax = new Vector2(0, 0);
            var ui = inspector.gameObject.AddComponent<SelectionInspectorUI>();
            var status = canvas.transform.Find("StatusBar/Status").GetComponent<Text>();
            var debug = canvas.transform.Find("DebugPanel/DebugInfo").GetComponent<Text>();
            SetReferences(ui, ("selection", selection), ("inspectorText", inspectorText), ("statusText", status), ("debugText", debug));
            status.text = "상태: 준비 완료   |   데모 데이터   |   설계 단위: mm / 화면 단위: m";
            debug.text = "파이프가이드 트윈\n\n단계: 3D 모델\n데이터: 데모\n선택: 없음\n상태: 준비 완료";
            // Reapply the installed Korean font to labels created after STEP 0.
            var font = canvas.GetComponent<ShellFont>();
            font.enabled = false; font.enabled = true;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static Material MakeMaterial(string name, Color color, float metal)
        {
            var path = "Assets/PipeGuideTwin/Materials/" + name + ".mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) throw new InvalidOperationException("Material exists: " + path);
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader is unavailable.");
            var material = new Material(shader) { name = name };
            material.SetColor("_BaseColor", color); material.SetFloat("_Metallic", metal); material.SetFloat("_Smoothness", .35f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
        private static GameObject Primitive(string name, Transform parent, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name;
            go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go;
        }
        private static void Selectable(GameObject go, string id, string label, PIDComponentType type, Material selectionMaterial, float diameter = 0)
        {
            var item = go.AddComponent<PipeGuideSelectable>();
            item.ObjectId = id; item.DisplayName = label; item.ObjectType = type;
            item.DiameterMm = diameter; item.Material = type == PIDComponentType.Pipe ? "Carbon Steel" : "";
            var highlight = go.AddComponent<SelectionHighlight>();
            SetReferences(highlight, ("selectionMaterial", selectionMaterial));
        }
        private static void Equipment(string id, string label, Transform parent, PrimitiveType type, Vector3 position, Vector3 size, Material material, Material selectionMaterial)
        {
            var go = Primitive(id, parent, type, position, size, material);
            Selectable(go, id, label, PIDComponentType.Equipment, selectionMaterial);
        }
        private static void Pipe(string id, string label, Transform parent, float diameterMm, Material material, Material selectionMaterial, params Vector3[] points)
        {
            var root = new GameObject(id); root.transform.SetParent(parent, false);
            root.transform.localPosition = points[0];
            float diameter = EngineeringUnitConverter.MmToMeters(diameterMm);
            for (int i = 1; i < points.Length; i++)
            {
                var offset = points[i] - points[i-1];
                var segment = Primitive("Segment_" + i, root.transform, PrimitiveType.Cylinder,
                    (points[i] + points[i-1]) * .5f - points[0], new Vector3(diameter, offset.magnitude * .5f, diameter), material);
                segment.transform.localRotation = Quaternion.FromToRotation(Vector3.up, offset.normalized);
            }
            Selectable(root, id, label, PIDComponentType.Pipe, selectionMaterial, diameterMm);
        }
        private static Text MakeLabel(string name, Transform parent, string value, int size, Vector2 position, Vector2 dimensions)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rect = (RectTransform)go.transform; rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position; rect.sizeDelta = dimensions;
            var text = go.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value; text.fontSize = size; text.color = Color.white; text.raycastTarget = false;
            return text;
        }
        private static void SetReferences(UnityEngine.Object target, params (string name, UnityEngine.Object value)[] fields)
        {
            var serialized = new SerializedObject(target);
            foreach (var field in fields) serialized.FindProperty(field.name).objectReferenceValue = field.value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
