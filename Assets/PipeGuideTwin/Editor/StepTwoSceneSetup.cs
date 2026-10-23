using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using PipeGuideTwin.Core;
using PipeGuideTwin.Data;
using PipeGuideTwin.Interaction;
using PipeGuideTwin.UI;
using PipeGuideTwin.Validation;
using PipeGuideTwin.Visualization;

namespace PipeGuideTwin.Editor
{
    public static class StepTwoSceneSetup
    {
        [MenuItem("PipeGuide Twin/Build STEP 2 Validation View")]
        public static void Build()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.path != StepZeroSceneBuilder.ScenePath || scene.isDirty || EditorApplication.isPlaying)
                throw new InvalidOperationException("Open the saved main scene in Edit Mode.");
            if(UnityEngine.Object.FindFirstObjectByType<ValidationResultController>() != null)
                throw new InvalidOperationException("STEP 2 already exists; refusing to overwrite.");
            var app=UnityEngine.Object.FindFirstObjectByType<PipeGuideAppManager>();
            var selection=UnityEngine.Object.FindFirstObjectByType<SelectionManager>();
            var host=new GameObject("ValidationResults"); host.transform.SetParent(GameObject.Find("_SYSTEM").transform,false);
            var results=host.AddComponent<ValidationResultController>();
            Refs(results,("selection",selection));
            var markerRoot=new GameObject("ValidationVisualizationRoot"); markerRoot.transform.SetParent(GameObject.Find("_WORLD").transform,false);
            var visual=host.AddComponent<ValidationVisualController>();
            var cyan=AssetDatabase.LoadAssetAtPath<Material>("Assets/PipeGuideTwin/Materials/SelectionCyan.mat");
            cyan.shader=Shader.Find("Universal Render Pipeline/Unlit");
            cyan.SetColor("_BaseColor",new Color(.2f,.92f,1));
            EditorUtility.SetDirty(cyan);
            Refs(visual,("app",app),("results",results),("markerRoot",markerRoot.transform),
                ("warningMaterial",Mat("ValidationWarning",new Color32(247,231,83,255))),
                ("mismatchMaterial",Mat("ValidationMismatch",new Color32(230,170,32,255))),
                ("duplicateMaterial",Mat("ValidationDuplicate",new Color32(242,98,26,255))),
                ("missingMaterial",Mat("ValidationMissing",new Color32(237,55,68,255))),
                ("selectionMaterial",cyan));
            var canvas=GameObject.Find("MainCanvas");
            var main=canvas.transform.Find("MainContent");
            var panel=Box("IssuePanel",main,new Vector2(12,-85),new Vector2(276,660),new Color32(24,36,55,245));
            Label("Heading",panel,"정합성 문제",23,new Vector2(16,-16),new Vector2(245,38));
            var summary=Label("Summary",panel,"검사 결과 준비 중",17,new Vector2(16,-66),new Vector2(245,125));
            var list=new GameObject("IssueList",typeof(RectTransform));
            var listRect=(RectTransform)list.transform; listRect.SetParent(panel,false);
            listRect.anchorMin=listRect.anchorMax=listRect.pivot=new Vector2(0,1);
            listRect.anchoredPosition=new Vector2(12,-205); listRect.sizeDelta=new Vector2(252,440);
            var row=Box("IssueRowTemplate",listRect,Vector2.zero,new Vector2(252,88),new Color32(42,57,78,255));
            var button=row.gameObject.AddComponent<Button>();
            Label("Label",row,"",19,new Vector2(12,-10),new Vector2(230,70));
            row.gameObject.SetActive(false);
            var panelUI=panel.gameObject.AddComponent<ValidationPanelUI>();
            Refs(panelUI,("results",results),("summary",summary),("listRoot",listRect),("rowTemplate",button));
            var nav=canvas.transform.Find("NavigationPanel");
            var model=nav.Find("Model3D").gameObject.AddComponent<Button>();
            var validation=nav.Find("ConsistencyCheck").gameObject.AddComponent<Button>();
            var workflow=canvas.AddComponent<WorkflowUIController>();
            Refs(workflow,("app",app),("issuePanel",panel.gameObject),("stageLabel",main.Find("Stage").GetComponent<Text>()),("modelButton",model),("validationButton",validation));
            var so=new SerializedObject(workflow);
            var items=canvas.GetComponentsInChildren<WorkflowPlaceholder>(true);
            var array=so.FindProperty("navigation"); array.arraySize=items.Length;
            for(int i=0;i<items.Length;i++) array.GetArrayElementAtIndex(i).objectReferenceValue=items[i];
            so.FindProperty("idleColor").colorValue=new Color32(24,36,55,255);
            so.FindProperty("activeColor").colorValue=new Color32(65,48,40,255);
            so.FindProperty("accentColor").colorValue=new Color32(245,146,56,255);
            so.ApplyModifiedPropertiesWithoutUndo();
            Refs(canvas.GetComponentInChildren<SelectionInspectorUI>(),("app",app),("validation",results));
            panel.gameObject.SetActive(false);
            markerRoot.SetActive(false);
            var font=canvas.GetComponent<ShellFont>(); font.enabled=false; font.enabled=true;
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        }
        private static Material Mat(string name,Color color)
        {
            var material=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};
            material.SetColor("_BaseColor",color); material.SetFloat("_Smoothness",.3f);
            AssetDatabase.CreateAsset(material,"Assets/PipeGuideTwin/Materials/"+name+".mat");
            return material;
        }
        private static RectTransform Box(string name,Transform parent,Vector2 position,Vector2 size,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));
            var r=(RectTransform)go.transform; r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1); r.anchoredPosition=position; r.sizeDelta=size;
            go.GetComponent<Image>().color=color; return r;
        }
        private static Text Label(string name,Transform parent,string value,int size,Vector2 position,Vector2 dimensions)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Text));
            var r=(RectTransform)go.transform; r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);
            r.anchoredPosition=position;r.sizeDelta=dimensions;
            var t=go.GetComponent<Text>();t.text=value;t.fontSize=size;t.color=Color.white;t.raycastTarget=false;
            t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");return t;
        }
        private static void Refs(UnityEngine.Object target,params (string key,UnityEngine.Object value)[] pairs)
        {
            var so=new SerializedObject(target);
            foreach(var pair in pairs)so.FindProperty(pair.key).objectReferenceValue=pair.value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
