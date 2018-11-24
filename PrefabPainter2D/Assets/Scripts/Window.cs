using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Window : EditorWindow {
    private Object firstPrefab;
    private bool isPainting;

    private void Awake()
    {
        firstPrefab = AssetDatabase.LoadAssetAtPath("Assets/Resources/clover.prefab", typeof(Object));
    }
    // draw the window
    [MenuItem("Window/PrefabPainter")]
    public static void ShowWindow(){
        GetWindow<Window>("Prefab Painter");
    }

    private void OnGUI()
    {
        Event e = Event.current;
        GUILayout.Space(20);

        // paint buttons
        GUILayout.BeginHorizontal();

        isPainting = GUILayout.Toggle(isPainting, "Paint", "button");
        isPainting &= (!GUILayout.Button("Cancel") && (!e.isKey || e.keyCode != KeyCode.Escape));

        GUILayout.EndHorizontal();
    }

    //private void Update()
    //{
    //    if (Input.GetMouseButtonDown(0)){
    //        Debug.Log("click");
    //    }
    //}

    public bool IsPainting(){
        return isPainting;
    }
}
