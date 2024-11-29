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
    float ThermalFactor = 0.1f;
    float ThermalThreshold = 0.01f;



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

        ThermalFactor = EditorGUILayout.Slider("Thermal Factor (c)", ThermalFactor, 0.01f, 1.0f);
        ThermalThreshold = EditorGUILayout.Slider("Thermal Threshold (T)", ThermalThreshold, 0.01f, 1.0f);

        if(GUILayout.Button("Apply Thermal Erosion"))
        {
            if (terrain == null) 
            {
                Debug.LogError("Terrain is null");
                return;
            }
            Debug.Log("Applying thermal erosion...");
            ErodeTerrainThermal(terrain, ThermalFactor, ThermalThreshold);
        }
        
        GUILayout.Label("3.Hydraulic erosion", EditorStyles.boldLabel);
    }

    void ErodeTerrainThermal(Terrain terrain, float factor, float threshold)
    {
        threshold *= 0.05f;
        TerrainData data = terrain.terrainData;
        Undo.RegisterCompleteObjectUndo(data, "Erode Terrain");
        int w = data.heightmapResolution;
        float[,] rawHeights = data.GetHeights(0, 0, w, w);
        float[] differences = new float[4];
        int[,] directions = new int[,] { {1, 0}, {-1, 0}, {0, 1}, {0, -1} };

        for (int i = 0; i < w; i++)
            for (int j = 0; j < w; j++)
            {
                differences[0] = (i + 1 < w) ? rawHeights[i, j] - rawHeights[i + 1, j] : 0;
                differences[1] = (i - 1 >= 0) ? rawHeights[i, j] - rawHeights[i - 1, j] : 0;
                differences[2] = (j + 1 < w) ? rawHeights[i, j] - rawHeights[i, j + 1] : 0;
                differences[3] = (j - 1 >= 0) ? rawHeights[i, j] - rawHeights[i, j - 1] : 0;

                float dtotal = 0;
                float dmax = 0;
                for (int k = 0; k < 4; k++)
                {
                    if (differences[k] > threshold)
                    {
                        dtotal += differences[k];
                        dmax = Math.Max(dmax, differences[k]);
                    }
                }

                if (dtotal > 0)
                {
                    rawHeights[i, j] -= factor * (dmax - threshold);

                    for (int k = 0; k < 4; k++)
                    {
                        int newI = i + directions[k, 0];
                        int newJ = j + directions[k, 1];
                        if (newI >= 0 && newI < w && newJ >= 0 && newJ < w && differences[k] > threshold)
                        {
                            rawHeights[newI, newJ] += factor * (dmax - threshold) * differences[k] / dtotal;
                        }
                    }
                }
            }

        data.SetHeights(0, 0, rawHeights);
    }

}
