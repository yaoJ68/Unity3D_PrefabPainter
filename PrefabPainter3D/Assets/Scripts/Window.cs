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
    private bool orientToSurface;
    private GUIStyle guiStyle = new GUIStyle();
    private UnityEngine.Object[] allPrefabs;
    private Vector2 presetsScrollPos;

    // draw the window
    [MenuItem("Window/PrefabPainter")]
    public static void ShowWindow(){
        GetWindow<Window>("Prefab Painter");
    }

    private void OnEnable()
    {
        // load all prefabs in the window
        allPrefabs = Resources.LoadAll("", typeof(GameObject));
        layerName = "Default";
        SceneView.onSceneGUIDelegate -= CustomeUpdate;
        SceneView.onSceneGUIDelegate += CustomeUpdate;
    }

    private void OnGUI()
    {
        Event e = Event.current;
        // draw title
        guiStyle.fontSize = 20;
        guiStyle.alignment = TextAnchor.MiddleCenter;
        guiStyle.fontStyle = FontStyle.Bold;
        GUILayout.Label("Prefab Painter", guiStyle);
        GUILayout.Space(20);

        // field for displaying all the prefabs in the project
        EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        for (int i = 0; i < allPrefabs.Length; i++){
            GUILayout.Toggle(false, AssetPreview.GetAssetPreview(allPrefabs[i]), "button");

            //EditorGUI.DrawPreviewTexture(new Rect(i*20, 10, 20,20), AssetPreview.GetAssetPreview(allPrefabs[i]));
        }
        GUILayout.EndHorizontal();

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
      
        // check box for object orientation
        //orientToSurface = EditorGUILayout.Toggle("Orient to Surface", orientToSurface);
        //GUILayout.Space(10);

        // draw paint buttons
        GUILayout.BeginHorizontal();
        isPainting = GUILayout.Toggle(isPainting, "Paint", "button");
        isPainting &= (!GUILayout.Button("Cancel") && (!e.isKey || e.keyCode != KeyCode.Escape));
        GUILayout.EndHorizontal();
        GUILayout.Space(20);

        // input for grouping
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

        // detecting clicking or dragging
        if (e.type == EventType.MouseDown && e.button ==0 && isPainting){
            RaycastHit hit;
            Tools.current = Tool.View;
            int layer = 1 << LayerMask.NameToLayer(layerName);

            // create object(s) when raycast hit a ocject with collider

            if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out hit, Mathf.Infinity, layer))
            {
                // if the user does not select any prefab to paint, do nothing; else, paint.
                try{
                    // set the created object orientation
                    Vector3 surfaceDirection = hit.normal;
                    Quaternion orientation = orientToSurface ? Quaternion.LookRotation(surfaceDirection.normalized) : Quaternion.identity;

                    GameObject placedObject = Instantiate(firstPrefab, hit.point, orientation);
                    placedObject.transform.localScale = new Vector3(1, 1, 1);

                    // change name of the painted prefab
                    if (prefabName.Length != 0)
                    {
                        placedObject.name = prefabName;
                    }
                    // set parent of the painted prefab if it is not null
                    if (parent != null){
                        placedObject.transform.parent = parent.transform;
                    }
                    // added undo function to the painting
                    Undo.RegisterCreatedObjectUndo(placedObject, "Undo painting");

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
