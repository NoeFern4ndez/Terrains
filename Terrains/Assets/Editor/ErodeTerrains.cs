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
    // Thermal erosion
    float ThermalFactor = 0.1f;
    float ThermalThreshold = 0.01f;
    // Hydraulic erosion
    float WaterGenK = 0.01f;
    float WaterEvapK = 0.01f;
    float WaterSedimentK = 0.01f;
    float SedimentPerWaterK = 0.01f;
    int Niters = 100;



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
        WaterGenK = EditorGUILayout.Slider("Rain (Kr)", WaterGenK, 0.01f, 1.0f);
        WaterEvapK = EditorGUILayout.Slider("Evaporation (Ke)", WaterEvapK, 0.01f, 1.0f);
        WaterSedimentK = EditorGUILayout.Slider("Sediment (Ks)", WaterSedimentK, 0.01f, 1.0f);
        SedimentPerWaterK = EditorGUILayout.Slider("Sediment per Water (Kc)", SedimentPerWaterK, 0.01f, 1.0f);
        Niters = EditorGUILayout.IntSlider("Number of iterations", Niters, 1, 100);

        if(GUILayout.Button("Apply Hydraulic Erosion"))
        {
            if (terrain == null) 
            {
                Debug.LogError("Terrain is null");
                return;
            }
            Debug.Log("Applying hydraulic erosion...");
            ErodeTerrainHydraulic(terrain, WaterGenK, WaterEvapK, WaterSedimentK, SedimentPerWaterK, Niters);
        }
    }

    void ErodeTerrainThermal(Terrain terrain, float factor, float threshold)
    {
        threshold *= 0.05f;
        TerrainData data = terrain.terrainData;
        Undo.RegisterCompleteObjectUndo(data, "Erode Terrain");
        int w = data.heightmapResolution;
        float[,] rawHeights = data.GetHeights(0, 0, w, w);
        float[] differences = new float[4];

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

                    if (i + 1 < w && differences[0] > threshold)
                        rawHeights[i + 1, j] += factor * (dmax - threshold) * differences[0] / dtotal;
                    if (i - 1 >= 0 && differences[1] > threshold)
                        rawHeights[i - 1, j] += factor * (dmax - threshold) * differences[1] / dtotal;
                    if (j + 1 < w && differences[2] > threshold)
                        rawHeights[i, j + 1] += factor * (dmax - threshold) * differences[2] / dtotal;
                    if (j - 1 >= 0 && differences[3] > threshold)
                        rawHeights[i, j - 1] += factor * (dmax - threshold) * differences[3] / dtotal;
                }
            }

        data.SetHeights(0, 0, rawHeights);
    }

    void ErodeTerrainHydraulic(Terrain terrain, float Kr, float Ke, float Ks, float Kc, int N)
    {
        TerrainData data = terrain.terrainData;
        Undo.RegisterCompleteObjectUndo(data, "Erode Terrain");
        int w = data.heightmapResolution;
        float[,] rawHeights = data.GetHeights(0, 0, w, w);
        // Paso 0
        float[,] water = new float[w, w];
        float[,] sediment = new float[w, w];

        for (int i = 0; i < w; i++)
            for (int j = 0; j < w; j++)
            {
                water[i, j] = 0;
                sediment[i, j] = 0;
            }

        for(int i = 1; i < w - 1; i++) 
        {
            for (int j = 1; j < w - 1; j++)
            {
                for (int k = 0; k < N; k++)
                {
                    // Paso 1
                    water[i, j] += Kr;
                    // Paso 2
                    if(water[i, j] > 0)
                    {
                        rawHeights[i, j] -= Ks * water[i, j];
                        sediment[i, j] += Ks * water[i, j];

                        // Paso 3
                        float left = rawHeights[i - 1, j];
                        float right = rawHeights[i + 1, j];
                        float up = rawHeights[i, j - 1];
                        float down = rawHeights[i, j + 1];
                        
                        float waterleft = water[i - 1, j];
                        float waterright = water[i + 1, j];
                        float waterup = water[i, j - 1];
                        float waterdown = water[i, j + 1];

                        float sedimentleft = sediment[i - 1, j];
                        float sedimentright = sediment[i + 1, j];
                        float sedimentup = sediment[i, j - 1];
                        float sedimentdown = sediment[i, j + 1];

                        float a = rawHeights[i, j] + water[i, j];
                        float aleft = left + waterleft;
                        float aright = right + waterright;
                        float aup = up + waterup;
                        float adown = down + waterdown;

                        float amean = (a + aleft + aright + aup + adown) / 5;
                        float adelta = a - amean;

                        float dleft = a - aleft;
                        float dright = a - aright;
                        float dup = a - aup;
                        float ddown = a - adown;

                        float dtotal = 0;
                        if (dleft > 0) dtotal += dleft;
                        if (dright > 0) dtotal += dright;
                        if (dup > 0) dtotal += dup;
                        if (ddown > 0) dtotal += ddown;
                        
                        float wdelta = Math.Min(water[i, j], adelta);

                        if (dtotal > 0)
                        {
                            float wdeltaleft = wdelta * dleft / dtotal;
                            float wdeltaright = wdelta * dright / dtotal;
                            float wdeltadown = wdelta * ddown / dtotal;
                            float wdeltadup = wdelta * dup / dtotal;

                            waterleft += wdeltaleft;
                            waterright += wdeltaright;
                            waterup += wdeltadup;
                            waterdown += wdeltadown;
                            water[i - 1, j] = waterleft;
                            water[i + 1, j] = waterright;
                            water[i, j - 1] = waterup;
                            water[i, j + 1] = waterdown;

                            sedimentleft += sediment[i, j] * wdeltaleft / water[i, j];
                            sedimentright += sediment[i, j] * wdeltaright / water[i, j];
                            sedimentup += sediment[i, j] * wdeltadup / water[i, j];
                            sedimentdown += sediment[i, j] * wdeltadown / water[i, j];
                            sediment[i - 1, j] = sedimentleft;
                            sediment[i + 1, j] = sedimentright;
                            sediment[i, j - 1] = sedimentup;
                            sediment[i, j + 1] = sedimentdown;

                            sediment[i, j] -= sediment[i, j] * (wdelta / water[i, j]);
                            water[i, j] -= wdelta;

                            // Paso 4
                            water[i, j] *= (1 - Ke);
                            float maxsediment = Kc * water[i, j];
                            if(sediment[i, j] > maxsediment)
                            {
                                float deltaSediment = Math.Max(0, sediment[i, j] - maxsediment);
                                sediment[i, j] -= deltaSediment;
                                rawHeights[i, j] += deltaSediment;
                            }
                        }
                    }
                }

                // Final
                rawHeights[i, j] += sediment[i, j];

            }
        }

        // Asignar a los bordes el valor de la celda adyacente
        for (int i = 0; i < w; i++)
        {
            rawHeights[i, 0] = rawHeights[i, 1];
            rawHeights[i, w - 1] = rawHeights[i, w - 2];
            rawHeights[0, i] = rawHeights[1, i];
            rawHeights[w - 1, i] = rawHeights[w - 2, i];
        }


        data.SetHeights(0, 0, rawHeights);
    }

}
