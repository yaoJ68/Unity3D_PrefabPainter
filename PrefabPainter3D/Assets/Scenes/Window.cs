using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Window : EditorWindow {
    private GameObject firstPrefab;
    public Font myFont;
    private string LayerName = "Default";
    private bool isPainting;
    private GUIStyle guiStyle = new GUIStyle();

    // draw the window
    [MenuItem("Window/PrefabPainter")]
    public static void ShowWindow(){
        GetWindow<Window>("Prefab Painter");
    }

    private void OnGUI()
    {
        guiStyle.fontSize = 20;
        guiStyle.alignment = TextAnchor.MiddleCenter;
        guiStyle.fontStyle = FontStyle.Bold;
        GUILayout.Label("Prefab Painter", guiStyle);

        Event e = Event.current;
        GUILayout.Space(20);

        // paint field for pregab
        GUILayout.Label("Prefab");
        firstPrefab = (GameObject)EditorGUILayout.ObjectField(firstPrefab, typeof(GameObject), true);
        GUILayout.Space(20);

        // paint text field for layer name
        LayerName = EditorGUILayout.TextField("Layer Name", LayerName);
        if (LayerName.Length == 0){
            LayerName = "Default";  // change the layer name to default if user does not type in anything
        }
        GUILayout.Space(20);

        // paint buttons
        GUILayout.BeginHorizontal();
        isPainting = GUILayout.Toggle(isPainting, "Paint", "button");
        isPainting &= (!GUILayout.Button("Cancel") && (!e.isKey || e.keyCode != KeyCode.Escape));
        GUILayout.EndHorizontal();
    }

    private void OnEnable()
    {
        SceneView.onSceneGUIDelegate -= CustomeUpdate;
        SceneView.onSceneGUIDelegate += CustomeUpdate;
    }

    void CustomeUpdate(UnityEditor.SceneView sv){
        Event e = Event.current;

        // make the raycast only collides with the selected layer
        int layer = ~LayerMask.NameToLayer(LayerName);

        if (e.type == EventType.MouseDrag || e.type == EventType.MouseDown && e.button ==0 && isPainting){
            RaycastHit hit;
            Tools.current = Tool.View;

            if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out hit, Mathf.Infinity, layer))
            {
                // if the user does not select any prefab to paint, do nothing; else, paint.
                if (firstPrefab == null){
                    return;
                }else{
                    GameObject placedObejct = PrefabUtility.InstantiatePrefab(firstPrefab as GameObject) as GameObject;
                    placedObejct.transform.position = hit.point;
                    placedObejct.transform.localScale = new Vector3(1, 1, 1);

                    e.Use();

                    Undo.RegisterCreatedObjectUndo(placedObejct, "undo painting");
                }
            }
        }
    }
}
