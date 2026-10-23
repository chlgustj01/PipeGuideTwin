using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using PipeGuideTwin.Core;
using PipeGuideTwin.Data;
using PipeGuideTwin.Data.Mock;
using PipeGuideTwin.Interaction;
using PipeGuideTwin.UI;

namespace PipeGuideTwin.Editor
{
    public static class ProductShellSceneSetup
    {
        private static PipeGuideTheme theme;
        private static Font font;
        private static Button fileButton,approveButton,rejectButton;
        private static Text fileMessage,decisionMessage;
        [MenuItem("PipeGuide Twin/Build Product UI Shell")]
        public static void Build()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.path!=StepZeroSceneBuilder.ScenePath || scene.isDirty || EditorApplication.isPlaying)
                throw new InvalidOperationException("Open the saved main scene in Edit Mode.");
            var canvas=GameObject.Find("MainCanvas");
            if(canvas.transform.Find("Pages")!=null)throw new InvalidOperationException("Product Shell already exists; existing pages are preserved.");
            theme=ScriptableObject.CreateInstance<PipeGuideTheme>();
            AssetDatabase.CreateAsset(theme,"Assets/PipeGuideTwin/Materials/PipeGuideTheme.asset");
            var data=ScriptableObject.CreateInstance<ProductShellMockData>();data.Pages=ProductShellDemoContent.Create();
            AssetDatabase.CreateAsset(data,"Assets/PipeGuideTwin/Data/Demo/ProductShellMockData.asset");
            font=canvas.transform.Find("TopBar/Brand").GetComponent<Text>().font;
            var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);
            var top=(RectTransform)canvas.transform.Find("TopBar");Stretch(top,new Vector2(0,.91f),Vector2.one);
            top.GetComponent<Image>().color=theme.Panel;
            var brand=top.Find("Brand").GetComponent<Text>();ConfigureText(brand,"파이프가이드 트윈",25,theme.PrimaryOrange,new Vector2(24,-27),new Vector2(290,48));
            var info=top.Find("ProjectInfo").GetComponent<Text>();ConfigureText(info,"데모 선박 / 기관실\n프로젝트 데모  ·  REV.A",15,theme.TextSecondary,new Vector2(1640,-21),new Vector2(260,60));
            var nav=(RectTransform)canvas.transform.Find("NavigationPanel");nav.SetParent(top,false);
            Stretch(nav,new Vector2(.168f,0),new Vector2(.847f,1));nav.GetComponent<Image>().color=theme.Panel;
            var placeholders=nav.GetComponentsInChildren<WorkflowPlaceholder>();
            var buttons=new Button[8];var indicators=new GameObject[8];
            for(int i=0;i<data.Pages.Length;i++)
            {
                var item=placeholders.Single(x=>x.Stage==data.Pages[i].Stage);
                var rect=(RectTransform)item.transform;Stretch(rect,new Vector2(i/8f,.02f),new Vector2((i+1)/8f,.98f));
                item.GetComponent<Image>().color=theme.Panel;
                var label=item.GetComponentInChildren<Text>();
                Stretch((RectTransform)label.transform,Vector2.zero,Vector2.one);label.text=data.Pages[i].NavigationLabel;label.fontSize=19;label.alignment=TextAnchor.MiddleCenter;label.color=theme.TextSecondary;
                buttons[i]=item.GetComponent<Button>()??item.gameObject.AddComponent<Button>();
                buttons[i].transition=Selectable.Transition.None;
                var indicator=Box("ActiveIndicator",rect,theme.PrimaryOrange);
                Stretch(indicator,new Vector2(.12f,.03f),new Vector2(.88f,.065f));indicator.GetComponent<Image>().raycastTarget=false;
                indicators[i]=indicator.gameObject;
            }
            var inspector=canvas.transform.Find("InspectorPanel");
            var debug=canvas.transform.Find("DebugPanel");
            var issuePanel=canvas.transform.Find("MainContent/IssuePanel");
            var controls=canvas.transform.Find("MainContent/ViewerControls");
            var pages=Rect("Pages",canvas.transform);Stretch(pages,new Vector2(0,.052f),new Vector2(1,.905f));
            var bindings=new WorkflowUIController.PageBinding[data.Pages.Length];
            for(int i=0;i<data.Pages.Length;i++)
            {
                var pageData=data.Pages[i];
                var root=Rect(pageData.PageName,pages);Stretch(root,Vector2.zero,Vector2.one);
                if(!pageData.ShowsWorld)root.gameObject.AddComponent<Image>().color=theme.Background;
                var left=Box("LeftPanel",root,theme.Panel);Stretch(left,new Vector2(.008f,.04f),new Vector2(.158f,.98f));Border(left);
                var center=Rect("CenterWorkspace",root);Stretch(center,new Vector2(.167f,.04f),new Vector2(.802f,.98f));
                var right=Box("RightPanel",root,theme.Panel);Stretch(right,new Vector2(.811f,.04f),new Vector2(.992f,.98f));Border(right);
                Label("PageTitle",center,pageData.Title,30,theme.TextPrimary,new Vector2(20,-14),new Vector2(1150,48));
                Label("PageSubtitle",center,pageData.Subtitle,17,theme.TextSecondary,new Vector2(22,-70),new Vector2(1150,38));
                AddCards(pageData.Left,left,288,false,false,pageData.Stage);
                AddCards(pageData.Center,center,1220,true,pageData.SplitCenter,pageData.Stage);
                AddCards(pageData.Right,right,347,false,false,pageData.Stage);
                var bottom=Rect("BottomStatus",root);Stretch(bottom,new Vector2(.008f,0),new Vector2(.992f,.034f));
                Label("PageNote",bottom,pageData.NavigationLabel+"  /  데모 화면",14,theme.TextSecondary,new Vector2(8,0),new Vector2(1600,30));
                if(pageData.Stage==WorkflowStage.Model3D)
                {
                    inspector.SetParent(right,false);debug.SetParent(right,false);
                    ResizeInspector(inspector,debug);
                    controls.SetParent(center,false);
                    var cr=(RectTransform)controls;cr.anchorMin=cr.anchorMax=new Vector2(0,0);cr.pivot=new Vector2(0,0);cr.anchoredPosition=new Vector2(20,18);cr.sizeDelta=new Vector2(1180,32);controls.GetComponent<Text>().fontSize=16;
                }
                if(pageData.Stage==WorkflowStage.ConsistencyCheck)
                {
                    issuePanel.SetParent(left,false);Stretch((RectTransform)issuePanel,Vector2.zero,Vector2.one);
                    issuePanel.GetComponent<Image>().color=theme.Panel;issuePanel.gameObject.SetActive(true);
                    var template=issuePanel.GetComponentsInChildren<Button>(true).First();template.GetComponent<Image>().color=theme.Card;
                }
                if(pageData.Stage==WorkflowStage.LocalRerouting)Legend(center);
                bindings[i]=new WorkflowUIController.PageBinding {Stage=pageData.Stage,Root=root.gameObject,RightPanel=right,Navigation=buttons[i],ActiveIndicator=indicators[i],ShowsWorld=pageData.ShowsWorld};
                root.gameObject.SetActive(pageData.Stage==WorkflowStage.Project);
                indicators[i].SetActive(pageData.Stage==WorkflowStage.Project);
                labelNav(buttons[i],pageData.Stage==WorkflowStage.Project);
            }
            canvas.transform.Find("MainContent").gameObject.SetActive(false);
            var status=canvas.transform.Find("StatusBar/Status").GetComponent<Text>();
            ConfigureText(status,"프로젝트 | 상태: 준비 완료 | 데모 화면",16,theme.TextPrimary,new Vector2(22,-10),new Vector2(1060,36));
            canvas.transform.Find("StatusBar").GetComponent<Image>().color=theme.Panel;
            Label("SystemMetadata",canvas.transform.Find("StatusBar"),"데이터: 데모 (MOCK)  |  설계: mm · 화면: m  |  REV.A",15,theme.TextSecondary,new Vector2(1120,-10),new Vector2(780,36));
            var workflow=canvas.GetComponent<WorkflowUIController>();
            Refs(workflow,("app",UnityEngine.Object.FindFirstObjectByType<PipeGuideAppManager>()),("theme",theme),("inspectorRoot",inspector),("debugRoot",debug),("status",status),("selection",UnityEngine.Object.FindFirstObjectByType<SelectionManager>()),("cameraController",Camera.main.GetComponent<EngineeringCameraController>()));
            var so=new SerializedObject(workflow);var array=so.FindProperty("pages");array.arraySize=bindings.Length;
            for(int i=0;i<bindings.Length;i++)
            {
                var p=array.GetArrayElementAtIndex(i);var b=bindings[i];
                p.FindPropertyRelative("Stage").enumValueIndex=(int)b.Stage;
                p.FindPropertyRelative("Root").objectReferenceValue=b.Root;p.FindPropertyRelative("RightPanel").objectReferenceValue=b.RightPanel;
                p.FindPropertyRelative("Navigation").objectReferenceValue=b.Navigation;p.FindPropertyRelative("ActiveIndicator").objectReferenceValue=b.ActiveIndicator;
                p.FindPropertyRelative("ShowsWorld").boolValue=b.ShowsWorld;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            var actions=canvas.AddComponent<ProductShellActions>();Refs(actions,("fileButton",fileButton),("fileMessage",fileMessage),("approveButton",approveButton),("rejectButton",rejectButton),("decisionMessage",decisionMessage),("theme",theme));
            var context=UnityEngine.Object.FindFirstObjectByType<PipeGuideProjectContext>();so=new SerializedObject(context);so.FindProperty("currentStage").enumValueIndex=(int)WorkflowStage.Project;so.ApplyModifiedPropertiesWithoutUndo();
            var shellFont=canvas.GetComponent<ShellFont>();shellFont.enabled=false;shellFont.enabled=true;
            ReflowCards();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
        [MenuItem("PipeGuide Twin/Reflow Product Shell Cards")]
        public static void ReflowCards()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Reflow the saved UI in Edit Mode.");
            var pages=GameObject.Find("MainCanvas").transform.Find("Pages");
            Canvas.ForceUpdateCanvases();
            foreach(Transform page in pages)
                foreach(var name in new[]{"LeftPanel","CenterWorkspace","RightPanel"})
                {
                    var column=(RectTransform)page.Find(name);
                    var cards=column.Cast<Transform>().Where(t=>t.name.StartsWith("Card_")).Select(t=>(RectTransform)t).ToArray();
                    bool center=name=="CenterWorkspace";
                    bool split=page.name=="ResultComparePage"&&center;
                    bool dashboard=page.name=="ProjectPage"&&center;
                    float top=center?120:18;
                    float dashboardTop=0;
                    foreach(var card in cards)
                    {
                        int index=Array.IndexOf(cards,card);
                        if(dashboard&&index>0)
                        {
                            float w=(column.rect.width-48)/2;
                            card.sizeDelta=new Vector2(w,card.sizeDelta.y);
                            card.Find("CardTitle").GetComponent<RectTransform>().sizeDelta=new Vector2(w-32,38);
                            card.Find("Badge").GetComponent<RectTransform>().sizeDelta=new Vector2(w-32,26);
                            card.Find("CardBody").GetComponent<RectTransform>().sizeDelta=new Vector2(w-32,500);
                            card.anchoredPosition=new Vector2(14+(index-1)*(w+20),-dashboardTop);
                        }
                        else if(!split)card.anchoredPosition=new Vector2(14,-top);
                        var body=card.Find("CardBody").GetComponent<Text>();
                        float content=body.preferredHeight+14;
                        float extra=card.Find("ChooseExcel")!=null?110:card.Find("ApproveMock")!=null?120:0;
                        float height=Mathf.Max(split?650:140,85+content+24+extra);
                        card.sizeDelta=new Vector2(card.sizeDelta.x,height);
                        body.rectTransform.sizeDelta=new Vector2(card.sizeDelta.x-32,content);
                        if(card.Find("ChooseExcel")!=null)
                        {
                            card.Find("ChooseExcel").GetComponent<RectTransform>().anchoredPosition=new Vector2(16,-height+95);
                            card.Find("FileMessage").GetComponent<RectTransform>().anchoredPosition=new Vector2(16,-height+43);
                        }
                        if(card.Find("ApproveMock")!=null)
                        {
                            card.Find("ApproveMock").GetComponent<RectTransform>().anchoredPosition=new Vector2(16,-height+105);
                            card.Find("RejectMock").GetComponent<RectTransform>().anchoredPosition=new Vector2(160,-height+105);
                            card.Find("DecisionMessage").GetComponent<RectTransform>().anchoredPosition=new Vector2(16,-height+55);
                        }
                        if(dashboard&&index==0)dashboardTop=top+height+14;
                        if(!split)top+=height+14;
                    }
                }
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
        private static void labelNav(Button button,bool active)=>button.GetComponentInChildren<Text>().color=active?theme.PrimaryOrange:theme.TextSecondary;
        private static void AddCards(ShellCardData[] cards,RectTransform parent,float width,bool center,bool split,WorkflowStage stage)
        {
            float y=center?120:18;
            for(int i=0;i<cards.Length;i++)
            {
                float cardWidth=split?(width-60)/2: center&& (stage==WorkflowStage.InitialRouting||stage==WorkflowStage.LocalRerouting)?360:width-28;
                float bodyFont=center?21:18;
                float height=85+cards[i].Body.Split('\n').Sum(line=>Mathf.Max(1,Mathf.CeilToInt(line.Length/(cardWidth/bodyFont))))*bodyFont*1.23f;
                if(stage==WorkflowStage.DataImport&&center&&i==0)height+=110;
                if(stage==WorkflowStage.ApprovalHistory&&!center&&parent.name=="RightPanel"&&i==0)height+=120;
                if(split)height=650;
                var card=Box("Card_"+i,parent,theme.Card);Position(card,new Vector2(14+(split?i*(cardWidth+20):0),-y),new Vector2(cardWidth,height));Border(card);
                Label("CardTitle",card,cards[i].Title,center?24:21,theme.TextPrimary,new Vector2(16,-15),new Vector2(cardWidth-32,38));
                Label("Badge",card,cards[i].Badge,14,theme.PrimaryOrange,new Vector2(16,-53),new Vector2(cardWidth-32,26));
                var body=Label("CardBody",card,cards[i].Body,(int)bodyFont,theme.TextSecondary,new Vector2(16,-85),new Vector2(cardWidth-32,height-92));body.lineSpacing=1.1f;
                if(stage==WorkflowStage.DataImport&&center&&i==0)
                {
                    fileButton=ActionButton("ChooseExcel",card,"Excel 파일 선택",new Vector2(16,-height+95),new Vector2(245,44),theme.PrimaryOrange);
                    fileMessage=Label("FileMessage",card,"UI 데모 · 파일 로더 미연결",16,theme.TextSecondary,new Vector2(16,-height+43),new Vector2(cardWidth-32,36));
                }
                if(stage==WorkflowStage.ApprovalHistory&&parent.name=="RightPanel"&&i==0)
                {
                    approveButton=ActionButton("ApproveMock",card,"승인",new Vector2(16,-height+105),new Vector2(132,42),theme.SuccessGreen);
                    rejectButton=ActionButton("RejectMock",card,"반려",new Vector2(160,-height+105),new Vector2(132,42),theme.DangerRed);
                    decisionMessage=Label("DecisionMessage",card,"데모 결정: 검토 대기",16,theme.TextSecondary,new Vector2(16,-height+55),new Vector2(cardWidth-32,52));
                }
                if(!split)y+=height+14;
            }
        }
        private static void ResizeInspector(Transform inspector,Transform debug)
        {
            Stretch((RectTransform)inspector,new Vector2(0,.27f),Vector2.one);inspector.GetComponent<Image>().color=theme.Panel;
            var info=inspector.GetComponentInChildren<Text>();ConfigureText(info,"객체 선택\n\n배관 또는 장비를 선택하세요.",19,theme.TextPrimary,new Vector2(18,-22),new Vector2(310,590));
            Stretch((RectTransform)debug,Vector2.zero,new Vector2(1,.25f));debug.GetComponent<Image>().color=theme.Card;
            var text=debug.GetComponentInChildren<Text>();text.fontSize=17;text.rectTransform.anchoredPosition=new Vector2(18,-14);text.rectTransform.sizeDelta=new Vector2(310,190);
        }
        private static void Legend(RectTransform center)
        {
            var colors=new[]{theme.DangerRed,theme.SuccessGreen,theme.SelectionCyan,theme.CandidateBlue};var names=new[]{"현재 경로","후보 A","후보 B","후보 C"};
            for(int i=0;i<4;i++)
            {
                var swatch=Box("Legend_"+i,center,colors[i]);swatch.anchorMin=swatch.anchorMax=new Vector2(0,0);swatch.pivot=new Vector2(0,0);swatch.anchoredPosition=new Vector2(20+i*200,24);swatch.sizeDelta=new Vector2(12,12);swatch.GetComponent<Image>().raycastTarget=false;
                var text=Label("LegendLabel_"+i,center,names[i],16,theme.TextSecondary,new Vector2(40+i*200,46),new Vector2(150,30));text.rectTransform.anchorMin=text.rectTransform.anchorMax=new Vector2(0,0);
            }
        }
        private static Button ActionButton(string name,Transform parent,string label,Vector2 position,Vector2 size,Color color)
        {
            var rect=Box(name,parent,color);Position(rect,position,size);var button=rect.gameObject.AddComponent<Button>();button.transition=Selectable.Transition.None;
            var text=Label("Label",rect,label,18,theme.Background,Vector2.zero,size);text.alignment=TextAnchor.MiddleCenter;return button;
        }
        private static RectTransform Rect(string name,Transform parent){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);return(RectTransform)go.transform;}
        private static RectTransform Box(string name,Transform parent,Color color){var r=Rect(name,parent);r.gameObject.AddComponent<Image>().color=color;return r;}
        private static void Stretch(RectTransform r,Vector2 lo,Vector2 hi){r.anchorMin=lo;r.anchorMax=hi;r.offsetMin=r.offsetMax=Vector2.zero;}
        private static void Position(RectTransform r,Vector2 position,Vector2 size){r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=position;r.sizeDelta=size;}
        private static Text Label(string name,Transform parent,string value,int size,Color color,Vector2 position,Vector2 dimensions)
        {var rect=Rect(name,parent);var t=rect.gameObject.AddComponent<Text>();ConfigureText(t,value,size,color,position,dimensions);return t;}
        private static void ConfigureText(Text t,string value,int size,Color color,Vector2 position,Vector2 dimensions)
        {Position(t.rectTransform,position,dimensions);t.text=value;t.fontSize=size;t.color=color;t.font=font;t.raycastTarget=false;}
        private static void Border(RectTransform parent)
        {
            for(int i=0;i<4;i++){var r=Box("Border_"+i,parent,theme.Border);r.GetComponent<Image>().raycastTarget=false;
                if(i<2){Stretch(r,new Vector2(0,i),new Vector2(1,i));r.sizeDelta=new Vector2(0,1);}
                else{Stretch(r,new Vector2(i-2,0),new Vector2(i-2,1));r.sizeDelta=new Vector2(1,0);}}
        }
        private static void Refs(UnityEngine.Object target,params(string key,UnityEngine.Object value)[] pairs)
        {var so=new SerializedObject(target);foreach(var p in pairs)so.FindProperty(p.key).objectReferenceValue=p.value;so.ApplyModifiedPropertiesWithoutUndo();}
    }
}
