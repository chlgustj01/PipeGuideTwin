// Optional Unity Splines integration. Compile ONLY after installing com.unity.splines
// and adding MARINE_PIPE_USE_SPLINES to Project Settings > Player > Scripting Define Symbols.
#if MARINE_PIPE_USE_SPLINES
using System;
using MarinePipe;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public static class MarinePipeSplinesBridge
{
    [MenuItem("Tools/Marine Pipe/03 - Selected Pipe to Editable Spline")]
    public static void ConvertSelected()
    {
        var selected = Selection.activeGameObject;
        var path = selected == null ? null : selected.GetComponent<MarinePipePath>();
        if (path == null) { EditorUtility.DisplayDialog("Marine Pipe", "Select one generated Pipe GameObject first.", "OK"); return; }
        var points = MarinePipeGeometry.Fillet(path.waypoints, path.bendRadiusM, 16);
        var old = selected.transform.Find("EditableSplineCenterline");
        if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
        var child = new GameObject("EditableSplineCenterline");
        child.transform.SetParent(selected.transform, false);
        var c = child.AddComponent<SplineContainer>();
        var s = c.Spline;
        s.Clear();
        foreach (var p in points) s.Add((float3)p, TangentMode.Linear);
        Undo.RegisterCreatedObjectUndo(child,"Add editable marine pipe spline");
        EditorUtility.SetDirty(c);
        EditorSceneManager.MarkSceneDirty(selected.scene);
        Selection.activeGameObject = child;
        Debug.Log("Created editable spline with " + points.Count + " knots. Note: modifying knots does NOT yet rebuild pipe mesh.");
    }
}
#endif
