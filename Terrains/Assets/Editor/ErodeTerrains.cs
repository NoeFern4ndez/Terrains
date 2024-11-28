using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Net;

public class ErodeTerrain : EditorWindow
{
    // string assetName = "TerrainTexture";

    Terrain terrain;



    // Add menu item to show the window
    [MenuItem ("Terrains/Erode Terrains")]
    private static void ShowWindow() {
        var window = GetWindow<ErodeTerrain>();
        window.titleContent = new GUIContent("Erode Terrains");
        window.Show();
    }

    private void OnGUI() 
    
    {
        GUILayout.Label("1.Terrain", EditorStyles.boldLabel);        
        terrain  = EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true) as Terrain;

        if (terrain == null) 
        {
            Debug.LogError("Terrain is null");
            return;
        }

        GUILayout.Label("2.Thermic erosion", EditorStyles.boldLabel);
        
        GUILayout.Label("3.Hydraulic erosion", EditorStyles.boldLabel);
    }

}
