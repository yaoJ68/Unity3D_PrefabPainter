using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(object))]
public class Painter : Editor {

    private Window window;

	// Update is called once per frame
	void Update () {
        
	}

    private void OnSceneGUI()
    {
        if (Input.GetMouseButtonDown(0)){
            Debug.Log("click");
        }
    }
}
