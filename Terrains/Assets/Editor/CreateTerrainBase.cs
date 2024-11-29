using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Net;

public class CreateTerrain : EditorWindow
{
    // string assetName = "TerrainTexture";

    Texture2D heightTexture;
    Terrain terrain;
    float heightMultiplier = 1.0f;
    // Midpoint
    int midpointN = 8;
    int midpointSeed = 0;
    float midpointAmplitude = 1;
    float midpointH = 0.5f;
    // Diamond-Square
    int diamondSquareN = 8;
    int diamondSquareSeed = 0;
    float diamondSquareAmplitude = 1;
    float diamondSquareH = 0.5f;
    // FBM
    float FMBAmplitude = 1;
    float FBMFreq = 4;
    float FBMGain = 0.5f;
    float FBMLacunarity = 3.0f;
    int FBMOctaves = 8;
    int FBMSeed = 0;
    enum FBMType { Perlin, Voronoi, Sine };
    int FBMTypeIndex = 0;
    int FBMVoronoiGridSize = 5;
    Vector2 FBMSineDirection = new Vector2(1, 1);
    bool FBMHybridMultifractal = true;




    // Add menu item to show the window
    [MenuItem ("Terrains/Create terrains")]
    private static void ShowWindow() {
        var window = GetWindow<CreateTerrain>();
        window.titleContent = new GUIContent("Create Terrains");
        window.Show();
    }

    private void OnGUI() {
        GUILayout.Label("1.Terrain", EditorStyles.boldLabel);        
        terrain  = EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true) as Terrain;

        if (terrain == null) 
        {
            Debug.LogError("Terrain is null");
            return;
        }

        GUILayout.Label("2.Terrain height configuration", EditorStyles.boldLabel);
        // Mapa de alturas
        heightTexture = EditorGUILayout.ObjectField("Height Texture", heightTexture, typeof(Texture2D), false) as Texture2D;
        if(GUILayout.Button("Apply height texture")) 
        {
            if (heightTexture == null) 
            {
                Debug.LogError("Texture is null");
                return;
            }
            Debug.Log("Getting height from texture...");
            ApplyHeightMap(heightTexture, terrain);
        }
        // Escalado de alturas
        heightMultiplier = EditorGUILayout.Slider("Height multiplier", heightMultiplier, 0.001f, 2.0f);
        if (GUILayout.Button("Apply height multiplier")) 
        {            
            if (terrain == null) 
            {
                Debug.LogError("Terrain is null");
                return;
            }
            Debug.Log("Applying height to terrain...");
            DivideHeights(terrain, heightMultiplier);
        }


        GUILayout.Label("3. Generación Punto medio 3D", EditorStyles.boldLabel);
        // Número de divisiones
        midpointN = EditorGUILayout.IntSlider("Number of divisions (N)", midpointN, 1, 11);
        // Amplitud
        midpointAmplitude = EditorGUILayout.Slider("Noise Amplitude (A)", midpointAmplitude, 0.001f, 1.0f);
        // H
        midpointH = EditorGUILayout.Slider("Amplitude falloff (H)", midpointH, 0.001f, 1.0f);
        // Semilla
        midpointSeed = EditorGUILayout.IntField("Seed", midpointSeed);
        
        if (GUILayout.Button("Generate midpoint terrain")) 
        {
            if (terrain == null) 
            {
                Debug.LogError("Terrain is null");
                return;
            }
            Debug.Log("Generating midpoint terrain...");
            GenerateMidpointTerrain(terrain, midpointN, midpointSeed, midpointAmplitude, midpointH);
        }

        GUILayout.Label("4. Generación diamante-cuadrado", EditorStyles.boldLabel);
        // Número de divisiones
        diamondSquareN = EditorGUILayout.IntSlider("Number of divisions (N)", diamondSquareN, 1, 11);
        // Amplitud
        diamondSquareAmplitude = EditorGUILayout.Slider("Noise Amplitude (A)", diamondSquareAmplitude, 0.001f, 1.0f);
        // H
        diamondSquareH = EditorGUILayout.Slider("Amplitude falloff (H)", diamondSquareH, 0.001f, 1.0f);
        // Semilla
        diamondSquareSeed = EditorGUILayout.IntField("Seed", diamondSquareSeed);

