using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings{
    public static int selectedPrefab;
    public static List<GameObject> allPresets;
    public static int selectedIndex;

    static Settings()
    {
        // load all prefabs in the window
        allPresets = new List<GameObject>();
        foreach (Object preset in Resources.LoadAll("", typeof(GameObject))){
            allPresets.Add((GameObject)preset);
        }
    }

    public static string GetSelectedName(){
        return allPresets[selectedIndex].name;
    }

    public static GameObject GetSelectedPreset(){
        return (GameObject)allPresets[selectedIndex];
    }

    public static void AddPreset(GameObject preset)
    {
        allPresets.Add(preset);
    }

    public static void DeletePreset(){
        allPresets.Remove(allPresets[selectedIndex]);
    }
}







// setting styles for gui panel
public static class Styles
{
    // Styles
    public static int presetIconWidth;
    public static int presetIconHeight;
    public static int presetWindowRows;
    public static Color32 colorBlue;        // box color for selected preset
    public static GUIStyle titleStyle;


    static Styles(){

        presetIconWidth = 60;
        presetIconHeight = 72;
        presetWindowRows = 3;
        colorBlue = new Color32(62, 125, 231, 255);

        titleStyle = new GUIStyle
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

    }
}


