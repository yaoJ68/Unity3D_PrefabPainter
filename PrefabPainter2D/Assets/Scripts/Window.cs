using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Window : EditorWindow {
    public GameObject firstPrefab;
    private bool isPainting;

    // draw the window
    [MenuItem("Window/PrefabPainter")]
    public static void ShowWindow(){
        GetWindow<Window>("Prefab Painter");
    }

    private void OnGUI()
    {
        Event e = Event.current;
        GUILayout.Space(20);

        firstPrefab = (GameObject)EditorGUILayout.ObjectField(firstPrefab, typeof(GameObject), true);
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

        if (e.type == EventType.MouseDown && e.button == 0 && isPainting){
            Vector3 mousePosition = e.mousePosition;
            mousePosition.y = sv.camera.pixelHeight - mousePosition.y;
            mousePosition.y = -mousePosition.y;
            mousePosition = sv.camera.ScreenToWorldPoint(e.mousePosition);
            mousePosition.z = 0;

            GameObject placedObject = (GameObject)PrefabUtility.InstantiatePrefab(firstPrefab);
            placedObject.transform.position = mousePosition;
            placedObject.transform.localScale = new Vector3(1, 1, 1);

            //RaycastHit hit;

            //Ray ray = Camera.current.ScreenPointToRay(new Vector3(e.mousePosition.x, Camera.current.pixelHeight - e.mousePosition.y, 0));
            //if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~LayerMask.NameToLayer("Default"))){
            //    GameObject placedObject = (GameObject)PrefabUtility.InstantiatePrefab(PrefabUtility.GetCorrespondingObjectFromSource(firstPrefab));
            //    Debug.Log("click");
            //    placedObject.transform.position = hit.point;
            //    placedObject.transform.localScale = new Vector3(1, 1, 1);
            //}

        }
    }
}
