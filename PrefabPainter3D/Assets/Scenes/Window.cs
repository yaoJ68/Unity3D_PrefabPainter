using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class Window : EditorWindow {
    private GameObject firstPrefab;
    public Font myFont;
    private string layerName;
    private string prefabName;
    private string groupName;
    private GameObject parent;
    private bool isPainting;
    private GUIStyle guiStyle = new GUIStyle();

    // draw the window
    [MenuItem("Window/PrefabPainter")]
    public static void ShowWindow(){
        GetWindow<Window>("Prefab Painter");
    }

    private void OnEnable()
    {
        layerName = "Default";
        SceneView.onSceneGUIDelegate -= CustomeUpdate;
        SceneView.onSceneGUIDelegate += CustomeUpdate;
    }

    private void OnGUI()
    {
        // draw title
        guiStyle.fontSize = 20;
        guiStyle.alignment = TextAnchor.MiddleCenter;
        guiStyle.fontStyle = FontStyle.Bold;
        GUILayout.Label("Prefab Painter", guiStyle);
        GUILayout.Space(20);

        // draw paint buttons
        Event e = Event.current;
        GUILayout.BeginHorizontal();
        isPainting = GUILayout.Toggle(isPainting, "Paint", "button");
        isPainting &= (!GUILayout.Button("Cancel") && (!e.isKey || e.keyCode != KeyCode.Escape));
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        // input field for setting the name of the painted prefab
        prefabName = EditorGUILayout.TextField("Prefab Name", prefabName);

        // input field for layer name
        layerName = EditorGUILayout.TextField("Layer", layerName);
        if (string.IsNullOrEmpty(layerName))
            layerName = "Default";  // change the layer name to default if user does not type in anything

        // input field for pregab
        GUILayout.BeginHorizontal();
        GUILayout.Label("Prefab");
        firstPrefab = (GameObject)EditorGUILayout.ObjectField(firstPrefab, typeof(GameObject), true);
        GUILayout.EndHorizontal();

        //input field for setting parent of the new painted object
        GUILayout.BeginHorizontal();
        GUILayout.Label("Parent");
        parent = (GameObject)EditorGUILayout.ObjectField(parent, typeof(GameObject), true);
        GUILayout.EndHorizontal();
        GUILayout.Space(20);

        groupName = EditorGUILayout.TextField("Group Name", groupName);
        if (string.IsNullOrEmpty(groupName)){
            groupName = "New Group";
        }
        if (GUILayout.Button("Group")){
            Group();
        }
    }

   

    void CustomeUpdate(UnityEditor.SceneView sv){
        Event e = Event.current;

        // Creating the object when clicking or dragging
        int layer = ~LayerMask.NameToLayer(layerName);  // the layer that the painted object is on

        if ((e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && e.button ==0 && isPainting){
            RaycastHit hit;
            Tools.current = Tool.View;

            if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out hit, Mathf.Infinity, layer))
            {
                // if the user does not select any prefab to paint, do nothing; else, paint.
                try{
                    // exception will be thrown if the prefab is null
                    GameObject placedObejct = PrefabUtility.InstantiatePrefab(firstPrefab as GameObject) as GameObject;
                    placedObejct.transform.position = hit.point;
                    placedObejct.transform.localScale = new Vector3(1, 1, 1);

                    // change name of the painted prefab
                    if (prefabName.Length != 0)
                    {
                        placedObejct.name = prefabName;
                    }
                    // set parent of the painted prefab if it is not null
                    if (parent != null){
                        placedObejct.transform.parent = parent.transform;
                    }
                    // added undo function to the painting
                    Undo.RegisterCreatedObjectUndo(placedObejct, "Undo painting");

                }catch(Exception){
                    return;
                }
            }
            e.Use();
        }
    }


    private void Group(){
        // Group obejcts together
        if (!Selection.activeTransform)
            return;

        // if there is a object with the same name, group under that object; else, create new parent
        var groupParent = GameObject.Find(groupName);
        if (groupParent == null){
            groupParent = new GameObject(groupName);
            Undo.RegisterCreatedObjectUndo(groupParent, "Group Selected");
            groupParent.transform.SetParent(Selection.activeTransform.parent, false);
        }

        foreach (var transform in Selection.transforms) 
            Undo.SetTransformParent(transform, groupParent.transform, "Group Selected");

        Selection.activeGameObject = groupParent;
    }
}
