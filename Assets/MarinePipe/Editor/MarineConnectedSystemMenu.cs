using MarinePipe;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MarineConnectedSystemMenu
{
    [MenuItem("Tools/Marine Pipe/04 - Build Connected Pump Header Demo")]
    public static void Build()
    {
        string report;
        var root=MarineConnectedSystemBuilder.Build(out report);
        Undo.RegisterCreatedObjectUndo(root,"Build marine connected piping V2");
        Selection.activeGameObject=root;
        var view=SceneView.lastActiveSceneView;
        if(view!=null) {view.pivot=new Vector3(-1f,1.5f,-.5f);view.rotation=Quaternion.Euler(27f,136f,0f);view.size=19f;view.Repaint();}
        var camera=GameObject.Find("MarinePipePreviewCamera");
        if(camera!=null){camera.transform.position=new Vector3(19f,15f,19f);camera.transform.LookAt(new Vector3(-1f,1.5f,0f));}
        EditorSceneManager.MarkSceneDirty(root.scene);
        Debug.Log(report);
        EditorUtility.DisplayDialog("Connected piping V2",report+"\nSave your Scene with Ctrl+S.","OK");
    }
}