        if (GUILayout.Button("Generate diamond-square terrain")) 
        {
            if (terrain == null) 
            {
                Debug.LogError("Terrain is null");
                return;
            }
            Debug.Log("Generating diamond-square terrain...");
            GenerateDiamondSquareTerrain(terrain, diamondSquareN, diamondSquareSeed, diamondSquareAmplitude, diamondSquareH);
        }

        GUILayout.Label("5. Generación FBM", EditorStyles.boldLabel);
        // Amplitud
        FMBAmplitude = EditorGUILayout.Slider("FBM Amplitude (A)", FMBAmplitude, 0.001f, 1.0f);
        // Frecuencia
        FBMFreq = EditorGUILayout.Slider("FBM Frequency (F)", FBMFreq, 0.001f, 8.0f);
        // Ganancia
        FBMGain = EditorGUILayout.Slider("FBM amplitude delta (gain)", FBMGain, 0.001f, 2.0f);
        // Lacunarity
        FBMLacunarity = EditorGUILayout.Slider("FBM frequency delta (Lacunarity)", FBMLacunarity, 0.001f, 6.0f);
        // Octavas
        FBMOctaves = EditorGUILayout.IntSlider("FBM Octaves", FBMOctaves, 1, 32);
        // Semilla
        FBMSeed = EditorGUILayout.IntField("Seed", FBMSeed);
        // Tipo de FBM
        FBMTypeIndex = EditorGUILayout.Popup("FBM Type", FBMTypeIndex, Enum.GetNames(typeof(FBMType)));

        if(FBMTypeIndex == 1)
            FBMVoronoiGridSize = EditorGUILayout.IntSlider("Voronoi Grid Size", FBMVoronoiGridSize, 1, 32);
        if(FBMTypeIndex == 2)
            FBMSineDirection = EditorGUILayout.Vector2Field("Sine Direction", FBMSineDirection);
        // Hybrid Multifractal
        FBMHybridMultifractal = EditorGUILayout.Toggle("Hybrid Multifractal", FBMHybridMultifractal);
        
