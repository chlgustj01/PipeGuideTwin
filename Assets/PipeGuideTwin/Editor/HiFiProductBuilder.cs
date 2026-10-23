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
using PipeGuideTwin.Validation;
using PipeGuideTwin.Visualization;

namespace PipeGuideTwin.Editor
{
    public static class HiFiProductBuilder
    {
        private static PipeGuideTheme t;
        private static Font font;
        private static Transform canvas;
        private static RenderTexture before,after;
        private static Button fileButton,approve,reject;
        private static Text fileMessage,decision;
        private static Button[] initialButtons=new Button[3],localButtons=new Button[3];
        private static Image[] initialEdges=new Image[3],localEdges=new Image[3];
        [MenuItem("PipeGuide Twin/Build High Fidelity Product")]
        public static void Build()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.path!=StepZeroSceneBuilder.ScenePath||scene.isDirty||EditorApplication.isPlaying)throw new InvalidOperationException("Open the saved main scene in Edit Mode.");
            if(GameObject.Find("HiFiStructure")!=null)throw new InvalidOperationException("HiFi rebuild already exists.");
            HiFiWorldBuilder.Build();
            canvas=GameObject.Find("MainCanvas").transform;
            t=AssetDatabase.LoadAssetAtPath<PipeGuideTheme>("Assets/PipeGuideTwin/Materials/PipeGuideTheme.asset");
            t.Background=new Color32(11,18,29,255);t.Panel=new Color32(17,28,43,255);t.Card=new Color32(23,37,55,255);t.Border=new Color32(42,60,79,255);t.TextSecondary=new Color32(151,174,196,255);EditorUtility.SetDirty(t);
            font=canvas.Find("TopBar/Brand").GetComponent<Text>().font;
            before=Texture("BeforePreview");after=Texture("AfterPreview");
            TopBar();
            var inspector=canvas.GetComponentsInChildren<SelectionInspectorUI>(true).First().transform;
            var debug=canvas.GetComponentsInChildren<Text>(true).First(x=>x.name=="DebugInfo").transform.parent;debug.gameObject.SetActive(false);
            foreach(Transform page in canvas.Find("Pages"))
            {
                var l=(RectTransform)page.Find("LeftPanel");var c=(RectTransform)page.Find("CenterWorkspace");var r=(RectTransform)page.Find("RightPanel");
                Anchor(l,new Vector2(.008f,.04f),new Vector2(.174f,.98f));Anchor(c,new Vector2(.182f,.04f),new Vector2(.775f,.98f));Anchor(r,new Vector2(.784f,.04f),new Vector2(.992f,.98f));
                foreach(var p in new[]{l,c,r}){Archive(p,inspector,debug);var im=p.GetComponent<Image>();if(im!=null)im.color=t.Panel;}
                var bottom=page.Find("BottomStatus");foreach(Transform child in bottom)child.gameObject.SetActive(false);
                var pageImage=page.GetComponent<Image>();if(pageImage!=null)pageImage.color=t.Background;
                switch(page.name)
                {
                    case "ProjectPage":Project(l,c,r);break;
                    case "DataPage":Data(l,c,r);break;
                    case "Model3DPage":Model(l,c);break;
                    case "ConsistencyPage":Consistency(l,c);break;
                    case "InitialRoutingPage":Routing(l,c,r,false);break;
                    case "LocalReroutingPage":Routing(l,c,r,true);break;
                    case "ResultComparePage":Compare(l,c,r);break;
                    case "ApprovalHistoryPage":Approval(l,c,r);break;
                }
            }
            Inspector(inspector);
            var actions=canvas.GetComponent<ProductShellActions>();Refs(actions,("fileButton",fileButton),("fileMessage",fileMessage),("approveButton",approve),("rejectButton",reject),("decisionMessage",decision));
            Routes();IssueMarkers();RefinePreviewsAndLegends();
            var status=canvas.Find("StatusBar/Status").GetComponent<Text>();status.fontSize=13;
            canvas.Find("StatusBar").GetComponent<Image>().color=t.Panel;
            var metadata=canvas.Find("StatusBar/SystemMetadata").GetComponent<Text>();metadata.text="데이터 소스  MOCK    |    설계 mm  /  화면 m    |    REV.A    ● 준비 완료";metadata.fontSize=12;metadata.color=t.TextSecondary;
            var sf=canvas.GetComponent<ShellFont>();sf.enabled=false;sf.enabled=true;
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
        private static void TopBar()
        {
            var top=canvas.Find("TopBar");top.GetComponent<Image>().color=t.Panel;
            var b=top.Find("Brand").GetComponent<Text>();b.text="PIPEGUIDE  TWIN";b.fontSize=23;b.color=t.TextPrimary;
            var info=top.Find("ProjectInfo").GetComponent<Text>();info.text="DEMO VESSEL  /  ENGINE ROOM\nREV.A   ● 시스템 정상";info.fontSize=12;
            var nav=top.Find("NavigationPanel");nav.GetComponent<Image>().color=t.Panel;
            foreach(var item in nav.GetComponentsInChildren<WorkflowPlaceholder>()){item.GetComponent<Image>().color=t.Panel;item.GetComponentInChildren<Text>().fontSize=16;}
            Chip("ProductMark",top,"PT",new Vector2(24,-72),52,t.PrimaryOrange);
        }
        private static void Project(Transform l,Transform c,Transform r)
        {
            Header(c,"프로젝트 개요","선박 배관 설계 검토 워크스페이스");Section(l,"프로젝트",18);
            var project=Card("ProjectSelection",l,12,60,294,200);Accent(project,t.PrimaryOrange);
            Label(project,"PROJECT 01",13,t.PrimaryOrange,18,18,240,24);Label(project,"PipeGuide Twin",22,t.TextPrimary,18,56,260,38);Label(project,"데모 선박  ·  기관실",14,t.TextSecondary,18,105,250,28);Chip("Ready",project,"활성",new Vector2(18,-153),72,t.SuccessGreen);
            var context=Card("ProjectContext",l,12,280,294,220);Section(context,"프로젝트 컨텍스트",18);Row(context,"선박","DEMO VESSEL",64);Row(context,"구역","ENGINE ROOM",106);Row(context,"리비전","REV.A",148);
            var preview=Card("RoomPreview",c,8,92,1118,322);var image=Raw(preview,before,12,12,1094,298);Chip("RoomBadge",preview,"기관실 / 3D PREVIEW",new Vector2(28,-26),190,t.SelectionCyan);
            var values=new[]{"8","20","4","6"};var labels=new[]{"장비","배관 라인","정합성 이슈","경로 후보"};
            for(int i=0;i<4;i++)Metric(c,"OverviewMetric_"+i,labels[i],values[i],8+i*282,434,272,118,i==2?t.WarningYellow:t.TextPrimary);
            var progress=Card("WorkflowProgress",c,8,574,1118,194);Section(progress,"검토 진행률",20);Label(progress,"02 / 08",28,t.PrimaryOrange,910,18,180,48);Progress(progress,24,89,1068,.25f,t.PrimaryOrange);
            var steps=new[]{"프로젝트","설계 데이터","3D 매핑","정합성 검사","경로 검토","설계자 결정"};for(int i=0;i<6;i++)Label(progress,steps[i],13,i<2?t.TextPrimary:t.TextSecondary,24+i*177,130,170,30);
            Section(r,"현재 작업 상태",20);var states=Card("Readiness",r,12,68,374,330);
            StatusRow(states,"설계 데이터","준비 완료",30,t.SuccessGreen);StatusRow(states,"3D 매핑","준비 완료",100,t.SuccessGreen);StatusRow(states,"정합성 검사","검토 대기",170,t.WarningYellow);StatusRow(states,"경로 추천","검토 대기",240,t.TextSecondary);
            var activity=Card("Activity",r,12,420,374,300);Section(activity,"최근 작업",20);TimelineRow(activity,"프로젝트 생성","REV.A · 원본 보존",70,t.PrimaryOrange);TimelineRow(activity,"기관실 모델 준비","장비 8 · 배관 20",145,t.SelectionCyan);TimelineRow(activity,"설계 검토 준비","설계자 결정 대기",220,t.TextSecondary);
        }
        private static void Data(Transform l,Transform c,Transform r)
        {
            Header(c,"설계 데이터 불러오기","구조화 설계 데이터 · Excel / CSV");Section(l,"데이터 소스",20);
            var source=Card("Workbook",l,12,70,294,180);Chip("FileType",source,"XLSX",new Vector2(18,-20),75,t.SuccessGreen);Label(source,"설계 워크북",21,t.TextPrimary,18,70,260,32);Label(source,"18개 시트  /  42 KB",14,t.TextSecondary,18,120,260,28);
            var flow=Card("DataFlow",l,12,270,294,330);Section(flow,"데이터 파이프라인",18);TimelineRow(flow,"원본 데이터","READ ONLY",70,t.SelectionCyan);TimelineRow(flow,"작업 사본","검토용 컨텍스트",150,t.PrimaryOrange);TimelineRow(flow,"변경 기록","설계자 결정",230,t.TextSecondary);
            var drop=Card("FileDropArea",c,8,92,1118,228);Border(drop,t.PrimaryOrange);
            UploadIcon(drop,100,68);Label(drop,"Excel 파일을 이곳에 놓으세요",24,t.TextPrimary,210,38,780,45);Label(drop,".xlsx   /   .csv",14,t.TextSecondary,212,94,500,28);
            fileButton=Button("ChooseExcel",drop,"Excel 파일 선택",212,150,230,44,t.PrimaryOrange);fileMessage=Label(drop,"",12,t.TextSecondary,470,156,600,40,"FileMessage");
            var file=Card("FileList",c,8,342,1118,132);Chip("FileIcon",file,"XLSX",new Vector2(20,-23),74,t.SuccessGreen);Label(file,"PipeGuideTwin_DB_Preprocessing_Data.xlsx",18,t.TextPrimary,120,24,850,36);Label(file,"18 SHEETS   ·   42 KB",13,t.TextSecondary,122,74,780,28);Chip("FileReady",file,"준비 완료",new Vector2(954,-46),130,t.SuccessGreen);
            var table=Card("SheetStatus",c,8,496,1118,288);Section(table,"시트 상태",20);var names=new[]{"Equipment","Nozzle","Pipe_Design","Pipe_3D","Validation","Routing"};var counts=new[]{"3","3","5","6","5","6"};for(int i=0;i<6;i++){Label(table,names[i],15,t.TextPrimary,24,68+i*32,650,28);Label(table,counts[i]+" 행",15,t.TextSecondary,790,68+i*32,120,28);Label(table,"● 준비",13,t.SuccessGreen,960,68+i*32,130,28);}
            Section(r,"데이터 품질",20);Metric(r,"IdQuality","ID 오류","0",12,68,181,132,t.SuccessGreen);Metric(r,"FkQuality","참조 키 오류","0",205,68,181,132,t.SuccessGreen);Metric(r,"DuplicateQuality","중복 객체","1",12,214,181,132,t.OrangeDuplicate);Metric(r,"MissingQuality","3D 누락","1",205,214,181,132,t.DangerRed);
            var health=Card("QualityHealth",r,12,372,374,192);Section(health,"데이터 품질 지수",18);Label(health,"92",44,t.SuccessGreen,22,64,160,65);Label(health,"%",20,t.TextSecondary,98,88,80,36);Progress(health,22,153,328,.92f,t.SuccessGreen);
            var check=Card("QualityChecks",r,12,588,374,178);Section(check,"검토 항목",18);Row(check,"ID 무결성","정상",64);Row(check,"3D 누락 검토","1건",110);
        }
        private static void Model(Transform l,Transform c)
        {
            Header(c,"3D 모델","기관실 / 배관 시스템 검토");Section(l,"시스템 트리",20);Chip("Visibility",l,"전체 표시",new Vector2(184,-17),110,t.SelectionCyan);
            var groups=new[]{("냉각수 계통",t.CandidateBlue,new[]{"PIPE_001","PIPE_002","EQ_PUMP_001"}),("가열 / 증기",t.PrimaryOrange,new[]{"PIPE_003","EQ_HEATER_001"}),("회수 계통",t.SuccessGreen,new[]{"PIPE_004"}),("유틸리티",t.SelectionCyan,new[]{"환경 배관 16개","필터 · 냉각기 · 스키드"})};float y=80;
            foreach(var group in groups){var g=Card("SystemGroup",l,12,y,294,62+group.Item3.Length*34);Accent(g,group.Item2);Label(g,group.Item1,17,t.TextPrimary,18,14,250,30);for(int i=0;i<group.Item3.Length;i++)Label(g,"◦  "+group.Item3[i],14,t.TextSecondary,24,54+i*32,260,28);y+=76+group.Item3.Length*34;}
            Toolbar(c,"장비 8    /    배관 20    /    밸브 8",t.SelectionCyan);
            Legend(c,false);
        }
        private static void Consistency(Transform l,Transform c)
        {
            Header(c,"정합성 검사","검출 이슈와 3D 위치 검토");
            var issue=l.GetComponentsInChildren<ValidationPanelUI>(true).First().transform;issue.SetParent(l,false);Anchor((RectTransform)issue,Vector2.zero,Vector2.one);issue.gameObject.SetActive(true);issue.GetComponent<Image>().color=t.Panel;
            var heading=issue.Find("Heading").GetComponent<Text>();SetText(heading,"검출 이슈",22,t.TextPrimary,18,18,280,40);
            var summary=issue.Find("Summary").GetComponent<Text>();SetText(summary,"",14,t.TextSecondary,18,76,280,105);
            var list=(RectTransform)issue.Find("IssueList");Pos(list,18,194,280,600);
            var template=list.GetComponentsInChildren<Button>(true).First();Pos(template.GetComponent<RectTransform>(),0,0,280,119);template.GetComponent<Image>().color=t.Card;Border(template.transform,t.Border);
            var text=template.GetComponentInChildren<Text>();SetText(text,"",14,t.TextPrimary,14,16,252,90);
            Chip("SeverityChip",template.transform,"높음",new Vector2(204,-10),62,t.DangerRed);
            var so=new SerializedObject(issue.GetComponent<ValidationPanelUI>());so.FindProperty("rowSpacing").floatValue=132;so.FindProperty("theme").objectReferenceValue=t;so.ApplyModifiedPropertiesWithoutUndo();
            Toolbar(c,"누락 1    불일치 1    중복 1    확인 필요 1",t.WarningYellow);IssueLegend(c);
        }
        private static void Routing(Transform l,Transform c,Transform r,bool local)
        {
            Header(c,local?"국소 재설계":"초기 경로 추천",local?"문제 구간 · 대체 경로 비교":"시작 노즐 → 끝 노즐 / 후보 경로 검토");
            if(local){var target=Card("RerouteTarget",l,12,26,294,270);Label(target,"재설계 대상",17,t.TextSecondary,18,20,260,30);Label(target,"PIPE_003",29,t.TextPrimary,18,64,260,45);Chip("Collision",target,"구조물 충돌",new Vector2(18,-124),142,t.DangerRed);Row(target,"시작","NODE_A",188);Row(target,"끝","NODE_B",226);}
            else{Endpoint(l,"시작 노즐","NZ_PUMP_OUT_01","냉각수 펌프",26,t.SuccessGreen);Endpoint(l,"끝 노즐","NZ_TANK_IN_01","냉각수 탱크",216,t.SelectionCyan);}
            var constraints=Card("RouteConstraints",l,12,local?320:422,294,222);Section(constraints,"설계 제약조건",18);Row(constraints,"직교 경로","ON",70);Row(constraints,"최소 간격","150 mm",116);Row(constraints,"연결 노드","유지",162);
            Toolbar(c,local?"현재 경로 4.1 m    /    대체 후보 3개":"추천 후보 3개    /    X · Y · Z 직교 경로",t.SelectionCyan);
            for(int i=0;i<3;i++)
            {
                float y=44+i*246;var card=Card((local?"LocalCandidate_":"InitialCandidate_")+i,r,12,y,374,230);var edge=Accent(card,t.Border);
                var button=card.gameObject.AddComponent<Button>();button.transition=Selectable.Transition.None;
                Label(card,((char)('A'+i)).ToString(),32,i==0?t.SuccessGreen:i==1?t.SelectionCyan:t.CandidateBlue,20,15,60,50);
                Label(card,"후보 "+(char)('A'+i),19,t.TextPrimary,86,22,180,36);if(i==0)Chip("Recommended",card,"추천",new Vector2(266,-22),86,t.SuccessGreen);
                var length=local?new[]{"3.2","3.5","3.9"}:new[]{"7.8","8.2","8.6"};var bend=local?new[]{"3","2","3"}:new[]{"5","3","4"};var clearance=local?new[]{"180","140","220"}:new[]{"180","220","260"};var cost=local?new[]{"42","44","48"}:new[]{"82","86","91"};
                MiniMetric(card,"길이",length[i]+" m",20,90,160);MiniMetric(card,"굽힘",bend[i]+"개",204,90,150);MiniMetric(card,"최소 간격",clearance[i]+" mm",20,158,160);MiniMetric(card,local?"비용 추정":"비용 지수",cost[i],204,158,150);
                if(local){localButtons[i]=button;localEdges[i]=edge;}else{initialButtons[i]=button;initialEdges[i]=edge;}
            }
            RouteLegend(c,local);
        }
        private static void Compare(Transform l,Transform c,Transform r)
        {
            Header(c,"결과 비교","원본 경로 → 후보 B");Section(l,"변경 컨텍스트",20);var context=Card("CompareContext",l,12,68,294,260);Label(context,"PIPE_003",28,t.TextPrimary,18,22,260,45);Row(context,"변경 전","원본 경로",92);Row(context,"변경 후","후보 B",140);Chip("Resolved",context,"충돌 해소",new Vector2(18,-202),130,t.SuccessGreen);
            var audit=Card("CompareReview",l,12,352,294,220);Section(audit,"검토 기준",18);Row(audit,"노드 연결","유지",64);Row(audit,"경로 방식","직교",110);Row(audit,"비용 / 압력","추정값",156);
            var labels=new[]{"길이","굽힘","충돌","여유 간격"};var values=new[]{"4.1 → 3.5 m","5 → 2","1 → 0","20 → 140 mm"};for(int i=0;i<4;i++)Metric(c,"CompareMetric_"+i,labels[i],values[i],8+i*282,92,272,105,i==2?t.SuccessGreen:t.TextPrimary,24);
            for(int i=0;i<2;i++)
            {
                var view=Card(i==0?"BeforeView":"AfterView",c,8+i*568,220,550,488);Label(view,i==0?"변경 전":"변경 후",24,t.TextPrimary,18,17,300,40);Chip("ViewState",view,i==0?"원본":"후보 B",new Vector2(410,-21),114,i==0?t.DangerRed:t.SelectionCyan);
                Raw(view,i==0?before:after,12,75,526,326);Label(view,i==0?"충돌 1건":"충돌 0건",18,i==0?t.DangerRed:t.SuccessGreen,20,422,220,35);Label(view,i==0?"Original Route":"Candidate B",13,t.TextSecondary,270,429,250,30);
            }
            Section(r,"개선 효과",20);var names=new[]{"길이","굽힘","충돌","여유 간격","압력손실 추정","비용 추정"};var numbers=new[]{"-14.6%","-60%","해소","+120 mm","-27%","-12%"};
            for(int i=0;i<6;i++){var card=Card("Improvement_"+i,r,12,64+i*117,374,105);Label(card,names[i],13,t.TextSecondary,18,12,340,26);Label(card,numbers[i],32,t.SuccessGreen,18,43,340,50);}
        }
        private static void Approval(Transform l,Transform c,Transform r)
        {
            Header(c,"승인 및 변경 이력","설계자 검토 · 결정 타임라인");Section(l,"리비전",20);var a=Card("RevisionA",l,12,68,294,130);Label(a,"REV.A",24,t.TextPrimary,18,20,250,40);Label(a,"원본 설계",14,t.TextSecondary,18,76,250,28);var b=Card("RevisionB",l,12,216,294,166);Accent(b,t.SuccessGreen);Label(b,"REV.B",24,t.TextPrimary,18,20,250,40);Label(b,"PIPE_003  /  후보 B",14,t.TextSecondary,18,76,260,28);Chip("Approved",b,"승인됨",new Vector2(18,-117),100,t.SuccessGreen);
            var track=Card("RevisionTimeline",c,8,94,1118,230);Label(track,"변경 타임라인",20,t.TextPrimary,24,20,600,36);TimelineRow(track,"REV.A   원본","원본 모델 보존",82,t.TextSecondary);TimelineRow(track,"REV.B   후보 B","PIPE_003 · 충돌 해소 / 굽힘 감소",154,t.SuccessGreen);
            var detail=Card("RevisionDetail",c,8,348,1118,346);Section(detail,"변경 상세",20);Row(detail,"대상 배관","PIPE_003",78);Row(detail,"변경 전","ORIGINAL_PIPE_003",132);Row(detail,"변경 후","CAND_B",186);Row(detail,"결정 사유","충돌 제거 및 굽힘 감소",240);
            Section(r,"설계자 결정",20);var action=Card("ApprovalAction",r,12,68,374,330);Label(action,"후보 B",29,t.TextPrimary,22,23,330,48);Chip("Awaiting",action,"검토 대기",new Vector2(22,-89),120,t.WarningYellow);approve=Button("ApproveMock",action,"승인",22,164,155,44,t.SuccessGreen);reject=Button("RejectMock",action,"반려",195,164,155,44,t.DangerRed);decision=Label(action,"설계자 결정을 선택하세요",14,t.TextSecondary,22,241,330,68,"DecisionMessage");
            var protection=Card("OriginalSafety",r,12,422,374,202);Section(protection,"원본 보호",18);TimelineRow(protection,"원본  →  작업 사본","설계 데이터 보존",68,t.SelectionCyan);TimelineRow(protection,"작업 사본  →  리비전","설계자 결정 후 기록",139,t.TextSecondary);
        }
        private static void Inspector(Transform root)
        {
            Archive((RectTransform)root,null,null);Anchor((RectTransform)root,Vector2.zero,Vector2.one);root.GetComponent<Image>().color=t.Panel;
            var ui=root.gameObject.AddComponent<StructuredInspectorUI>();ui.App=UnityEngine.Object.FindFirstObjectByType<PipeGuideAppManager>();ui.Selection=UnityEngine.Object.FindFirstObjectByType<SelectionManager>();ui.Validation=UnityEngine.Object.FindFirstObjectByType<ValidationResultController>();ui.Theme=t;ui.CameraController=Camera.main.GetComponent<EngineeringCameraController>();
            ui.Heading=Label(root,"선택한 객체",20,t.TextSecondary,24,22,350,36,"InspectorHeading");
            ui.Identifier=Label(root,"객체 선택",30,t.TextPrimary,24,83,350,48,"ObjectIdValue");
            ui.Name=Label(root,"배관 또는 장비를 클릭하세요",15,t.TextSecondary,24,143,350,55,"ObjectNameValue");
            ui.Chip=Label(root,"선택 대기",14,t.SelectionCyan,24,211,350,30,"ObjectStatus");
            var row1=Card("PropertyOne",root,18,266,362,95);ui.FirstLabel=Label(row1,"직경",13,t.TextSecondary,16,12,330,25);ui.FirstValue=Label(row1,"—",25,t.TextPrimary,16,44,330,40,"FirstValue");
            var row2=Card("PropertyTwo",root,18,375,362,95);ui.SecondLabel=Label(row2,"재질",13,t.TextSecondary,16,12,330,25);ui.SecondValue=Label(row2,"—",22,t.TextPrimary,16,44,330,40,"SecondValue");
            var row3=Card("PropertyThree",root,18,484,362,95);ui.ThirdLabel=Label(row3,"시작 노드",13,t.TextSecondary,16,12,330,25);ui.ThirdValue=Label(row3,"—",18,t.TextPrimary,16,47,330,35,"ThirdValue");
            var row4=Card("PropertyFour",root,18,593,362,95);ui.FourthLabel=Label(row4,"끝 노드",13,t.TextSecondary,16,12,330,25);ui.FourthValue=Label(row4,"—",18,t.TextPrimary,16,47,330,35,"FourthValue");
            ui.FocusButton=Button("FocusSelected",root,"3D 위치 보기",18,716,362,45,t.SelectionCyan);ui.Health=Label(root,"상태  ●  활성",13,t.TextSecondary,24,791,350,28,"ObjectHealth");EditorUtility.SetDirty(ui);
        }
        private static void Routes()
        {
            var fixtures=ScriptableObject.CreateInstance<HiFiVisualFixtures>();
            fixtures.Initial=new[]{Path("Initial_A",new Vector3(-3,1.5f,1),new Vector3(-3,3,1),new Vector3(3,3,1),new Vector3(3,3,2),new Vector3(3,2,2)),Path("Initial_B",new Vector3(-3,1.5f,1),new Vector3(-3,3.55f,1),new Vector3(3,3.55f,1),new Vector3(3,3.55f,2),new Vector3(3,2,2)),Path("Initial_C",new Vector3(-3,1.5f,1),new Vector3(-3,4.1f,1),new Vector3(3,4.1f,1),new Vector3(3,4.1f,2),new Vector3(3,2,2))};
            fixtures.Original=Path("Original_Route",new Vector3(2,1.2f,-3),new Vector3(0,1.2f,-3),new Vector3(0,2.55f,-3),new Vector3(0,2.55f,1));
            fixtures.Local=new[]{Path("Local_A",new Vector3(2,1.2f,-3),new Vector3(-1.3f,1.2f,-3),new Vector3(-1.3f,3.1f,-3),new Vector3(-1.3f,3.1f,1),new Vector3(0,3.1f,1),new Vector3(0,2.55f,1)),Path("Local_B",new Vector3(2,1.2f,-3),new Vector3(-.65f,1.2f,-3),new Vector3(-.65f,3.65f,-3),new Vector3(-.65f,3.65f,1),new Vector3(0,3.65f,1),new Vector3(0,2.55f,1)),Path("Local_C",new Vector3(2,1.2f,-3),new Vector3(.7f,1.2f,-3),new Vector3(.7f,4.15f,-3),new Vector3(.7f,4.15f,1),new Vector3(0,4.15f,1),new Vector3(0,2.55f,1))};
            fixtures.CollisionPositionMm=new Vector3(0,2550,-1600);AssetDatabase.CreateAsset(fixtures,"Assets/PipeGuideTwin/Data/Demo/HiFiVisualFixtures.asset");
            var root=new GameObject("HiFiCandidateVisuals");root.transform.SetParent(GameObject.Find("RoutingVisualizationRoot").transform,false);
            var controller=root.AddComponent<MockRouteVisualController>();controller.App=UnityEngine.Object.FindFirstObjectByType<PipeGuideAppManager>();controller.Fixtures=fixtures;controller.Theme=t;controller.VisualRoot=root.transform;
            controller.RouteMaterials=new[]{Unlit("CandidateGreen",t.SuccessGreen),Unlit("CandidateCyan",t.SelectionCyan),Unlit("CandidateBlue",t.CandidateBlue),Unlit("OriginalRed",t.DangerRed)};
            controller.BeforeCamera=PreviewCamera("BeforeCamera",before,~((1<<25)|(1<<26)|(1<<27)));controller.AfterCamera=PreviewCamera("AfterCamera",after,~((1<<25)|(1<<27)|(1<<28)));
            controller.InitialButtons=initialButtons;controller.LocalButtons=localButtons;controller.InitialBorders=initialEdges;controller.LocalBorders=localEdges;EditorUtility.SetDirty(controller);
        }
        private static void IssueMarkers()
        {
            var root=new GameObject("HiFiIssueIndicators");root.transform.SetParent(GameObject.Find("_WORLD").transform.Find("ValidationVisualizationRoot"),false);
            var m=root.AddComponent<HiFiIssueMarkers>();m.App=UnityEngine.Object.FindFirstObjectByType<PipeGuideAppManager>();m.Results=UnityEngine.Object.FindFirstObjectByType<ValidationResultController>();m.Root=root.transform;m.Materials=new[]{Unlit("WarningMarker",t.WarningYellow),Unlit("CriticalMarker",t.DangerRed),Unlit("DuplicateMarker",t.OrangeDuplicate)};EditorUtility.SetDirty(m);
        }
        public static void RefinePreviewsAndLegends()
        {
            canvas=GameObject.Find("MainCanvas").transform;t=AssetDatabase.LoadAssetAtPath<PipeGuideTheme>("Assets/PipeGuideTwin/Materials/PipeGuideTheme.asset");font=canvas.Find("TopBar/Brand").GetComponent<Text>().font;
            var routes=UnityEngine.Object.FindFirstObjectByType<MockRouteVisualController>();
            if(routes.Fixtures.Initial[0].PointsMm.Length==4)
            {
                var points=routes.Fixtures.Initial[0].PointsMm.ToList();points.Insert(3,new Vector3(3000,3000,2000));routes.Fixtures.Initial[0].PointsMm=points.ToArray();EditorUtility.SetDirty(routes.Fixtures);
            }
            var path="Assets/PipeGuideTwin/Textures/HiFi/ProjectPreview.renderTexture";
            var rt=AssetDatabase.LoadAssetAtPath<RenderTexture>(path);
            if(rt==null){rt=new RenderTexture(1200,330,24){name="ProjectPreview",antiAliasing=2};AssetDatabase.CreateAsset(rt,path);}
            if(routes.ProjectCamera==null)
            {
                routes.ProjectCamera=PreviewCamera("ProjectPreviewCamera",rt,~((1<<25)|(1<<26)|(1<<27)|(1<<28)));
                routes.ProjectCamera.fieldOfView=34;routes.ProjectCamera.aspect=1200f/330;
                routes.ProjectCamera.transform.position=new Vector3(11,7,-14);routes.ProjectCamera.transform.LookAt(new Vector3(0,1.9f,1));
            }
            var pages=canvas.Find("Pages");pages.Find("ProjectPage/CenterWorkspace/RoomPreview").GetComponentInChildren<RawImage>().texture=rt;
            foreach(var camera in new[]{routes.BeforeCamera,routes.AfterCamera}){camera.transform.position=new Vector3(5.8f,4.5f,-7.5f);camera.transform.LookAt(new Vector3(0,2,-.3f));camera.fieldOfView=54;camera.aspect=1.6f;EditorUtility.SetDirty(camera);}
            foreach(var name in new[]{"ConsistencyPage","InitialRoutingPage","LocalReroutingPage"})
            {
                var center=pages.Find(name+"/CenterWorkspace");var old=center.Find("WorkspaceLegend");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
                if(name=="ConsistencyPage")IssueLegend(center);else RouteLegend(center,name=="LocalReroutingPage");
            }
            if(canvas.Find("SelectedObjectLabel")==null)
            {
                var root=Card("SelectedObjectLabel",canvas,0,0,230,52);root.anchorMin=root.anchorMax=root.pivot=new Vector2(.5f,.5f);root.GetComponent<Image>().raycastTarget=false;
                var label=Label(root,"",13,t.SelectionCyan,12,5,206,46);var component=root.gameObject.AddComponent<SelectedObjectLabel>();component.Selection=UnityEngine.Object.FindFirstObjectByType<SelectionManager>();component.ViewerCamera=Camera.main;component.Canvas=(RectTransform)canvas;component.Label=label;component.Background=root.GetComponent<Image>();foreach(var image in root.GetComponentsInChildren<Image>())image.enabled=false;label.enabled=false;EditorUtility.SetDirty(component);
            }
            var sf=canvas.GetComponent<ShellFont>();sf.enabled=false;sf.enabled=true;EditorUtility.SetDirty(routes);EditorUtility.SetDirty(routes.ProjectCamera);
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
        private static void RouteLegend(Transform center,bool local)=>LegendEntries(center,local?new[]{t.DangerRed,t.SuccessGreen,t.SelectionCyan,t.CandidateBlue}:new[]{t.SuccessGreen,t.SelectionCyan,t.CandidateBlue},local?new[]{"원본 경로","후보 A","후보 B","후보 C"}:new[]{"후보 A","후보 B","후보 C"});
        private static void IssueLegend(Transform center)=>LegendEntries(center,new[]{t.DangerRed,t.WarningYellow,t.OrangeDuplicate,t.WarningYellow},new[]{"누락","불일치","중복","확인 필요"});
        private static void LegendEntries(Transform center,Color[] colors,string[] names)
        {
            var go=new GameObject("WorkspaceLegend",typeof(RectTransform));go.transform.SetParent(center,false);var root=(RectTransform)go.transform;root.anchorMin=root.anchorMax=new Vector2(0,0);root.pivot=new Vector2(0,0);root.anchoredPosition=new Vector2(8,12);root.sizeDelta=new Vector2(1118,42);
            for(int i=0;i<names.Length;i++){var dot=Card("Swatch",root,12+i*210,11,10,10);dot.GetComponent<Image>().color=colors[i];dot.GetComponent<Image>().raycastTarget=false;Label(root,names[i],12,t.TextSecondary,32+i*210,7,180,30);}
        }
        private static Camera PreviewCamera(string name,RenderTexture target,int mask)
        {var go=new GameObject(name);go.transform.SetParent(GameObject.Find("_CAMERA").transform,false);var camera=go.AddComponent<Camera>();camera.CopyFrom(Camera.main);camera.targetTexture=target;camera.cullingMask=mask;camera.transform.position=new Vector3(9,6.5f,-10);camera.transform.LookAt(new Vector3(0,1.6f,.4f));camera.fieldOfView=64;camera.enabled=false;return camera;}
        private static MockPathVisual Path(string id,params Vector3[] points)=>new MockPathVisual{Id=id,PointsMm=points.Select(EngineeringUnitConverter.UnityToPositionMm).ToArray()};
        private static RenderTexture Texture(string name){var rt=new RenderTexture(800,500,24){name=name,antiAliasing=2};AssetDatabase.CreateAsset(rt,"Assets/PipeGuideTwin/Textures/HiFi/"+name+".renderTexture");return rt;}
        private static Material Unlit(string name,Color color){var m=new Material(Shader.Find("Universal Render Pipeline/Unlit")){name=name};m.SetColor("_BaseColor",color);AssetDatabase.CreateAsset(m,"Assets/PipeGuideTwin/Materials/HiFi/"+name+".mat");return m;}
        private static void Archive(RectTransform panel,Transform inspector,Transform debug)
        {
            var legacy=new GameObject("_LegacyContent",typeof(RectTransform));legacy.transform.SetParent(panel,false);
            foreach(var child in panel.Cast<Transform>().ToArray())if(child!=legacy.transform&&child!=inspector&&child!=debug)child.SetParent(legacy.transform,false);
            legacy.SetActive(false);
        }
        private static RectTransform Card(string name,Transform parent,float x,float y,float w,float h){var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);var rect=(RectTransform)go.transform;Pos(rect,x,y,w,h);go.GetComponent<Image>().color=t.Card;Border(rect,t.Border);return rect;}
        private static void Border(Transform parent,Color color){for(int i=0;i<4;i++){var g=new GameObject("Stroke_"+i,typeof(RectTransform),typeof(Image));g.transform.SetParent(parent,false);var r=(RectTransform)g.transform;if(i<2){Anchor(r,new Vector2(0,i),new Vector2(1,i));r.sizeDelta=new Vector2(0,1);}else{Anchor(r,new Vector2(i-2,0),new Vector2(i-2,1));r.sizeDelta=new Vector2(1,0);}g.GetComponent<Image>().color=color;g.GetComponent<Image>().raycastTarget=false;}}
        private static Image Accent(Transform parent,Color color){var g=new GameObject("SelectedBorder",typeof(RectTransform),typeof(Image));g.transform.SetParent(parent,false);Anchor((RectTransform)g.transform,Vector2.zero,new Vector2(0,1));((RectTransform)g.transform).sizeDelta=new Vector2(3,0);var image=g.GetComponent<Image>();image.color=color;image.raycastTarget=false;return image;}
        private static Text Label(Transform parent,string value,int size,Color color,float x,float y,float w,float h,string name="Label") {var g=new GameObject(name,typeof(RectTransform),typeof(Text));g.transform.SetParent(parent,false);var text=g.GetComponent<Text>();SetText(text,value,size,color,x,y,w,h);return text;}
        private static void SetText(Text text,string value,int size,Color color,float x,float y,float w,float h){Pos(text.rectTransform,x,y,w,h);text.text=value;text.font=font;text.fontSize=size;text.color=color;text.raycastTarget=false;}
        private static Text Chip(string name,Transform p,string value,Vector2 pos,float width,Color color){var card=Card(name,p,pos.x,-pos.y,width,29);card.GetComponent<Image>().color=t.Background;var text=Label(card,value,12,color,6,4,width-12,24);text.alignment=TextAnchor.UpperCenter;return text;}
        private static Button Button(string name,Transform p,string value,float x,float y,float w,float h,Color color){var card=Card(name,p,x,y,w,h);card.GetComponent<Image>().color=color;var b=card.gameObject.AddComponent<Button>();b.transition=Selectable.Transition.None;var label=Label(card,value,15,t.Background,0,0,w,h);label.alignment=TextAnchor.MiddleCenter;return b;}
        private static void Header(Transform c,string title,string subtitle){Label(c,title,31,t.TextPrimary,8,8,1100,50);Label(c,subtitle,13,t.TextSecondary,10,61,1100,28);}
        private static void Section(Transform p,string title,int size)=>Label(p,title,size,t.TextPrimary,18,18,340,40);
        private static void Row(Transform p,string key,string value,float y){Label(p,key,13,t.TextSecondary,20,y,148,28);Label(p,value,14,t.TextPrimary,164,y,850,30);}
        private static void StatusRow(Transform p,string key,string value,float y,Color color){Label(p,key,15,t.TextPrimary,20,y+6,190,35);Chip("State",p,value,new Vector2(242,-y),110,color);}
        private static void TimelineRow(Transform p,string title,string subtitle,float y,Color color){var dot=Card("TimelineDot",p,20,y+7,7,7);dot.GetComponent<Image>().color=color;Label(p,title,15,t.TextPrimary,46,y,950,32);Label(p,subtitle,12,t.TextSecondary,46,y+31,950,27);}
        private static void Metric(Transform p,string name,string title,string value,float x,float y,float w,float h,Color color,int size=34){var card=Card(name,p,x,y,w,h);Label(card,title,13,t.TextSecondary,16,13,w-32,28);Label(card,value,size,color,16,48,w-32,h-48);}
        private static void MiniMetric(Transform p,string title,string value,float x,float y,float w){Label(p,title,12,t.TextSecondary,x,y,w,26);Label(p,value,22,t.TextPrimary,x,y+29,w,37);}
        private static void Progress(Transform p,float x,float y,float width,float amount,Color color){var track=Card("ProgressTrack",p,x,y,width,6);track.GetComponent<Image>().color=t.Border;var fill=Card("ProgressFill",track,0,0,width*amount,6);fill.GetComponent<Image>().color=color;}
        private static void Toolbar(Transform c,string value,Color color){var box=Card("WorkspaceToolbar",c,8,95,1118,41);Label(box,value,13,color,16,10,1080,29);}
        private static void Legend(Transform c,bool original){var root=new GameObject("WorkspaceLegend",typeof(RectTransform));root.transform.SetParent(c,false);var r=(RectTransform)root.transform;r.anchorMin=r.anchorMax=new Vector2(0,0);r.pivot=new Vector2(0,0);r.anchoredPosition=new Vector2(8,12);r.sizeDelta=new Vector2(1118,42);var colors=original?new[]{t.DangerRed,t.SuccessGreen,t.SelectionCyan,t.CandidateBlue}:new[]{t.CandidateBlue,t.SelectionCyan,t.PrimaryOrange,t.SuccessGreen};var names=original?new[]{"현재 / 누락","후보 A","후보 B","후보 C"}:new[]{"냉각수","해수","가열 / 증기","회수"};for(int i=0;i<4;i++){var dot=Card("LegendSwatch",r,12+i*210,11,10,10);dot.GetComponent<Image>().color=colors[i];dot.GetComponent<Image>().raycastTarget=false;Label(r,names[i],12,t.TextSecondary,32+i*210,7,180,30);}}
        private static void Endpoint(Transform p,string title,string id,string name,float y,Color color){var card=Card("Endpoint",p,12,y,294,168);Accent(card,color);Label(card,title,13,color,18,15,260,28);Label(card,id,16,t.TextPrimary,18,65,260,34);Label(card,name,13,t.TextSecondary,18,118,260,28);}
        private static RawImage Raw(Transform p,UnityEngine.Texture texture,float x,float y,float w,float h){var g=new GameObject("RenderPreview",typeof(RectTransform),typeof(RawImage));g.transform.SetParent(p,false);Pos((RectTransform)g.transform,x,y,w,h);var image=g.GetComponent<RawImage>();image.texture=texture;return image;}
        private static void UploadIcon(Transform p,float x,float y){var bar=Card("UploadStem",p,x+14,y,6,45);bar.GetComponent<Image>().color=t.PrimaryOrange;var a=Card("ArrowA",p,x,y+9,22,5);a.localRotation=Quaternion.Euler(0,0,45);a.GetComponent<Image>().color=t.PrimaryOrange;var b=Card("ArrowB",p,x+18,y+9,22,5);b.localRotation=Quaternion.Euler(0,0,-45);b.GetComponent<Image>().color=t.PrimaryOrange;var baseLine=Card("UploadBase",p,x-3,y+55,43,4);baseLine.GetComponent<Image>().color=t.PrimaryOrange;}
        private static void Pos(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
        private static void Anchor(RectTransform r,Vector2 lo,Vector2 hi){r.anchorMin=lo;r.anchorMax=hi;r.offsetMin=r.offsetMax=Vector2.zero;}
        private static void Refs(UnityEngine.Object target,params(string key,UnityEngine.Object value)[] pairs){var so=new SerializedObject(target);foreach(var p in pairs)so.FindProperty(p.key).objectReferenceValue=p.value;so.ApplyModifiedPropertiesWithoutUndo();}
    }
}
