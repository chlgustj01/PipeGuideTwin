using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarinePipe
{
    public static class MarinePipeSceneBuilder
    {
        public const string RootName = "MARINE_PIPE_DEMO";
        static readonly Color[] Palette = {
            new Color(.25f,.69f,.90f), new Color(.96f,.66f,.20f), new Color(.35f,.81f,.55f),
            new Color(.80f,.48f,.87f), new Color(.91f,.38f,.39f), new Color(.75f,.80f,.85f)
        };

        static Material MakeMat(string name, Color color, bool metallic = true)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Diffuse");
            var mat = new Material(shader) { name = name, color = color };
            if (metallic && mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", .65f);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", .62f);
            return mat;
        }

        static GameObject Box(Transform parent, string name, Vector3 position, Vector3 scale, Material mat)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name; obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position; obj.transform.localScale = scale;
            var collider = obj.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
            obj.GetComponent<Renderer>().sharedMaterial = mat;
            return obj;
        }

        static GameObject Cylinder(Transform parent, string name, Vector3 position, Vector3 direction, float diameter, float thickness, Material mat)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obj.name = name; obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
            obj.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction.normalized);
            obj.transform.localScale = new Vector3(diameter, thickness * .5f, diameter);
            var collider = obj.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
            obj.GetComponent<Renderer>().sharedMaterial = mat;
            return obj;
        }

        public static GameObject BuildDemo(int pipesPerLevel = 24)
        {
            pipesPerLevel = Mathf.Clamp(pipesPerLevel, 4, 42);
            var old = GameObject.Find(RootName);
            if (old != null) UnityEngine.Object.DestroyImmediate(old);
            var root = new GameObject(RootName);
            var steel = MakeMat("Graphite structural steel", new Color(.17f,.23f,.30f));
            var deck = MakeMat("Steel deck", new Color(.27f,.32f,.35f));
            var fittings = MakeMat("Stainless fittings", new Color(.70f,.73f,.75f));
            var valveMat = MakeMat("Safety valve red", new Color(.90f,.27f,.17f));
            var pumpMat = MakeMat("Equipment teal", new Color(.14f,.43f,.51f));
            var colors = new Material[Palette.Length];
            for (int i = 0; i < colors.Length; i++) colors[i] = MakeMat("System_" + i, Palette[i]);

            Box(root.transform,"ENGINE_ROOM_DECK",new Vector3(0,-.15f,0),new Vector3(32,.3f,17),deck);
            // Three pipe tiers and an industrial steel support rack.
            for (int x = -14; x <= 14; x += 7)
            {
                foreach (int side in new[] {-1,1})
                    Box(root.transform,"RACK_COLUMN",new Vector3(x,3.4f,side*5.8f),new Vector3(.23f,6.8f,.23f),steel);
                foreach (float y in new[] {1.9f,3.5f,5.1f})
                    Box(root.transform,"RACK_CROSSBEAM",new Vector3(x,y,0),new Vector3(.22f,.2f,12f),steel);
                Box(root.transform,"TOP_BRACE",new Vector3(x,6.8f,0),new Vector3(.19f,.18f,12f),steel);
            }
            // Three stacked tiers, 24 lines each by default (72 total) with end offsets and elbows.
            for (int tier = 0; tier < 3; tier++)
            for (int i = 0; i < pipesPerLevel; i++)
            {
                int system = (i + tier * 2) % colors.Length;
                float z = -5.1f + (10.2f * (i + .5f) / pipesPerLevel);
                float y = 2.12f + tier * 1.6f;
                float diameter = i % 7 == 0 ? .24f : (i % 3 == 0 ? .15f : .09f);
                float rz = i % 2 == 0 ? .8f : -.8f;
                float dy = i % 5 == 0 ? .50f : .0f;
                var record = new PipeRecord {
                    id = string.Format("{0:00}-P-{1:000}",tier+1,i+1),
                    system = new[]{"Cooling", "Ballast", "FireMain", "Fuel", "SeaWater", "Air"}[system],
                    outerDiameterM = diameter,
                    wallThicknessM = diameter * .10f,
                    bendRadiusM = Mathf.Max(.18f,diameter*1.8f),
                    valve = i % 4 == 0,
                    waypoints = new [] {
                        new Vector3(-13.2f,y,z), new Vector3(-9.0f,y,z),
                        new Vector3(-7.1f,y+dy,z),new Vector3(5.7f,y+dy,z),
                        new Vector3(8.5f,y+dy,z+rz),new Vector3(13.1f,y+dy,z+rz)
                    }
                };
                AddPipe(root.transform,record,colors[system],fittings,valveMat);
            }
            // Main cross-system headers run perpendicular to the rack.
            for (int i = 0; i < 7; i++)
            {
                float x = -10.9f + i*3.6f;
                var record = new PipeRecord {
                    id = "HEADER-" + (i+1), system = "CrossHeader",outerDiameterM = .18f,
                    wallThicknessM = .018f,bendRadiusM = .36f,valve = i%2==0,
                    waypoints = new []{new Vector3(x,1.05f,-7.7f),new Vector3(x,1.05f,-6.7f),new Vector3(x,1.4f,6.7f),new Vector3(x,1.4f,7.7f)}
                };
                AddPipe(root.transform,record,colors[i%colors.Length],fittings,valveMat);
            }
            // Representative pump/equipment skids.
            for (int i=0; i<4; i++)
            {
                float x=-10.5f+i*6.6f;
                var equipment=new GameObject("PUMP_SKID_"+(i+1)); equipment.transform.SetParent(root.transform,false);
                equipment.transform.localPosition=new Vector3(x,0,6.7f);
                Box(equipment.transform,"BASE",new Vector3(0,.18f,0),new Vector3(3.5f,.36f,2.1f),steel);
                Cylinder(equipment.transform,"PUMP_CASING",new Vector3(-.65f,.8f,0),Vector3.right,1.1f,1.0f,pumpMat);
                Cylinder(equipment.transform,"MOTOR",new Vector3(.75f,.75f,0),Vector3.right,.9f,1.3f,fittings);
                Cylinder(equipment.transform,"SUCTION_FLANGE",new Vector3(-1.25f,.82f,0),Vector3.right,1.3f,.13f,fittings);
                Cylinder(equipment.transform,"DISCHARGE_FLANGE",new Vector3(0,1.35f,0),Vector3.up,.8f,.13f,fittings);
            }
            // Additional wall framing for depth cues, no intrusive floating text.
            for (int i = -14; i <= 14; i += 4)
                Box(root.transform,"BULKHEAD_STIFFENER",new Vector3(i,3.2f,-8.2f),new Vector3(.16f,6.4f,.16f),steel);
            return root;
        }

        public static GameObject AddPipe(Transform parent, PipeRecord record, Material pipeMaterial = null, Material fittingMaterial = null, Material valveMaterial = null)
        {
            if (record == null || record.waypoints == null || record.waypoints.Length < 2) throw new ArgumentException("Missing pipe path");
            pipeMaterial = pipeMaterial ?? MakeMat("Pipe",Palette[0]);
            fittingMaterial = fittingMaterial ?? MakeMat("Fitting",new Color(.75f,.75f,.78f));
            valveMaterial = valveMaterial ?? MakeMat("Valve",new Color(.9f,.29f,.17f));
            var go = new GameObject(record.id ?? "Pipe"); go.transform.SetParent(parent,false);
            var tag = go.AddComponent<MarinePipeTag>(); tag.pipeId=record.id; tag.system=record.system; tag.outerDiameterM=record.outerDiameterM;
            var path = go.AddComponent<MarinePipePath>(); path.waypoints=record.waypoints; path.outerDiameterM=record.outerDiameterM; path.wallThicknessM=record.wallThicknessM; path.bendRadiusM=record.bendRadiusM;
            var centerline = MarinePipeGeometry.Fillet(record.waypoints,record.bendRadiusM,12);
            var filter=go.AddComponent<MeshFilter>();
            filter.sharedMesh=MarinePipeGeometry.SweepHollow(centerline,record.outerDiameterM*.5f,record.wallThicknessM,16);
            go.AddComponent<MeshRenderer>().sharedMaterial=pipeMaterial;
            var first=(record.waypoints[1]-record.waypoints[0]).normalized;
            var last=(record.waypoints[record.waypoints.Length-1]-record.waypoints[record.waypoints.Length-2]).normalized;
            float d=record.outerDiameterM;
            Cylinder(go.transform,"START_FLANGE",record.waypoints[0]+first*.09f,first,d*1.55f,.07f,fittingMaterial);
            Cylinder(go.transform,"END_FLANGE",record.waypoints[record.waypoints.Length-1]-last*.09f,last,d*1.55f,.07f,fittingMaterial);
            if (record.valve)
            {
                Vector3 point=Vector3.Lerp(record.waypoints[1],record.waypoints[2],.48f);
                Vector3 axis=(record.waypoints[2]-record.waypoints[1]).normalized;
                Cylinder(go.transform,"VALVE_BODY",point,axis,d*2.25f,d*2.35f,valveMaterial);
                Cylinder(go.transform,"VALVE_BONNET",point+Vector3.up*d*1.4f,Vector3.up,d*.8f,d*1.6f,fittingMaterial);
                Cylinder(go.transform,"HANDWHEEL",point+Vector3.up*d*2.3f,Vector3.up,d*2.3f,d*.2f,valveMaterial);
            }
            return go;
        }
    }
}
