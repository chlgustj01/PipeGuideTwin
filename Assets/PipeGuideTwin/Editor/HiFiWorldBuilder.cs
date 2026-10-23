using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using PipeGuideTwin.Data;
using PipeGuideTwin.Interaction;
using PipeGuideTwin.Visualization;

namespace PipeGuideTwin.Editor
{
    public static class HiFiWorldBuilder
    {
        private static Material steel, dark, blue, cyan, orange, green, floor, safety, silver;
        public static void Build()
        {
            if(GameObject.Find("HiFiStructure")!=null)throw new InvalidOperationException("HiFi world already exists.");
            steel=Mat("BrushedSteel",new Color(.43f,.52f,.61f),.65f);
            dark=Mat("Graphite",new Color(.09f,.14f,.19f),.5f);
            blue=Mat("CoolingBlue",new Color(.08f,.35f,.65f),.35f);
            cyan=Mat("SeaWaterCyan",new Color(.08f,.62f,.65f),.3f);
            orange=Mat("HeatingOrange",new Color(.78f,.35f,.12f),.3f);
            green=Mat("ReturnGreen",new Color(.14f,.51f,.36f),.3f);
            floor=Mat("Deck",new Color(.12f,.17f,.22f),.35f);
            safety=Mat("SafetyPaint",new Color(.85f,.65f,.12f),.1f);
            silver=Mat("EquipmentSilver",new Color(.61f,.68f,.72f),.55f);
            var equipment=GameObject.Find("EquipmentRoot").transform;
            Equipment(GameObject.Find("EQ_PUMP_001"),0,blue);
            Equipment(GameObject.Find("EQ_TANK_001"),1,silver);
            Equipment(GameObject.Find("EQ_HEATER_001"),2,orange);
            var definitions=new[]{("EQ_FILTER_001","해수 여과기",new Vector3(-5.4f,1,-2.7f),3), ("EQ_EXCHANGER_001","판형 열교환기",new Vector3(-4.5f,1.15f,3.6f),2), ("EQ_COOLER_001","보조 냉각기",new Vector3(0,1,4),2), ("EQ_SKID_001","유틸리티 스키드",new Vector3(5.2f,.8f,-1.2f),0), ("EQ_RECEIVER_001","서비스 수조",new Vector3(5.2f,1.2f,4),1)};
            foreach(var d in definitions)
            {
                var go=Root(d.Item1,equipment);go.transform.localPosition=d.Item3;
                var item=go.AddComponent<PipeGuideSelectable>();item.ObjectId=d.Item1;item.DisplayName=d.Item2;item.ObjectType=PIDComponentType.Equipment;
                go.AddComponent<BoxCollider>().size=new Vector3(2.1f,2,1.5f);
                go.AddComponent<SelectionHighlight>().ConfigureSelectionMaterial(AssetDatabase.LoadAssetAtPath<Material>("Assets/PipeGuideTwin/Materials/SelectionCyan.mat"));
                Equipment(go,d.Item4,d.Item4==0?blue:silver);
            }
            var roots=GameObject.Find("PipeModelRoot").transform;
            var primary=new[]{new[]{new Vector3(-3,.75f,1),new Vector3(-3,2.4f,1),new Vector3(0,2.4f,1)},new[]{new Vector3(0,2.4f,1),new Vector3(3,2.4f,1),new Vector3(3,2.4f,2)},new[]{new Vector3(2,1,-3),new Vector3(0,1,-3),new Vector3(0,2.4f,-3),new Vector3(0,2.4f,1)},new[]{new Vector3(-3,.5f,1),new Vector3(-3,.5f,-2),new Vector3(2,.5f,-2),new Vector3(2,.5f,-3)}};
            var systems=new[]{blue,cyan,orange,green};
            for(int i=0;i<4;i++)
            {
                var go=GameObject.Find("PIPE_00"+(i+1));
                foreach(var renderer in go.GetComponentsInChildren<Renderer>())renderer.enabled=false;
                var local=primary[i].Select(p=>go.transform.InverseTransformPoint(p)).ToArray();
                Tube("HiFiPipeGeometry",go.transform,local,i<2?.105f:.08f,systems[i],go.name+"_Tube");
                var item=go.GetComponent<PipeGuideSelectable>();item.StartNodeId=i==2?"NZ_HEATER_OUT_01":"NZ_PUMP_OUT_01";item.EndNodeId=i==1?"NZ_TANK_IN_01":"NODE_MID_01";
                Flange("Flange_IN",go.transform,local[0],(local[1]-local[0]).normalized,i<2?.27f:.22f);
                Flange("Flange_OUT",go.transform,local[local.Length-1],(local[local.Length-1]-local[local.Length-2]).normalized,i<2?.27f:.22f);
            }
            // Explicit engineering display fixtures. No path search or obstacle reasoning.
            for(int i=0;i<16;i++)
            {
                int system=i%4;float z=2.65f+(i/4)*.52f;float h=2.8f+system*.34f;
                Vector3[] points;
                if(i<12)points=new[]{new Vector3(-6.7f,.65f+i*.035f,z),new Vector3(-6.7f,h,z),new Vector3(6.3f,h,z),new Vector3(6.3f,1.0f,z)};
                else points=new[]{new Vector3(-5.8f,.85f+(i-12)*.28f,-4.7f),new Vector3(5.8f,.85f+(i-12)*.28f,-4.7f),new Vector3(5.8f,.85f+(i-12)*.28f,1.8f)};
                var pipe=Root("ENV_PIPE_"+(i+1).ToString("000"),roots);
                Tube("DisplayGeometry",pipe.transform,points,.055f+(system==0?.025f:0),systems[system],pipe.name+"_Tube");
                Flange("Flange_A",pipe.transform,points[1],(points[1]-points[0]).normalized,.20f);
                Flange("Flange_B",pipe.transform,points[points.Length-1],(points[points.Length-1]-points[points.Length-2]).normalized,.20f);
                if(i<8)Valve(pipe.transform,new Vector3(-4.9f+i*1.25f,h,z),i+1);
            }
            Structure();Lighting();
            var camera=Camera.main;camera.transform.position=new Vector3(11.8f,7,-14.5f);camera.transform.LookAt(new Vector3(0,1.6f,.5f));camera.fieldOfView=52;
            camera.backgroundColor=new Color(.055f,.082f,.12f);
            var so=new SerializedObject(camera.GetComponent<EngineeringCameraController>());so.FindProperty("pivot").vector3Value=new Vector3(0,1.6f,.5f);so.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void Equipment(GameObject root,int kind,Material body)
        {
            root.transform.localScale=Vector3.one;
            var old=root.GetComponent<Renderer>();if(old!=null)old.enabled=false;
            var box=root.GetComponent<BoxCollider>();if(box!=null)box.size=new Vector3(2.1f,2,1.5f);
            var capsule=root.GetComponent<CapsuleCollider>();if(capsule!=null){capsule.height=3;capsule.radius=.9f;}
            var g=Root("IndustrialAssembly",root.transform).transform;
            if(kind==0)
            {
                Part("SkidBase",g,PrimitiveType.Cube,new Vector3(0,-.58f,0),new Vector3(2.3f,.2f,1.3f),dark);
                Part("Motor",g,PrimitiveType.Cylinder,new Vector3(-.55f,.1f,0),new Vector3(.7f,.55f,.7f),body,Quaternion.Euler(0,0,90));
                Part("PumpCasing",g,PrimitiveType.Cylinder,new Vector3(.55f,.05f,0),new Vector3(.85f,.3f,.85f),silver,Quaternion.Euler(0,0,90));
                for(int i=0;i<7;i++)Part("MotorFin_"+i,g,PrimitiveType.Cylinder,new Vector3(-1.0f+i*.14f,.1f,0),new Vector3(.78f,.016f,.78f),body,Quaternion.Euler(0,0,90));
                Part("Coupling",g,PrimitiveType.Cylinder,new Vector3(.08f,.1f,0),new Vector3(.28f,.2f,.28f),dark,Quaternion.Euler(0,0,90));
            }
            else if(kind==1)
            {
                Part("VesselBody",g,PrimitiveType.Cylinder,Vector3.zero,new Vector3(1.65f,1.1f,1.65f),body);
                Part("TopCap",g,PrimitiveType.Sphere,new Vector3(0,1.09f,0),new Vector3(1.65f,.35f,1.65f),silver);
                Part("BottomCap",g,PrimitiveType.Sphere,new Vector3(0,-1.09f,0),new Vector3(1.65f,.3f,1.65f),silver);
                for(int i=0;i<4;i++){float x=i%2==0?-.55f:.55f;float z=i<2?-.55f:.55f;Part("VesselLeg_"+i,g,PrimitiveType.Cube,new Vector3(x,-1.25f,z),new Vector3(.12f,.5f,.12f),steel);}
                Part("Manhole",g,PrimitiveType.Cylinder,new Vector3(0,1.3f,0),new Vector3(.5f,.06f,.5f),dark);
                Part("LevelIndicator",g,PrimitiveType.Cube,new Vector3(.83f,0,-.1f),new Vector3(.06f,1.65f,.08f),cyan);
            }
            else if(kind==2)
            {
                Part("ExchangerBody",g,PrimitiveType.Cylinder,new Vector3(0,.1f,0),new Vector3(1.05f,1.05f,1.05f),body,Quaternion.Euler(0,0,90));
                for(int i=0;i<8;i++)Part("CoolingRib_"+i,g,PrimitiveType.Cylinder,new Vector3(-.85f+i*.24f,.1f,0),new Vector3(1.14f,.035f,1.14f),steel,Quaternion.Euler(0,0,90));
                Part("ControlBox",g,PrimitiveType.Cube,new Vector3(0,.8f,.3f),new Vector3(.75f,.4f,.3f),dark);
                Part("SupportLeft",g,PrimitiveType.Cube,new Vector3(-.7f,-.65f,0),new Vector3(.2f,.65f,.9f),steel);
                Part("SupportRight",g,PrimitiveType.Cube,new Vector3(.7f,-.65f,0),new Vector3(.2f,.65f,.9f),steel);
            }
            else
            {
                Part("FilterBody",g,PrimitiveType.Cylinder,Vector3.zero,new Vector3(1,.75f,1),silver);
                Part("FilterLid",g,PrimitiveType.Cylinder,new Vector3(0,.8f,0),new Vector3(1.18f,.08f,1.18f),body);
                Part("Instrument",g,PrimitiveType.Cube,new Vector3(.55f,.4f,0),new Vector3(.3f,.4f,.35f),dark);
                Part("FilterStand",g,PrimitiveType.Cylinder,new Vector3(0,-.75f,0),new Vector3(1.2f,.1f,1.2f),steel);
            }
            Nozzle(g,"IN",new Vector3(-1.1f,.3f,0));Nozzle(g,"OUT",new Vector3(1.1f,.3f,0));
        }
        private static void Nozzle(Transform p,string id,Vector3 at)
        {var g=Root("Nozzle_"+id,p).transform;g.localPosition=at;Part("Stub",g,PrimitiveType.Cylinder,Vector3.zero,new Vector3(.24f,.25f,.24f),steel,Quaternion.Euler(0,0,90));Flange("NozzleFlange",g,new Vector3(at.x<0?-.25f:.25f,0,0),Vector3.right,.36f);}
        private static void Valve(Transform p,Vector3 at,int id)
        {
            var g=Root("VALVE_"+id.ToString("000"),p).transform;g.localPosition=at;
            Part("ValveBody",g,PrimitiveType.Sphere,Vector3.zero,new Vector3(.28f,.28f,.28f),steel);
            Part("Stem",g,PrimitiveType.Cylinder,new Vector3(0,.2f,0),new Vector3(.045f,.17f,.045f),silver);
            var circle=Enumerable.Range(0,25).Select(i=>new Vector3(Mathf.Cos(i*Mathf.PI/12)*.19f,.4f,Mathf.Sin(i*Mathf.PI/12)*.19f)).ToArray();
            Tube("Handwheel",g,circle,.025f,safety,"ValveWheel_"+id,false);
            Part("WheelSpoke",g,PrimitiveType.Cube,new Vector3(0,.4f,0),new Vector3(.34f,.035f,.035f),safety);
        }
        private static void Flange(string name,Transform p,Vector3 at,Vector3 direction,float diameter)
        {
            Part(name,p,PrimitiveType.Cylinder,at,new Vector3(diameter,.035f,diameter),silver,Quaternion.FromToRotation(Vector3.up,direction));
        }
        private static void Structure()
        {
            var p=Root("HiFiStructure",GameObject.Find("StructureRoot").transform).transform;
            var deck=GameObject.Find("Floor");deck.transform.localScale=new Vector3(18,.22f,13);deck.GetComponent<Renderer>().sharedMaterial=floor;
            var a=GameObject.Find("Wall_A");a.transform.localPosition=new Vector3(0,2.4f,6.4f);a.transform.localScale=new Vector3(18,4.8f,.18f);a.GetComponent<Renderer>().sharedMaterial=dark;
            var b=GameObject.Find("Wall_B");b.transform.localPosition=new Vector3(-8.8f,2.4f,0);b.transform.localScale=new Vector3(.18f,4.8f,13);b.GetComponent<Renderer>().sharedMaterial=dark;
            for(int x=-8;x<=8;x+=2)Part("DeckSeam_"+x,p,PrimitiveType.Cube,new Vector3(x,.005f,0),new Vector3(.015f,.01f,12.5f),steel);
            for(int z=-6;z<=6;z+=2)Part("DeckLine_"+z,p,PrimitiveType.Cube,new Vector3(0,.008f,z),new Vector3(17.5f,.01f,.015f),steel);
            for(int x=-7;x<=7;x+=7)
            {
                Part("VerticalFrame_"+x,p,PrimitiveType.Cube,new Vector3(x,2.3f,5.9f),new Vector3(.22f,4.6f,.3f),steel);
                Part("PipeRackPost_"+x,p,PrimitiveType.Cube,new Vector3(x,1.9f,2.5f),new Vector3(.16f,3.8f,.16f),steel);
            }
            for(int j=0;j<3;j++)Part("RackBeam_"+j,p,PrimitiveType.Cube,new Vector3(0,2.6f+j*.7f,4.9f),new Vector3(14.3f,.12f,.18f),steel);
            Part("RoofBeam",p,PrimitiveType.Cube,new Vector3(0,4.55f,5.85f),new Vector3(17.5f,.26f,.3f),steel);
            Part("Walkway",p,PrimitiveType.Cube,new Vector3(0,.08f,-3.9f),new Vector3(11,.15f,1.1f),dark);
            for(int i=0;i<40;i++)Part("Grating_"+i,p,PrimitiveType.Cube,new Vector3(-5.3f+i*.27f,.17f,-3.9f),new Vector3(.035f,.025f,1.05f),steel);
            for(int i=0;i<2;i++)Part("WalkwayEdge_"+i,p,PrimitiveType.Cube,new Vector3(0,.18f,-4.45f+i*1.1f),new Vector3(11,.025f,.045f),safety);
            for(int i=0;i<6;i++){var g=Root("Support_"+i,p).transform;g.localPosition=new Vector3(-5.5f+i*2.1f,0,3.6f);Part("SupportPost",g,PrimitiveType.Cube,new Vector3(0,1.35f,0),new Vector3(.075f,2.7f,.075f),steel);Part("SupportArm",g,PrimitiveType.Cube,new Vector3(0,2.65f,.4f),new Vector3(.08f,.08f,1),steel);}
            for(int i=0;i<5;i++)Part("BulkheadRib_"+i,p,PrimitiveType.Cube,new Vector3(-7+i*3.5f,2.4f,6.23f),new Vector3(.08f,4.5f,.08f),steel);
        }
        private static void Lighting()
        {
            var light=GameObject.Find("Directional Light").GetComponent<Light>();light.intensity=1.65f;light.color=new Color(.88f,.94f,1);light.transform.rotation=Quaternion.Euler(42,-35,0);
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.3f,.36f,.43f);RenderSettings.ambientIntensity=1;
            var parent=GameObject.Find("_LIGHTING").transform;
            for(int i=0;i<3;i++){var go=Root("IndustrialFill_"+i,parent);var l=go.AddComponent<Light>();l.type=LightType.Point;l.range=16;l.intensity=i==0?6:4;l.color=i==1?new Color(1,.78f,.52f):new Color(.57f,.78f,1);go.transform.position=new Vector3(-5+i*5,5,1);l.shadows=LightShadows.None;}
        }
        private static Material Mat(string name,Color color,float metallic)
        {var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",.45f);AssetDatabase.CreateAsset(m,"Assets/PipeGuideTwin/Materials/HiFi/"+name+".mat");return m;}
        private static GameObject Root(string name,Transform p){var g=new GameObject(name);g.transform.SetParent(p,false);return g;}
        private static GameObject Part(string name,Transform p,PrimitiveType type,Vector3 pos,Vector3 size,Material m,Quaternion? rotation=null)
        {var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=size;g.transform.localRotation=rotation??Quaternion.identity;g.GetComponent<Renderer>().sharedMaterial=m;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
        private static void Tube(string name,Transform p,Vector3[] points,float radius,Material m,string assetName,bool rounded=true)
        {
            var path=new List<Vector3>{points[0]};
            for(int i=1;i<points.Length-1;i++)
            {
                if(!rounded){path.Add(points[i]);continue;}
                float r=Mathf.Min(.22f,Vector3.Distance(points[i],points[i-1])*.25f,Vector3.Distance(points[i],points[i+1])*.25f);
                var a=points[i]-(points[i]-points[i-1]).normalized*r;var b=points[i]+(points[i+1]-points[i]).normalized*r;
                for(int k=0;k<=8;k++){float t=k/8f;path.Add((1-t)*(1-t)*a+2*(1-t)*t*points[i]+t*t*b);}
            }
            path.Add(points[points.Length-1]);const int sides=12;var verts=new List<Vector3>();var triangles=new List<int>();Vector3 normal=Vector3.up;
            for(int i=0;i<path.Count;i++)
            {
                var tangent=(path[Mathf.Min(i+1,path.Count-1)]-path[Mathf.Max(0,i-1)]).normalized;
                normal=Vector3.ProjectOnPlane(normal,tangent).normalized;if(normal.sqrMagnitude<.1f)normal=Vector3.Cross(tangent,Vector3.right).normalized;
                var binormal=Vector3.Cross(tangent,normal).normalized;
                for(int j=0;j<sides;j++){float a=j*2*Mathf.PI/sides;verts.Add(path[i]+radius*(Mathf.Cos(a)*normal+Mathf.Sin(a)*binormal));}
                if(i==0)continue;
                for(int j=0;j<sides;j++){int a=(i-1)*sides+j,b=(i-1)*sides+(j+1)%sides,c=i*sides+j,d=i*sides+(j+1)%sides;triangles.AddRange(new[]{a,b,c,b,d,c});}
            }
            var mesh=new Mesh{name=assetName};mesh.SetVertices(verts);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh,"Assets/PipeGuideTwin/Models/HiFi/"+assetName+".asset");
            var go=Root(name,p);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=m;
        }
    }
}
