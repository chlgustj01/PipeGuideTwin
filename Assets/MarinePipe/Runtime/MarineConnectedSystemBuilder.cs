using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarinePipe
{
    // Demonstration topology: one inlet -> pump -> discharge -> header -> three branches.
    // All node positions are in metres in a common, root-local coordinate frame.
    public static class MarineConnectedSystemBuilder
    {
        public const string RootName = "MARINE_CONNECTED_SYSTEM_V2";
        const float Tol = 0.001f;
        static readonly Color Sea = new Color(.20f, .69f, .84f);
        static readonly Color Supply = new Color(.93f, .64f, .18f);
        static readonly Color Steel = new Color(.25f, .31f, .37f);
        static readonly Color Silver = new Color(.73f, .78f, .83f);
        static readonly Color Red = new Color(.83f, .27f, .23f);

        class Connection { public string from, to; public Vector3 start, end; public Connection(string a, string b, Vector3 p, Vector3 q) { from=a; to=b; start=p; end=q; } }
        static Material Mat(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Diffuse");
            var result = new Material(shader) { name = name, color = color };
            if (result.HasProperty("_Metallic")) result.SetFloat("_Metallic", .55f);
            if (result.HasProperty("_Smoothness")) result.SetFloat("_Smoothness", .55f);
            return result;
        }
        static GameObject Box(Transform parent, string name, Vector3 p, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition = p;
            go.transform.localScale = scale; go.GetComponent<Renderer>().sharedMaterial = material;
            RemoveCollider(go); return go;
        }
        static GameObject Cylinder(Transform parent, string name, Vector3 p, Vector3 axis, float radius, float length, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name; go.transform.SetParent(parent, false); go.transform.localPosition=p;
            go.transform.localRotation=Quaternion.FromToRotation(Vector3.up, axis.normalized);
            go.transform.localScale = new Vector3(radius * 2, length / 2, radius * 2);
            go.GetComponent<Renderer>().sharedMaterial=material; RemoveCollider(go); return go;
        }
        static void RemoveCollider(GameObject go) { var c=go.GetComponent<Collider>(); if (c != null) UnityEngine.Object.DestroyImmediate(c); }
        static void Beam(Transform parent, string name, Vector3 a, Vector3 b, float width, Material mat)
        {
            Vector3 v = b-a; if (v.sqrMagnitude < 1e-8f) return;
            Box(parent,name,(a+b)*.5f,new Vector3(width, v.magnitude, width),mat).transform.localRotation=Quaternion.FromToRotation(Vector3.up,v.normalized);
        }
        static void Wheel(Transform parent, string name, Vector3 center, float radius, Material metal)
        {
            var hub=new GameObject(name); hub.transform.SetParent(parent,false); hub.transform.localPosition=center;
            const int slices=16;
            for (int i=0; i<slices; i++)
            {
                float a=2*Mathf.PI*i/slices, b=2*Mathf.PI*(i+1)/slices;
                var p=new Vector3(radius*Mathf.Cos(a),0,radius*Mathf.Sin(a));
                var q=new Vector3(radius*Mathf.Cos(b),0,radius*Mathf.Sin(b));
                Beam(hub.transform,"HANDWHEEL_RIM",p,q,.035f,metal);
            }
            for (int i=0;i<4;i++)
            {
                float a=i*Mathf.PI*.5f;
                Beam(hub.transform,"HANDWHEEL_SPOKE",Vector3.zero,new Vector3(radius*Mathf.Cos(a),0,radius*Mathf.Sin(a)),.025f,metal);
            }
            Cylinder(hub.transform,"HANDWHEEL_HUB",Vector3.zero,Vector3.up,.055f,.07f,metal);
        }
        static void Valve(Transform parent, string name, Vector3 p, Vector3 axis, float diameter, Material body, Material metal)
        {
            var root=new GameObject(name); root.transform.SetParent(parent,false);
            Cylinder(root.transform,"VALVE_CASING",p,axis,diameter*.86f,diameter*2.0f,body);
            Cylinder(root.transform,"VALVE_END_A",p-axis*diameter*.92f,axis,diameter*.72f,.045f,metal);
            Cylinder(root.transform,"VALVE_END_B",p+axis*diameter*.92f,axis,diameter*.72f,.045f,metal);
            Cylinder(root.transform,"STEM",p+Vector3.up*diameter*1.28f,Vector3.up,.045f,diameter*1.40f,metal);
            Wheel(root.transform,"HANDWHEEL",p+Vector3.up*diameter*2.02f,diameter*1.05f,body);
        }
        static void Flange(Transform parent, string name, Vector3 p, Vector3 axis, float diameter, Material metal)
        {
            var go=new GameObject(name); go.transform.SetParent(parent,false);
            Cylinder(go.transform,"FLANGE_DISC",p,axis,diameter*.85f,.085f,metal);
            Vector3 u = Vector3.Cross(axis.normalized,Vector3.up);
            if (u.sqrMagnitude < .01f) u=Vector3.Cross(axis.normalized,Vector3.right);
            u.Normalize(); Vector3 v=Vector3.Cross(axis.normalized,u).normalized;
            for(int i=0;i<8;i++)
            {
                float a=2*Mathf.PI*i/8;
                Vector3 bp=p+(Mathf.Cos(a)*u+Mathf.Sin(a)*v)*diameter*.72f;
                Cylinder(go.transform,"BOLT_"+i,bp,axis,.023f,.13f,metal);
            }
        }
        static void TeeMarker(Transform parent, string name, Vector3 p, float diameter, Material metal)
        {
            var root=new GameObject(name); root.transform.SetParent(parent,false);
            Cylinder(root.transform,"TEE_HEADER_SLEEVE",p,Vector3.forward,diameter*.57f,diameter*1.8f,metal);
            Cylinder(root.transform,"TEE_BRANCH_SLEEVE",p,Vector3.right,diameter*.55f,diameter*1.65f,metal);
        }
        static void Pump(Transform parent, Vector3 inlet, Vector3 outlet, Material steel, Material pump, Material fitting)
        {
            var go=new GameObject("P-101_CENTRIFUGAL_PUMP"); go.transform.SetParent(parent,false);
            Vector3 center=(inlet+outlet)*.5f;
            Box(go.transform,"BASE_PLATE",new Vector3(center.x,.22f,center.z),new Vector3(3.4f,.30f,1.9f),steel);
            Cylinder(go.transform,"PUMP_CASING",center+new Vector3(-.35f,0,0),Vector3.right,.53f,1.05f,pump);
            Cylinder(go.transform,"DRIVE_MOTOR",center+new Vector3(.53f,0,0),Vector3.right,.44f,.86f,fitting);
            Box(go.transform,"PUMP_PEDESTAL",new Vector3(center.x-.3f,.42f,center.z),new Vector3(.65f,.25f,.65f),steel);
            Box(go.transform,"MOTOR_PEDESTAL",new Vector3(center.x+.53f,.43f,center.z),new Vector3(.65f,.25f,.60f),steel);
            Cylinder(go.transform,"INLET_NOZZLE",inlet+Vector3.right*.16f,Vector3.right,.21f,.35f,pump);
            Cylinder(go.transform,"OUTLET_NOZZLE",outlet-Vector3.right*.16f,Vector3.right,.17f,.35f,pump);
            Flange(go.transform,"PUMP_SUCTION_FLANGE",inlet,Vector3.right,.42f,fitting);
            Flange(go.transform,"PUMP_DISCHARGE_FLANGE",outlet,Vector3.right,.34f,fitting);
        }
        static PipeRecord Record(string id, string system, float diameter, params Vector3[] points)
        {
            return new PipeRecord { id=id, system=system, outerDiameterM=diameter,
                wallThicknessM=diameter*.09f, bendRadiusM=Mathf.Max(.28f,diameter*1.8f), waypoints=points, valve=false };
        }
        static void Add(Transform parent, PipeRecord p, Material material, Material fitting, Material valve,
                        List<Connection> connections, string from, string to)
        {
            MarinePipeSceneBuilder.AddPipe(parent,p,material,fitting,valve);
            connections.Add(new Connection(from,to,p.waypoints[0],p.waypoints[p.waypoints.Length-1]));
        }
        public static GameObject Build(out string report)
        {
            var old=GameObject.Find(RootName); if(old!=null) UnityEngine.Object.DestroyImmediate(old);
            // Retain previous demo for comparison; hide it to avoid overlapping geometry.
            var v1=GameObject.Find(MarinePipeSceneBuilder.RootName); if(v1!=null) v1.SetActive(false);
            var root=new GameObject(RootName);
            var steel=Mat("V2_structure",Steel); var sea=Mat("V2_seawater",Sea);
            var supply=Mat("V2_discharge",Supply); var fittings=Mat("V2_fittings",Silver);
            var valve=Mat("V2_valve",Red); var pumpMat=Mat("V2_pump",new Color(.21f,.49f,.51f));
            Box(root.transform,"DECK",new Vector3(0,-.14f,0),new Vector3(29,.28f,17),steel);
            for(int x=-10;x<=10;x+=5)
            {
                for(int side=-1;side<=1;side+=2)
                    Box(root.transform,"RACK_POST",new Vector3(x,2.05f,side*6.1f),new Vector3(.16f,4.1f,.16f),steel);
                Beam(root.transform,"HEADER_SUPPORT",new Vector3(x,3.0f,-6.1f),new Vector3(x,3.0f,6.1f),.14f,steel);
            }
            var pumpIn=new Vector3(-7.8f,.95f,-2f);
            var pumpOut=new Vector3(-6.2f,.95f,-2f);
            Pump(root.transform,pumpIn,pumpOut,steel,pumpMat,fittings);
            Vector3 intake=new Vector3(-12f,.95f,-2f), beforePump=new Vector3(-10.1f,.95f,-2f);
            var connections=new List<Connection>();
            Add(root.transform,Record("SW-001_INTAKE","SeaWater",.42f,intake,beforePump),sea,fittings,valve,connections,"SEA_INTAKE","SUCTION_VALVE");
            Add(root.transform,Record("SW-002_SUCTION","SeaWater",.42f,beforePump,pumpIn),sea,fittings,valve,connections,"SUCTION_VALVE","PUMP_IN");
            Valve(root.transform,"XV-101_SUCTION_ISOLATION",beforePump,Vector3.right,.42f,valve,fittings);
            // Pump is modeled as a graph edge rather than an empty visual gap.
            connections.Add(new Connection("PUMP_IN","PUMP_OUT",pumpIn,pumpOut));
            Vector3 d1=new Vector3(-4.4f,.95f,-2f), d2=new Vector3(-4.4f,3f,-2f), d3=new Vector3(-1f,3f,-2f);
            Add(root.transform,Record("SW-003_DISCHARGE","CoolingSupply",.34f,pumpOut,d1,d2,d3),supply,fittings,valve,connections,"PUMP_OUT","HEADER_T2");
            Valve(root.transform,"NRV-101_DISCHARGE_CHECK_VISUAL",new Vector3(-5.5f,.95f,-2f),Vector3.right,.34f,valve,fittings);
            Vector3[] head={new Vector3(-1f,3f,-6f),new Vector3(-1f,3f,-4.5f),d3,new Vector3(-1f,3f,0f),new Vector3(-1f,3f,4.5f),new Vector3(-1f,3f,6f)};
            string[] labels={"HEADER_T0","HEADER_T1","HEADER_T2","HEADER_T3","HEADER_T4","HEADER_T5"};
            
for (int i = 0; i < head.Length - 1; i++)
{
    bool isLeftOfSupply = i < 2;

    Vector3 start = isLeftOfSupply ? head[i + 1] : head[i];
    Vector3 end = isLeftOfSupply ? head[i] : head[i + 1];

    string fromNode = isLeftOfSupply ? labels[i + 1] : labels[i];
    string toNode = isLeftOfSupply ? labels[i] : labels[i + 1];

    Add(
        root.transform,
        Record(
            "HDR-" + (i + 1).ToString("00"),
            "CoolingSupply",
            .34f,
            start,
            end
        ),
        supply,
        fittings,
        valve,
        connections,
        fromNode,
        toNode
    );
}

            for(int i=0;i<3;i++)
            {
                int headIndex=i==0?1:i==1?3:4;
                Vector3 junction=head[headIndex];
                Vector3 valveNode=new Vector3(2.3f,3f,junction.z);
                Vector3 down=new Vector3(8f,3f,junction.z);
                Vector3 terminal=new Vector3(8f,1.2f,junction.z);
                Add(root.transform,Record("BR-"+(i+1).ToString("00")+"A","CoolingSupply",.22f,junction,valveNode),supply,fittings,valve,connections,labels[headIndex],"BR_VALVE_"+i);
                Add(root.transform,Record("BR-"+(i+1).ToString("00")+"B","CoolingSupply",.22f,valveNode,down,terminal),supply,fittings,valve,connections,"BR_VALVE_"+i,"CONSUMER_"+i);
                Valve(root.transform,"XV-20"+(i+1)+"_BRANCH_ISOLATION",valveNode,Vector3.right,.22f,valve,fittings);
                TeeMarker(root.transform,"TEE_"+(i+1),junction,.34f,fittings);
                Box(root.transform,"CONSUMER_"+(i+1)+"_SKID",new Vector3(9.0f,.4f,junction.z),new Vector3(2.3f,.8f,1.2f),pumpMat);
                Flange(root.transform,"CONSUMER_"+(i+1)+"_PORT",terminal,Vector3.up,.22f,fittings);
            }
            TeeMarker(root.transform,"TEE_PUMP_HEADER",d3,.34f,fittings);
            // Add in-scene inspection hints through Hierarchy names, not floating text covering pipes.
            report=Validate(connections);
            return root;
        }
        static string Validate(List<Connection> edges)
        {
            var nodes=new Dictionary<string,Vector3>(); var adj=new Dictionary<string,List<string>>();
            int failures=0;
            foreach(var e in edges)
            {
                if ((e.end-e.start).magnitude < Tol) failures++;
                if(nodes.ContainsKey(e.from) && Vector3.Distance(nodes[e.from],e.start)>Tol) failures++; else nodes[e.from]=e.start;
                if(nodes.ContainsKey(e.to) && Vector3.Distance(nodes[e.to],e.end)>Tol) failures++; else nodes[e.to]=e.end;
                if(!adj.ContainsKey(e.from)) adj[e.from]=new List<string>();
                adj[e.from].Add(e.to);
            }
            var reached=new HashSet<string>();var q=new Queue<string>();q.Enqueue("SEA_INTAKE");reached.Add("SEA_INTAKE");
            while(q.Count>0){string a=q.Dequeue(); if(!adj.ContainsKey(a))continue;foreach(string b in adj[a])if(reached.Add(b))q.Enqueue(b);}
            
int connectedConsumers = 0;

for (int i = 0; i < 3; i++)
{
    string consumerId = "CONSUMER_" + i;

    if (reached.Contains(consumerId))
    {
        connectedConsumers++;
        Debug.Log("[CONNECTED] " + consumerId);
    }
    else
    {
        failures++;
        Debug.LogWarning("[NOT CONNECTED] " + consumerId);
    }
}

            return "V2 connectivity check: "+edges.Count+" directed connections, "+nodes.Count+" nodes, "+connectedConsumers+"/3 consumers reachable; "+(failures==0?"PASS":"FAIL ("+failures+" issue(s))")+". Visual topology only; no hydraulic simulation or boolean Tee union.";
        }
    }
}
