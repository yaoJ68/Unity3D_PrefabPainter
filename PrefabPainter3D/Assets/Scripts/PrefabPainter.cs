using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class PrefabPainter : EditorWindow {
    // project settings
    private bool is2D;

    // preset settings
    private string layerName;
    private string presetName;
    private string groupName;
    private bool isPainting;
    private bool orientToSurface;
    private Vector3 scaling;
    private Vector3 rotation;
    private GameObject parent;
    private float spaceBetween;

    // gui settings
    private Vector2 windowScrollPos;
    private Vector2 presetsScrollPos;
    private bool showWindow;
    private Vector3 lastObejctPos;
    private bool startDrag;



    // draw the window
    [MenuItem("Window/PrefabPainter")]
    public static void ShowWindow(){
        GetWindow<PrefabPainter>("Prefab Painter");
    }

    private void OnEnable()
    {
        scaling = Vector3.one;
        rotation = Vector3.zero;
        showWindow = true;
        layerName = "Default";  // set layer to default on initialization
        presetName = Settings.GetSelectedName();
        SceneView.onSceneGUIDelegate -= CustomeUpdate;
        SceneView.onSceneGUIDelegate += CustomeUpdate;
    }

    private void OnGUI()
    {
        windowScrollPos = EditorGUILayout.BeginScrollView(windowScrollPos, false, false);   // if the window size is too small, add scroll bar
        Event e = Event.current;
        // draw title
        GUILayout.Label("Prefab Painter", Styles.titleStyle);
        GUILayout.Space(20);

        // buttons for choosing project template
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        is2D = GUILayout.Toggle(is2D, "2D Project", "button", GUILayout.Width(position.width / 3),GUILayout.Height(30));
        GUILayout.FlexibleSpace();
        is2D = !GUILayout.Toggle(!is2D, "3D Project", "button", GUILayout.Width(position.width / 3), GUILayout.Height(30));
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);

        {
            // draw area to display all presets
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);
            if(GUILayout.Button("Delete")){
                Settings.DeletePreset();
            }
            EditorGUILayout.EndHorizontal();


            // draw scroll field for displaying all the prefabs in the project
            int windowBorder = 2;


            Rect realRect = EditorGUILayout.GetControlRect(GUILayout.Height(Styles.presetIconHeight * Styles.presetWindowRows + windowBorder));
            Rect virtualRect = new Rect(realRect);

            {
                virtualRect.width = Mathf.Max(virtualRect.width - 20, 1); // space for scroll 

                int presetColumns = Mathf.FloorToInt(Mathf.Max(1, (virtualRect.width - windowBorder * 2) / Styles.presetIconWidth));
                int virtualRows = Mathf.CeilToInt((float)Settings.allPresets.Count/ presetColumns);

                virtualRect.height = Mathf.Max(virtualRect.height, Styles.presetIconHeight * virtualRows + windowBorder);
            }

            presetsScrollPos = GUI.BeginScrollView(realRect, presetsScrollPos, virtualRect, false, true);

            // Empty preset list - Drag&Drop Info
            if (Settings.allPresets.Count == 0)
            {
                GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                EditorGUI.LabelField(realRect, "Drag & Drop Prefab Here", labelStyle);
            }

            int presetIndex = 0;
            int iconDrawCount = 0;
            int presetUnderCursor = -1;


            for (int y = (int)virtualRect.yMin + windowBorder; y < (int)virtualRect.yMax; y += Styles.presetIconHeight)
            {
                if (presetIndex >= Settings.allPresets.Count)
                    break;

                for (int x = (int)virtualRect.xMin + windowBorder; (x + Styles.presetIconWidth) < (int)(virtualRect.xMax); x += Styles.presetIconWidth)
                {
                    if (presetIndex >= Settings.allPresets.Count)
                        break;


                    Rect presetIconRectScrolled = new Rect(x - presetsScrollPos.x, y - presetsScrollPos.y, Styles.presetIconWidth, Styles.presetIconHeight);

                    // only visible incons
                    if (presetIconRectScrolled.Overlaps(realRect))
                    {
                        Rect presetIconRect = new Rect(x, y, Styles.presetIconWidth, Styles.presetIconHeight);

                        // detect the selected prefab
                        if (presetIconRect.Contains(e.mousePosition))
                            presetUnderCursor = presetIndex;

                        iconDrawCount++;


                        // Draw all Prefab preview
                        EditorGUI.DrawRect(new Rect(presetIconRect.x, presetIconRect.y, 0, 0), Styles.colorBlue);

                        // detect selected preset
                        if (presetUnderCursor == presetIndex){
                            if (e.type == EventType.MouseDown && e.button == 0){
                                Settings.selectedIndex = presetUnderCursor;
                                presetName = Settings.GetSelectedName();
                            }
                            else{
                                EditorGUI.DrawRect(presetIconRect, Color.gray);
                            }
                        }
                        if (Settings.selectedIndex == presetIndex)
                            EditorGUI.DrawRect(presetIconRect, Styles.colorBlue);


                        Rect iconRect = new Rect(x + 1, y + 1, Styles.presetIconWidth - 2, Styles.presetIconWidth - 2);

                        // Prefab preview
                        Texture2D presetPreview = AssetPreview.GetAssetPreview(Settings.allPresets[presetIndex]);

                        if ( presetPreview!= null)
                        {
                            GUI.DrawTexture(iconRect, presetPreview);
                        }
                        else
                        {
                            // Missing prefab
                            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
                            labelStyle.normal.textColor = Color.red;
                            labelStyle.alignment = TextAnchor.LowerCenter;
                            EditorGUI.LabelField(presetIconRect, "Missing", labelStyle);

                            labelStyle = new GUIStyle(EditorStyles.miniLabel)
                            {
                                alignment = TextAnchor.MiddleCenter
                            };
                            EditorGUI.LabelField(iconRect, "Image not\navaiable", labelStyle);
                            }
                    }

                    presetIndex++;
                }
            }


            // Drag & Drop
            if (e.type == EventType.DragUpdated || e.type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    foreach (UnityEngine.Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is GameObject &&
                            PrefabUtility.GetPrefabType(draggedObject as GameObject) != PrefabType.None &&
                            AssetDatabase.Contains(draggedObject))
                        {

                            if (!Settings.allPresets.Contains((GameObject)draggedObject)){
                                Settings.AddPreset(draggedObject as GameObject);
                            }

                        }
                    }

                }
                e.Use();
                Repaint();
            }
            GUI.EndScrollView();


            // reset button
            if (GUILayout.Button("Reset", GUILayout.Width(position.width/5))){
                layerName = "Default";
                parent = null;
                orientToSurface = false;
                scaling = new Vector3(1, 1, 1);
                rotation = new Vector3(0, 0, 0);
                spaceBetween = 1;
                Repaint();
            }

            // input field for setting the name of the painted prefab
            presetName = EditorGUILayout.TextField("Preset Name", presetName);


            // input field for layer name
            layerName = EditorGUILayout.TextField("Collision Layer", layerName);
            if (string.IsNullOrEmpty(layerName))
                layerName = "Default";  // change the layer name to default if user does not type in anything
                

            //input field for setting parent of the new painted object
            GUILayout.BeginHorizontal();

            GUILayout.Label("Hierarchy Parent");
            parent = (GameObject)EditorGUILayout.ObjectField(parent, typeof(GameObject), true);

            GUILayout.EndHorizontal();

           
            // check box for object orientation
            orientToSurface = EditorGUILayout.Toggle("Orient to Surface", orientToSurface);

            // slider bar to adjuste the space between objects when drag and paint
            GUILayout.BeginHorizontal();
            GUILayout.Label("Space Between Objects");
            spaceBetween = EditorGUILayout.Slider(spaceBetween, 1.0f, 100.0f);
            GUILayout.EndHorizontal();

            // foldout field for trasnform information
            showWindow = EditorGUILayout.Foldout(showWindow, "Transform");

            if (showWindow)
            {
                scaling = EditorGUILayout.Vector3Field("Scale", scaling);
                rotation = EditorGUILayout.Vector3Field("Rotation", rotation);

            }
            GUILayout.Space(5);

            // 'paint' and 'cancel' button
            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            isPainting = GUILayout.Toggle(isPainting, "Paint", "button", GUILayout.Width(position.width/3));
            GUILayout.FlexibleSpace();
            isPainting &= (!GUILayout.Button("Cancel", GUILayout.Width(position.width /3)) && (!e.isKey || e.keyCode != KeyCode.Escape));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(30);



            // input area for grouping
            EditorGUILayout.LabelField("Grouping Presets", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            groupName = EditorGUILayout.TextField("Group Name", groupName);

            if (string.IsNullOrEmpty(groupName))
            {
                groupName = "New Group";
            }
            if (GUILayout.Button("Group"))
            {
                Group();
            }
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }
   

    void CustomeUpdate(UnityEditor.SceneView sv){
        Event e = Event.current;

        // if mouse up, strop painting and return
        if (e.type == EventType.MouseUp){
            startDrag = false;
            return;
        }

        // if clicking or dragging, paint
        if ((e.type==EventType.MouseDrag || e.type == EventType.MouseDown) && e.button ==0 && isPainting){

            RaycastHit hit;
            Tools.current = Tool.View;
            int layer = 1 << LayerMask.NameToLayer(layerName);
            Vector3 spawnPosition = HandleUtility.GUIPointToWorldRay(e.mousePosition).origin; // initialize the swpwan position as the mouse position
            Quaternion orientation = Quaternion.identity;

            // if the porject is 3d, change settings when initialize objects
            if (!is2D)
            {
                if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(e.mousePosition), out hit, Mathf.Infinity, layer)){
                    Vector3 forward = Vector3.Cross(Vector3.right, hit.normal);
                    Quaternion surfaceDirection = Quaternion.LookRotation(forward, hit.normal);
                    orientation = orientToSurface ? surfaceDirection : Quaternion.identity; //change the quaternion setting when the obejct should be oriented to surface
                    spawnPosition = hit.point;   // set the position of the object to raycast hit point

                }else{
                    return; // in 3D mode when not hitting a collider, return 
                }
              
            }

            // rotate the object
            Quaternion newDirection = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
            orientation.x += newDirection.x;
            orientation.y += newDirection.y;
            orientation.z += newDirection.z;


            // if the mouse is dragging, initialize the first object and every other objects after given space
            if (e.type == EventType.MouseDrag && !startDrag){
                startDrag = true;
            }else if (e.type == EventType.MouseDrag){
                // if the distance between current mouse position and last object position is smaller thant the given space
                // not paint and return
                if (Vector3.Distance(spawnPosition, lastObejctPos) < spaceBetween){
                    return;
                }
            }

            GameObject placedObject = (GameObject)Instantiate(Settings.GetSelectedPreset(), spawnPosition, orientation);
            lastObejctPos = placedObject.transform.position;

            // change size
            placedObject.transform.localScale = scaling;

           
            // change name of the painted prefab
            if (presetName.Length != 0)
            {
                placedObject.name = presetName;
            }

            // set parent of the painted prefab if it is not null
            if (parent != null)
            {
                placedObject.transform.parent = parent.transform;
            }

            // added undo function to the painting
            Undo.RegisterCreatedObjectUndo(placedObject, "Undo painting");

            e.Use();
        }

        // exit paiting mode when press escape
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape){
            isPainting = false;
            Repaint();
        }
    }


    // Group obejcts together
    private void Group(){
       
        if (!Selection.activeTransform)
            return;

        // if there is a object with the same name as the entered, group under this object
        // else, create a new parent and group under it
        var groupParent = GameObject.Find(groupName);

        if (groupParent == null){

            groupParent = new GameObject(groupName);
            Undo.RegisterCreatedObjectUndo(groupParent, "Group Selected");
            groupParent.transform.SetParent(Selection.activeTransform.parent, false);
        }

        // register undo for grouping a single/ as set of objects
        foreach (var transform in Selection.transforms) {
            Undo.SetTransformParent(transform, groupParent.transform, "Group Selected");
        }

        Selection.activeGameObject = groupParent;
    }
}
