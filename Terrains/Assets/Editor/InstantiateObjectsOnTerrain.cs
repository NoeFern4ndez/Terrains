using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class InstantiateObjectsOnTerrain : EditorWindow
{
    Terrain terrain;

    GameObject rockPrefab1;
    GameObject rockPrefab2;
    GameObject rockPrefab3;
    GameObject rockPrefab4;

    GameObject treePrefab1;
    GameObject treePrefab2;
    GameObject treePrefab3;
    GameObject treePrefab4;

    float rockDensity = 0.75f;
    float treeDensity = 0.5f;
    float rockMaxScale = 9.0f;
    float treeMaxScale = 200.0f;
    float rockMinScale = 3.0f;
    float treeMinScale = 150.0f;
    float rockMaxHeightOnTerrain = 0.55f;
    float rockMinHeightOnTerrain = 0.2f;
    float treeMaxHeightOnTerrain = 0.3f;
    float treeMinHeightOnTerrain = 0.15f;

    float minRockDistance = 32.0f;
    float minTreeDistance = 14.0f; 

    int treeSeed;
    int rockSeed;

    private List<Vector3> rockInstantiatedPositions = new List<Vector3>();
    private List<Vector3> treeInstantiatedPositions = new List<Vector3>();  

    [MenuItem("Terrains/Instantiate Objects on Terrain")]
    private static void ShowWindow()
    {
        var window = GetWindow<InstantiateObjectsOnTerrain>();
        window.titleContent = new GUIContent("Instantiate Objects on Terrain");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("1.Terrain", EditorStyles.boldLabel);
        terrain = EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true) as Terrain;

        if (terrain == null)
        {
            Debug.LogError("Terrain is null");
            return;
        }


        GUILayout.Label("2.Rock Instantiation", EditorStyles.boldLabel);
        rockPrefab1 = EditorGUILayout.ObjectField("Rock Prefab 1", rockPrefab1, typeof(GameObject), true) as GameObject;
        rockPrefab2 = EditorGUILayout.ObjectField("Rock Prefab 2", rockPrefab2, typeof(GameObject), true) as GameObject;
        rockPrefab3 = EditorGUILayout.ObjectField("Rock Prefab 3", rockPrefab3, typeof(GameObject), true) as GameObject;
        rockPrefab4 = EditorGUILayout.ObjectField("Rock Prefab 4", rockPrefab4, typeof(GameObject), true) as GameObject;

        rockDensity = EditorGUILayout.Slider("Rock Density", rockDensity, 0.0f, 1.0f);
        rockMaxScale = EditorGUILayout.Slider("Rock Max Scale", rockMaxScale, 0.0f, 10.0f);
        rockMinScale = EditorGUILayout.Slider("Rock Min Scale", rockMinScale, 0.0f, 10.0f);
        rockMaxHeightOnTerrain = EditorGUILayout.Slider("Rock Max Height on Terrain", rockMaxHeightOnTerrain, 0.0f, 1.0f);
        rockMinHeightOnTerrain = EditorGUILayout.Slider("Rock Min Height on Terrain", rockMinHeightOnTerrain, 0.0f, 1.0f);
        minRockDistance = EditorGUILayout.FloatField("Min Rock Distance", minRockDistance);

        rockSeed = EditorGUILayout.IntField("Rock Seed", rockSeed);

        if (GUILayout.Button("Instantiate Rocks"))
        {
            InstantiateRocks();
        }

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Delete Rocks"))
        {
            deleteRocks();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Label("3.Tree Instantiation", EditorStyles.boldLabel);
        treePrefab1 = EditorGUILayout.ObjectField("Tree Prefab 1", treePrefab1, typeof(GameObject), true) as GameObject;
        treePrefab2 = EditorGUILayout.ObjectField("Tree Prefab 2", treePrefab2, typeof(GameObject), true) as GameObject;
        treePrefab3 = EditorGUILayout.ObjectField("Tree Prefab 3", treePrefab3, typeof(GameObject), true) as GameObject;
        treePrefab4 = EditorGUILayout.ObjectField("Tree Prefab 4", treePrefab4, typeof(GameObject), true) as GameObject;

        treeDensity = EditorGUILayout.Slider("Tree Density", treeDensity, 0.0f, 1.0f);
        treeMaxScale = EditorGUILayout.Slider("Tree Max Scale", treeMaxScale, 0.0f, 200.0f);
        treeMinScale = EditorGUILayout.Slider("Tree Min Scale", treeMinScale, 0.0f, 200.0f);
        treeMaxHeightOnTerrain = EditorGUILayout.Slider("Tree Max Height on Terrain", treeMaxHeightOnTerrain, 0.0f, 1.0f);
        treeMinHeightOnTerrain = EditorGUILayout.Slider("Tree Min Height on Terrain", treeMinHeightOnTerrain, 0.0f, 1.0f);
        minTreeDistance = EditorGUILayout.FloatField("Min Tree Distance", minTreeDistance);

        treeSeed = EditorGUILayout.IntField("Tree Seed", treeSeed);

        if (GUILayout.Button("Instantiate Trees"))
        {
            InstantiateTrees();
        }

        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("Delete Trees"))
        {
            deleteTrees();
        }
        GUI.backgroundColor = Color.white;
    }

    float getMaxHeightOnTerrain()
    {
        float maxHeight = 0.0f;
        for (int i = 0; i < terrain.terrainData.size.x; i++)
        {
            for (int j = 0; j < terrain.terrainData.size.z; j++)
            {
                float y = terrain.terrainData.GetHeight(i, j);
                if (y > maxHeight)
                {
                    maxHeight = y;
                }
            }
        }
        return maxHeight;
    }

    float getMinHeightOnTerrain()
    {
        float minHeight = 0.0f;
        for (int i = 0; i < terrain.terrainData.size.x; i++)
        {
            for (int j = 0; j < terrain.terrainData.size.z; j++)
            {
                float y = terrain.terrainData.GetHeight(i, j);
                if (y < minHeight)
                {
                    minHeight = y;
                }
            }
        }
        return minHeight;
    }

    private void InstantiateRocks()
    {
        Vector3 terrainPosition = terrain.transform.position;

        bool notUsePrefab1 = rockPrefab1 == null;
        bool notUsePrefab2 = rockPrefab2 == null;
        bool notUsePrefab3 = rockPrefab3 == null;
        bool notUsePrefab4 = rockPrefab4 == null;

        if (notUsePrefab1 && notUsePrefab2 && notUsePrefab3 && notUsePrefab4)
        {
            Debug.LogError("Rock Prefabs are null");
            return;
        }

        if (terrain == null)
        {
            Debug.LogError("Terrain is null");
            return;
        }

        float terrainMaxHeight = getMaxHeightOnTerrain();
        float maxHeightOnTerrain = rockMaxHeightOnTerrain * terrainMaxHeight;
        float minHeightOnTerrain = rockMinHeightOnTerrain * terrainMaxHeight;

        System.Random random = new System.Random(rockSeed);
        rockInstantiatedPositions.Clear();

        for (int i = 0; i < terrain.terrainData.size.x; i++)
        {
            for (int j = 0; j < terrain.terrainData.size.z; j++)
            {
                if (random.NextDouble() < rockDensity)
                {
                    float x = i + (float)random.NextDouble() + terrainPosition.x;
                    float z = j + (float)random.NextDouble() + terrainPosition.z;
                    float y = terrain.SampleHeight(new Vector3(x, 0, z)) + terrainPosition.y;

                    if (y < maxHeightOnTerrain && y > minHeightOnTerrain)
                    {
                        //Vector3 normal = terrain.terrainData.GetInterpolatedNormal(x / terrain.terrainData.size.x, z / terrain.terrainData.size.z);
                        Vector3 normal = terrain.terrainData.GetInterpolatedNormal(i / terrain.terrainData.size.x, j / terrain.terrainData.size.z);
                        //Vector3 
                        if (Vector3.Angle(normal, Vector3.up) > 30.0f) // Check if the surface is not horizontal
                        {
                            continue;
                        }

                        Vector3 position = new Vector3(x, y, z);
                        bool tooClose = false;

                        foreach (var pos in treeInstantiatedPositions)
                        {
                            if (Vector3.Distance(pos, position) < minRockDistance)
                            {
                                tooClose = true;
                                break;
                            }
                        }

                        foreach (var pos in rockInstantiatedPositions)
                        {
                            if (Vector3.Distance(pos, position) < minRockDistance)
                            {
                                tooClose = true;
                                break;
                            }
                        }

                        if (tooClose)
                        {
                            continue;
                        }

                        float scale = (float)random.NextDouble() * (rockMaxScale - rockMinScale) + rockMinScale;
                        int rockPrefabIndex = random.Next(1, 5);

                        GameObject rockPrefab = null;

                        switch (rockPrefabIndex)
                        {
                            case 1:
                                if (notUsePrefab1)
                                {
                                    continue;
                                }
                                rockPrefab = rockPrefab1;
                                break;
                            case 2:
                                if (notUsePrefab2)
                                {
                                    continue;
                                }
                                rockPrefab = rockPrefab2;
                                break;
                            case 3:
                                if (notUsePrefab3)
                                {
                                    continue;
                                }
                                rockPrefab = rockPrefab3;
                                break;
                            case 4:
                                if (notUsePrefab4)
                                {
                                    continue;
                                }
                                rockPrefab = rockPrefab4;
                                break;
                        }

                        GameObject rock = Instantiate(rockPrefab, position, Quaternion.identity);
                        rock.transform.localScale = new Vector3(scale, scale, scale);
                        rock.name += "_" + terrain.name;
                        rock.transform.parent = terrain.transform;
                        rockInstantiatedPositions.Add(position);
                        Undo.RegisterCreatedObjectUndo(rock, "Instantiate Rocks");
                    }
                }
            }
        }
        Debug.Log("Rocks instantiated");
    }

    private void InstantiateTrees()
    {
        Vector3 terrainPosition = terrain.transform.position;

        bool notUsePrefab1 = treePrefab1 == null;
        bool notUsePrefab2 = treePrefab2 == null;
        bool notUsePrefab3 = treePrefab3 == null;
        bool notUsePrefab4 = treePrefab4 == null;

        if (notUsePrefab1 && notUsePrefab2 && notUsePrefab3 && notUsePrefab4)
        {
            Debug.LogError("Tree Prefabs are null");
            return;
        }

        if (terrain == null)
        {
            Debug.LogError("Terrain is null");
            return;
        }

        float terrainMaxHeight = getMaxHeightOnTerrain();
        float maxHeightOnTerrain = treeMaxHeightOnTerrain * terrainMaxHeight;
        float minHeightOnTerrain = treeMinHeightOnTerrain * terrainMaxHeight;

        System.Random random = new System.Random(treeSeed);
        treeInstantiatedPositions.Clear();

        for (int i = 0; i < terrain.terrainData.size.x; i++)
        {
            for (int j = 0; j < terrain.terrainData.size.z; j++)
            {
                if (random.NextDouble() < treeDensity)
                {
                    float x = i + (float)random.NextDouble() + terrainPosition.x;
                    float z = j + (float)random.NextDouble() + terrainPosition.z;
                    float y = terrain.SampleHeight(new Vector3(x, 0, z)) + terrainPosition.y;

                    if (y < maxHeightOnTerrain && y > minHeightOnTerrain)
                    {
                        Vector3 normal = terrain.terrainData.GetInterpolatedNormal(i / terrain.terrainData.size.x, j / terrain.terrainData.size.z);
                        if (Vector3.Angle(normal, Vector3.up) > 25.0f) // Check if the surface is not horizontal
                        {
                            continue;
                        }

                        Vector3 position = new Vector3(x, y, z);
                        bool tooClose = false;

                        foreach (var pos in treeInstantiatedPositions)
                        {
                            if (Vector3.Distance(pos, position) < minTreeDistance)
                            {
                                tooClose = true;
                                break;
                            }
                        }
                        foreach (var pos in rockInstantiatedPositions)
                        {
                            if (Vector3.Distance(pos, position) < minTreeDistance)
                            {
                                tooClose = true;
                                break;
                            }
                        }

                        if (tooClose)
                        {
                            continue;
                        }

                        float scale = (float)random.NextDouble() * (treeMaxScale - treeMinScale) + treeMinScale;
                        int treePrefabIndex = random.Next(1, 5);

                        GameObject treePrefab = null;

                        switch (treePrefabIndex)
                        {
                            case 1:
                                if (notUsePrefab1)
                                {
                                    continue;
                                }
                                treePrefab = treePrefab1;
                                break;
                            case 2:
                                if (notUsePrefab2)
                                {
                                    continue;
                                }
                                treePrefab = treePrefab2;
                                break;
                            case 3:
                                if (notUsePrefab3)
                                {
                                    continue;
                                }
                                treePrefab = treePrefab3;
                                break;
                            case 4:
                                if (notUsePrefab4)
                                {
                                    continue;
                                }
                                treePrefab = treePrefab4;
                                break;
                        }
                        Vector3 treePosition = position;
                        treePosition.y += (float)random.NextDouble() * 6.0f + 8.0f;
                        GameObject tree = Instantiate(treePrefab, treePosition, Quaternion.identity);
                        tree.transform.localScale = new Vector3(scale, scale, scale);
                        tree.transform.eulerAngles = new Vector3(-90, (float)random.NextDouble() * 360.0f, 0);   
                        tree.name += "_" + terrain.name; 
                        tree.transform.parent = terrain.transform;
                        treeInstantiatedPositions.Add(position);
                        Undo.RegisterCreatedObjectUndo(tree, "Instantiate Trees");
                    }
                }
            }
        }
        Debug.Log("Trees instantiated");
    }

    private void deleteTrees()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allObjects)
        {
            if (go.name.ToLower().Contains("tree") && go.name.ToLower().Contains(terrain.name.ToLower()))
            {
                DestroyImmediate(go);
            }
        }
    }

    private void deleteRocks()
    {
        GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject go in allObjects)
        {
            if (go.name.ToLower().Contains("rock") && go.name.ToLower().Contains(terrain.name.ToLower()))
            {
                DestroyImmediate(go);
            }
        }
    }
}