        if(GUILayout.Button("Generate FBM terrain")) 
        {
            if (terrain == null) 
            {
                Debug.LogError("Terrain is null");
                return;
            }
            Debug.Log("Generating FBM terrain...");
            GenerateFBM(terrain, FBMSeed, FMBAmplitude, FBMFreq, FBMGain, FBMLacunarity, FBMOctaves, FBMHybridMultifractal, FBMTypeIndex, FBMSineDirection);
        }        
    }

    void DivideHeights(Terrain terrain, float scale) 
    {
        // Get terrain data
        TerrainData data = terrain.terrainData;
        // Register data for undo option
        Undo.RegisterCompleteObjectUndo(data, "");
        // Get resolution
        int w = data.heightmapResolution;
        // Get height data
        float[,] rawHeights = data.GetHeights(0,0,w,w);

        // Divide every height value
        for (int i = 0; i < w; i++)
            for (int j = 0; j < w; j++)
                rawHeights[i, j] = rawHeights[i, j] * scale;

        // Set the new height data
        Vector3 tam = data.size;
        data.SetHeights(0, 0, rawHeights);
        data.size = tam;
        terrain.terrainData = data;
    }

    void ApplyHeightMap(Texture2D texture, Terrain terrain) 
    {
        // Get terrain data
        TerrainData data = terrain.terrainData;
        // Register data for undo option
        Undo.RegisterCompleteObjectUndo(data, "");
        // Get texture data
        Color32[] rawData = texture.GetPixels32(0);
        int w = texture.width;
        int h = texture.height;

        // Create height matrix
        float[,] rawHeights = new float[texture.width, texture.height];
        // Copy data from texture to height matrix
        for (int i = 0; i < w; i++)
            for (int j = 0; j < h; j++)
                rawHeights[j, i] = rawData[j*w + i].r / 255.0f;

        // Set the new height data
        Vector3 tam = data.size;
        data.heightmapResolution = w;
        data.SetHeights(0, 0, rawHeights);
        data.size = tam;
        terrain.terrainData = data;
    }

    void ApplyHeightMap(float[,] h, Terrain terrain)
    {
        TerrainData data = terrain.terrainData;
        // Register data for undo option
        Undo.RegisterCompleteObjectUndo(data, "");
        Vector3 tam = data.size;
        data.heightmapResolution = h.GetLength(0);
        data.SetHeights(0, 0, h);
        data.size = tam;
        terrain.terrainData = data;
    }

    float zValue(int n, float A, float H) 
    {
        return A * Mathf.Pow(H, n) * (UnityEngine.Random.value - 0.5f);
    }

    void GenerateMidpointTerrain(Terrain terrain, int N, int seed, float A, float H)
    {
        int nVert = (int)Mathf.Pow(2, N) + 1;
        var vertices = new float[nVert, nVert];

        UnityEngine.Random.InitState(seed);
    
        int n = 0;
        vertices[0, 0] = 0.5f + zValue(n, A, H);
        vertices[0, nVert-1] = 0.5f + zValue(n, A, H);
        vertices[nVert-1, 0] = 0.5f + zValue(n, A, H);
        vertices[nVert-1, nVert-1] = 0.5f + zValue(n, A, H);

        // For each division
        for (n = 0; n < N; n++) 
        {
            // Size of each square
            int d = (int)Mathf.Pow(2, (N - n));
            // Half size of each square
            int d2 = d/2;
            // 1. Compute rows (North & South)
            for (int j=0; j<nVert; j+=d)
                for (int i=0; i+d<nVert; i+=d)
                    vertices[i+d2,j] = (vertices[i,j] + vertices[i+d,j]) * 0.5f + zValue(n, A, H);

            // 2. Compute columns (East & West)
            for (int i=0; i<nVert; i+=d)
                for (int j=0; j+d<nVert; j+=d)
                    vertices[i,j+d2] = (vertices[i,j] + vertices[i,j+d]) * 0.5f + zValue(n, A, H);

            // 3. Compute centers
           for (int i=0; i+d<nVert; i+=d)
                for (int j=0; j+d<nVert; j+=d)
                    vertices[i+d2,j+d2] = (vertices[i+d2,j] + vertices[i+d2,j+d] +
                    vertices[i,j+d2] + vertices[i+d,j+d2]) * 0.25f + zValue(n, A, H);
        }

        // Apply heightmap	
        ApplyHeightMap(vertices, terrain);
    }
 
    void GenerateDiamondSquareTerrain(Terrain terrain, int N, int seed, float A, float H)
    {
        int nVert = (int)Mathf.Pow(2, N) + 1;
        var vertices = new float[nVert, nVert];

        UnityEngine.Random.InitState(seed);
    
        int n = 0;
        vertices[0, 0] = 0.5f + zValue(n, A, H);
        vertices[0, nVert-1] = 0.5f + zValue(n, A, H);
        vertices[nVert-1, 0] = 0.5f + zValue(n, A, H);
        vertices[nVert-1, nVert-1] = 0.5f + zValue(n, A, H);

        // For each division
        for (n = 0; n < N; n++) 
        {
            // Size of each square
            int d = (int)Mathf.Pow(2, (N - n));
            // Half size of each square
            int d2 = d/2;
            // 1. Compute rows (North & South)
            for (int j=0; j<nVert; j+=d)
                for (int i=0; i+d<nVert; i+=d)
                    vertices[i+d2,j] = (vertices[i,j] + vertices[i+d,j]) * 0.5f + zValue(n, A, H);

            // 2. Compute columns (East & West)
            for (int i=0; i<nVert; i+=d)
                for (int j=0; j+d<nVert; j+=d)
                    vertices[i,j+d2] = (vertices[i,j] + vertices[i,j+d]) * 0.5f + zValue(n, A, H);

            // 3. Compute centers
            for (int i=0; i+d<nVert; i+=d)
                for (int j=0; j+d<nVert; j+=d)
                    vertices[i+d2,j+d2] = (vertices[i+d2,j] + vertices[i+d2,j+d] +
                    vertices[i,j+d2] + vertices[i+d,j+d2]) * 0.25f + zValue(n, A, H);

            // 4. Compute diamonds
            for (int i=0; i+d<nVert; i+=d)
                for (int j=0; j+d<nVert; j+=d)
                {
                    vertices[i+d2,j+d2] = (vertices[i+d2,j] + vertices[i+d2,j+d] +
                    vertices[i,j+d2] + vertices[i+d,j+d2]) * 0.25f + zValue(n, A, H);
                    vertices[i+d2,j+d2] += zValue(n, A, H);
                }
        }

        // Apply heightmap
        ApplyHeightMap(vertices, terrain);
    }

    float Voronoi(float x, float y)
    {
        // Define the size of the grid
        int gridSize = FBMVoronoiGridSize;
        float cellSize = 1.0f / gridSize;

        // Compute the cell coordinates
        int xCell = Mathf.FloorToInt(x / cellSize);
        int yCell = Mathf.FloorToInt(y / cellSize);

        float minDistance = float.MaxValue;

        // Check neighboring cells (to include edge cases)
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                // Get random seed position within the cell
                Vector2 cellPosition = new Vector2(
                    (xCell + i) * cellSize + UnityEngine.Random.value * cellSize,
                    (yCell + j) * cellSize + UnityEngine.Random.value * cellSize
                );

                // Calculate the distance from the input point to the seed point
                float distance = Vector2.Distance(new Vector2(x, y), cellPosition);

                // Track the minimum distance for Voronoi effect
                minDistance = Mathf.Min(minDistance, distance);
            }
        }

        // Normalize the distance for terrain generation (optional scaling)
        return minDistance;
    }

    float Sine(float x, float y, Vector2 direction)
    {
        return Mathf.Sin(direction.x * x + direction.y * y);
    }

    
    void GenerateFBM(Terrain terrain, int seed, float A0, float F0, float dA, float dF, int octaves, bool H, int type, Vector2 direction0)
    {
        int nVert = terrain.terrainData.heightmapResolution;
        var vertices = new float[nVert, nVert];

        UnityEngine.Random.InitState(seed);
        
        for(int i = 0; i < nVert; i++)
        {
            for(int j = 0; j < nVert; j++)
            {
                float A = A0;
                float F = F0;
                float y = 0;
                Vector2 direction = new Vector2(direction0.x, direction0.y);

                for(int k = 1; k <= octaves; k++)
                {
                    float Noise = 0;

                    if(type == 0)
                        Noise = Mathf.PerlinNoise(F * i / nVert + seed, F * j / nVert + seed);
                    else if(type == 1)
                        Noise = Voronoi(F * i / nVert + seed, F * j / nVert + seed);
                    else if(type == 2)
                        Noise = Sine(F * i / nVert + seed, F * j / nVert + seed, direction);


                    float weight = A * Noise;
                    
                    A *= dA;
                    F *= dF;
                    if(H)
                    {
                        y += A * Noise * weight;
                        direction.x += UnityEngine.Random.value * y;
                        direction.y += UnityEngine.Random.value * y;
                    }
                    else
                    {
                        y += A * Noise;
                        direction.x += UnityEngine.Random.value;
                        direction.y += UnityEngine.Random.value;
                    }

                    weight = y;
                }
                
                vertices[i, j] = y;
            }
        }
        
        ApplyHeightMap(vertices, terrain);
    }

}
