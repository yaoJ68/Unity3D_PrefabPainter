using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class Window : EditorWindow {
    private string layerName;
    private string presetName;
    private string groupName;
    private GameObject parent;

    private bool isPainting;
    private bool orientToSurface;
    private Vector2 presetsScrollPos;
     

    // draw the window
    [MenuItem("Window/PrefabPainter")]
    public static void ShowWindow(){
        GetWindow<Window>("Prefab Painter");
    }

    private void OnEnable()
    {
        layerName = "Default";  // set layer to default on initialization
        SceneView.onSceneGUIDelegate -= CustomeUpdate;
        SceneView.onSceneGUIDelegate += CustomeUpdate;
    }

    private void OnGUI()
    {
        Event e = Event.current;

        // draw title
        GUILayout.Label("Prefab Painter", Styles.titleStyle);
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
                            EditorGUI.LabelField(iconRect, "Shift+Drag\nRelink", labelStyle);
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



            // input field for setting the name of the painted prefab
            presetName = EditorGUILayout.TextField("Preset Name", presetName);

            // input field for layer name
            layerName = EditorGUILayout.TextField("Layer", layerName);
            if (string.IsNullOrEmpty(layerName))
                layerName = "Default";  // change the layer name to default if user does not type in anything
                



            //input field for setting parent of the new painted object
            GUILayout.BeginHorizontal();

            GUILayout.Label("Parent");
            parent = (GameObject)EditorGUILayout.ObjectField(parent, typeof(GameObject), true);

            GUILayout.EndHorizontal();

            // check box for object orientation
            orientToSurface = EditorGUILayout.Toggle("Orient to Surface", orientToSurface);
            GUILayout.Space(5);


            // 'paint' button
            GUILayout.BeginHorizontal();
            isPainting = GUILayout.Toggle(isPainting, "Paint", "button");
            isPainting &= (!GUILayout.Button("Cancel") && (!e.isKey || e.keyCode != KeyCode.Escape));
            GUILayout.EndHorizontal();
            GUILayout.Space(30);



            // input area for grouping
            EditorGUILayout.LabelField("Grouping Presets", EditorStyles.boldLabel);
            groupName = EditorGUILayout.TextField("Group Name", groupName);

            if (string.IsNullOrEmpty(groupName))
            {
                groupName = "New Group";
            }
            if (GUILayout.Button("Group"))
            {
                Group();
            }
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

                // set the created object orientation
                Vector3 forward = Vector3.Cross(Vector3.right, hit.normal);
                Quaternion surfaceDirection = Quaternion.LookRotation(forward, hit.normal);
                Quaternion orientation = orientToSurface ? surfaceDirection : Quaternion.identity;

                GameObject placedObject = (GameObject)Instantiate(Settings.GetSelectedPreset(), hit.point, orientation);
                placedObject.transform.localScale = new Vector3(1, 1, 1);

                // change name of the painted prefab
                if (presetName.Length != 0)
                {
                    placedObject.name = presetName;
                }
                // set parent of the painted prefab if it is not null
                if (parent != null){
                    placedObject.transform.parent = parent.transform;
                }
                // added undo function to the painting
                Undo.RegisterCreatedObjectUndo(placedObject, "Undo painting");

               
            }
            e.Use();
        }

        // exit paiting mode when press escape
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape){
            isPainting = false;
            Repaint();
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
