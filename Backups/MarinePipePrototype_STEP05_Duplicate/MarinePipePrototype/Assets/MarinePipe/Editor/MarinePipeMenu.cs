using System;
using System.IO;
using MarinePipe;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MarinePipeMenu
{
    [MenuItem("Tools/Marine Pipe/01 - Build Industrial Pipe Demo")]
    public static void BuildDemo()
    {
        var root = MarinePipeSceneBuilder.BuildDemo(24);
        SetupCameraAndLight(root.transform);
        Selection.activeGameObject = root;
        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.pivot = new Vector3(0f,2.5f,0f);
            SceneView.lastActiveSceneView.rotation = Quaternion.Euler(24f, 132f, 0f);
            SceneView.lastActiveSceneView.size = 26f;
            SceneView.lastActiveSceneView.Repaint();
        }
        EditorSceneManager.MarkSceneDirty(root.scene);
        Debug.Log("Marine Pipe: created 79 hollow pipe lines (72 rack + 7 headers), valves, flanges, supports and pump skids. Save Scene with Ctrl+S.");
    }

    [MenuItem("Tools/Marine Pipe/02 - Import JSON Database")]
    public static void ImportJson()
    {
        string file = EditorUtility.OpenFilePanel("Select Marine Pipe JSON database",Application.dataPath,"json");
        if (string.IsNullOrEmpty(file)) return;
        try
        {
            var data = JsonUtility.FromJson<PipeDatabase>(File.ReadAllText(file));
            if (data == null || data.pipes == null || data.pipes.Length == 0) throw new ArgumentException("JSON must contain nonempty pipes array");
            GameObject root = GameObject.Find("MARINE_PIPE_IMPORTED");
            if (root != null) UnityEngine.Object.DestroyImmediate(root);
            root = new GameObject("MARINE_PIPE_IMPORTED");
            int count=0;
            foreach (var pipe in data.pipes)
            {
                MarinePipeSceneBuilder.AddPipe(root.transform,pipe);
                count++;
            }
            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(root.scene);
            Debug.Log("Marine Pipe: imported " + count + " pipe(s). Save your Scene.");
        }
        catch (Exception ex)
        {
            Debug.LogError("Marine Pipe JSON import failed: " + ex.Message);
            EditorUtility.DisplayDialog("Marine Pipe import failed",ex.ToString(),"OK");
        }
    }

    static void SetupCameraAndLight(Transform parent)
    {
        if (GameObject.Find("MarinePipePreviewCamera") == null)
        {
            var go=new GameObject("MarinePipePreviewCamera");
            go.transform.position=new Vector3(25f,18f,25f);
            go.transform.LookAt(new Vector3(0,2.8f,0));
            var cam=go.AddComponent<Camera>();
            cam.fieldOfView=49f;
            cam.nearClipPlane=.1f;
            cam.farClipPlane=240f;
            cam.clearFlags=CameraClearFlags.Skybox;
            go.tag="MainCamera";
        }
        if (GameObject.Find("MarinePipeSun") == null)
        {
            var go=new GameObject("MarinePipeSun");
            go.transform.rotation=Quaternion.Euler(42,-27,0);
            var l=go.AddComponent<Light>(); l.type=LightType.Directional; l.intensity=1.3f;
        }
    }
}
