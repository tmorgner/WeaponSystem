using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class SplitTerrain : EditorWindow
{
    [MenuItem("Tools/Terrain Splitter/Split Terrain")]
    static void ShowToolWindow()
    {
        GetWindow<SplitTerrain>();
    }

    void OnEnable()
    {
        if (transform == null && Selection.activeGameObject != null)
        {
            transform = Selection.activeGameObject.GetComponent<Terrain>();
        }
    }

    Terrain transform;
    int splitCounts = 4; // Match with TerrainManager if using

    public void OnGUI()
    {
        transform = (Terrain) EditorGUILayout.ObjectField("Terrain", transform, typeof(Terrain), true);
        splitCounts = EditorGUILayout.IntSlider("Split Divisions", splitCounts, 1, 6);
        EditorGUILayout.LabelField("This will result in " + GetNumberOfSplits() + " terrain tiles along each axis.");
        if (transform != null)
        {
            var segmentSize = transform.terrainData.size / GetNumberOfSplits();
            EditorGUILayout.LabelField("The terrain tile will have a size of (" + segmentSize.x + "," + segmentSize.z + ").");
        }
        else
        {
            EditorGUILayout.LabelField("<Please add a terrain first>");
        }

        var enabledState = GUI.enabled;
        if (transform == null)
        {
            GUI.enabled = false;
        }

        if (GUILayout.Button("Split!"))
        {
            ProcessTerrain();
            ReconnectTerrains();
        }

        GUI.enabled = enabledState;
    }

    void ReconnectTerrains()
    {
        for (var x = 0; x < splitCounts; x++)
        {
            for (var z = 0; z < splitCounts; z++)
            {
                var center = GameObject.Find($"{transform.name}{x}_{z}");
                var left = GameObject.Find($"{transform.name}{x - 1}_{z}");
                var top = GameObject.Find($"{transform.name}{x}_{z + 1}");
                StitchTerrain(center, left, top);
            }
        }
    }

    void ProcessTerrain()
    {
        var splits = GetNumberOfSplits();

        try
        {
            for (var x = 0; x < splits; x++)
            {
                for (var z = 0; z < splits; z++)
                {
                    var progress = ((x * splits) + z) / ((float) splits * splits);
                    EditorUtility.DisplayProgressBar("Splitting Terrain", $"Copying terrain data for Segment ({x},{z})", progress);

                    CopyTerrain(transform, splits, new Vector2Int(x, z));
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        Terrain.SetConnectivityDirty();
    }

    int GetNumberOfSplits()
    {
        var potency = Mathf.Max(splitCounts - 1, 0);
        var splits = (int) Mathf.Pow(2, potency);
        return splits;
    }

    void CopyTerrain(Terrain sourceTerrain, int splits, Vector2Int segment)
    {
        if (segment.x < 0 || segment.y < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(segment), segment, "Cannot work with negative bounds on terrain data.");
        }

        var name = $"{sourceTerrain.gameObject.name}_[{segment.x},{segment.y}]";
        if (!CreateTerrainAsset(name, out var targetTerrain))
        {
            Debug.LogWarning("Asset already exists at this path.");
            return;
        }

        CopyTerrainConfiguration(sourceTerrain, targetTerrain, splits);
        var targetTerrainData = targetTerrain.terrainData;
        var sourcePosition = sourceTerrain.transform.position;
        var targetTerrainPosition = new Vector3(sourcePosition.x + segment.x * targetTerrainData.size.x,
                                                sourcePosition.y,
                                                sourcePosition.z + segment.y * targetTerrainData.size.z);
        targetTerrain.gameObject.transform.position = targetTerrainPosition;
        targetTerrain.gameObject.name = name;

        CopyHeightData(sourceTerrain, segment, splits, targetTerrainData);
        CopyDetailLayer(sourceTerrain, segment, splits, targetTerrainData);
        CopySplatMaps(sourceTerrain, segment, splits, targetTerrainData);
        CopyTreeInstances(sourceTerrain, segment, splits, targetTerrain);

        EditorUtility.SetDirty(targetTerrainData);
        targetTerrain.Flush();
    }

    static void CopyHeightData(Terrain sourceTerrain,
                               Vector2Int segment,
                               int splits,
                               TerrainData targetTerrainData)
    {
        var heightMapResolution = targetTerrainData.heightmapResolution;
        var xMin = segment.x * (heightMapResolution - 1);
        var zMin = segment.y * (heightMapResolution - 1);

        var heightValues = sourceTerrain.terrainData.GetHeights(xMin, zMin, heightMapResolution, heightMapResolution);
        targetTerrainData.SetHeightsDelayLOD(0, 0, heightValues);
    }

    static void CopyTreeInstances(Terrain sourceTerrain, Vector2Int segment, int splits, Terrain targetTerrain)
    {
        var sourceTerrainData = sourceTerrain.terrainData;
        var bounds = new Rect(segment.x / (float) splits,
                              segment.y / (float) splits,
                              1 / (float) splits,
                              1 / (float) splits);

        var treeInstances = new List<TreeInstance>();
        foreach (var treeInstance in sourceTerrainData.treeInstances)
        {
            var treeInstancePosition = treeInstance.position;
            if (!bounds.Contains(new Vector3(treeInstancePosition.x, treeInstancePosition.z)))
            {
                continue;
            }

            var posX = (treeInstancePosition.x - bounds.x) * splits;
            var posZ = (treeInstancePosition.z - bounds.y) * splits;

            var copy = treeInstance;
            copy.position = new Vector3(posX, treeInstancePosition.y, posZ);
            treeInstances.Add(copy);
        }

        targetTerrain.terrainData.treeInstances = treeInstances.ToArray();
    }

    /// <summary>
    ///  This manual copying is ugly, but for some reason Unity does not return
    ///  a sub-section of the alpha texture, it instead returns a friggin'
    ///  scaled down version of that texture.
    /// </summary>
    /// <param name="sourceTerrain"></param>
    /// <param name="segment"></param>
    /// <param name="splits"></param>
    /// <param name="targetTerrainData"></param>
    static void CopySplatMaps(Terrain sourceTerrain,
                              Vector2Int segment,
                              int splits,
                              TerrainData targetTerrainData)
    {
        var xBase = segment.x * targetTerrainData.alphamapResolution;
        var yBase = segment.y * targetTerrainData.alphamapResolution;
        var alphaMaps = sourceTerrain.terrainData.GetAlphamaps(xBase, yBase, targetTerrainData.alphamapResolution, targetTerrainData.alphamapResolution);
        targetTerrainData.SetAlphamaps(0, 0, alphaMaps);
    }

    static void CopyDetailLayer(Terrain sourceTerrain,
                                Vector2Int segment,
                                int split,
                                TerrainData targetTerrainData)
    {
        for (var layer = 0; layer < sourceTerrain.terrainData.detailPrototypes.Length; layer++)
        {
            var xBase = segment.x * targetTerrainData.detailWidth;
            var yBase = segment.y * targetTerrainData.detailHeight;

            var detailLayer = sourceTerrain.terrainData.GetDetailLayer(xBase,
                                                                       yBase,
                                                                       targetTerrainData.detailWidth,
                                                                       targetTerrainData.detailHeight,
                                                                       layer);
            targetTerrainData.SetDetailLayer(0, 0, layer, detailLayer);
        }
    }

    static void CopyTerrainConfiguration(Terrain source, Terrain target, int splits)
    {
        target.treeDistance = source.treeDistance;
        target.treeBillboardDistance = source.treeBillboardDistance;
        target.treeCrossFadeLength = source.treeCrossFadeLength;
        target.treeMaximumFullLODCount = source.treeMaximumFullLODCount;
        target.detailObjectDistance = source.detailObjectDistance;
        target.heightmapPixelError = source.heightmapPixelError;
        target.heightmapMaximumLOD = source.heightmapMaximumLOD;
        target.basemapDistance = source.basemapDistance;
        target.lightmapIndex = source.lightmapIndex;
        target.realtimeLightmapIndex = source.realtimeLightmapIndex;
        target.lightmapScaleOffset = source.lightmapScaleOffset;
        target.realtimeLightmapScaleOffset = source.realtimeLightmapScaleOffset;
        target.freeUnusedRenderingResources = source.freeUnusedRenderingResources;
        target.castShadows = source.castShadows;
        target.reflectionProbeUsage = source.reflectionProbeUsage;
        target.materialType = source.materialType;
        target.materialTemplate = source.materialTemplate;
        target.legacySpecular = source.legacySpecular;
        target.legacyShininess = source.legacyShininess;
        target.drawHeightmap = source.drawHeightmap;
        target.allowAutoConnect = source.allowAutoConnect;
        target.groupingID = source.groupingID + 1;
        target.drawInstanced = source.drawInstanced;
        target.drawTreesAndFoliage = source.drawTreesAndFoliage;
        target.patchBoundsMultiplier = source.patchBoundsMultiplier;
        target.treeLODBiasMultiplier = source.treeLODBiasMultiplier;
        target.collectDetailPatches = source.collectDetailPatches;
        target.editorRenderFlags = source.editorRenderFlags;
        target.bakeLightProbesForTrees = source.bakeLightProbesForTrees;
        target.deringLightProbesForTrees = source.deringLightProbesForTrees;
        target.preserveTreePrototypeLayers = source.preserveTreePrototypeLayers;

        var targetTerrainData = target.terrainData;
        var sourceTerrainData = source.terrainData;

        targetTerrainData.terrainLayers = sourceTerrainData.terrainLayers;
        targetTerrainData.treePrototypes = sourceTerrainData.treePrototypes;
        targetTerrainData.detailPrototypes = sourceTerrainData.detailPrototypes;
        targetTerrainData.baseMapResolution = sourceTerrainData.baseMapResolution / splits;
        targetTerrainData.alphamapResolution = sourceTerrainData.alphamapResolution / splits;
        targetTerrainData.heightmapResolution = (sourceTerrainData.heightmapResolution - 1) / splits + 1;
        targetTerrainData.SetDetailResolution(sourceTerrainData.detailResolution / splits, sourceTerrainData.detailResolutionPerPatch * splits);
        targetTerrainData.size = new Vector3(sourceTerrainData.size.x / splits, sourceTerrainData.size.y, sourceTerrainData.size.z / splits);
    }

    static bool CreateTerrainAsset(string assetName, out Terrain targetTerrain)
    {
        var sceneObject = GameObject.Find(assetName);
        if (sceneObject != null)
        {
            targetTerrain = sceneObject.GetComponent<Terrain>();
            if (targetTerrain != null)
            {
                return true;
            }
        }

        var scene = SceneManager.GetActiveScene();
        var scenePath = scene.path;
        if (string.IsNullOrWhiteSpace(scenePath))
        {
            scenePath = "Assets/Resources";
        }
        else
        {
            scenePath = Path.GetDirectoryName(scenePath);
            if (string.IsNullOrWhiteSpace(scenePath))
            {
                scenePath = "Assets/Resources";
            }
        }

        if (!AssetDatabase.IsValidFolder(scenePath))
        {
            Directory.CreateDirectory(scenePath);
        }

        var fileName = $"{scenePath}/{assetName}.asset";
        if (File.Exists(fileName))
        {
            var terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>(fileName);
            if (terrainData != null)
            {
                var terrainObject = Terrain.CreateTerrainGameObject(terrainData);
                targetTerrain = terrainObject.GetComponent<Terrain>();
                return true;
            }

            targetTerrain = default;
            return false;
        }

        var targetTerrainData = new TerrainData();
        var gameObject = Terrain.CreateTerrainGameObject(targetTerrainData);
        targetTerrain = gameObject.GetComponent<Terrain>();

        // Must do this before Splat
        AssetDatabase.CreateAsset(targetTerrainData, fileName);
        return true;
    }

    void StitchTerrain(GameObject center, GameObject left, GameObject top)
    {
        if (center == null)
            return;
        var centerTerrain = center.GetComponent<Terrain>();
        var centerHeights = centerTerrain.terrainData.GetHeights(0, 0, centerTerrain.terrainData.heightmapWidth, centerTerrain.terrainData.heightmapHeight);
        if (top != null)
        {
            var topTerrain = top.GetComponent<Terrain>();
            var topHeights = topTerrain.terrainData.GetHeights(0, 0, topTerrain.terrainData.heightmapWidth, topTerrain.terrainData.heightmapHeight);
            if (topHeights.GetLength(0) != centerHeights.GetLength(0))
            {
                Debug.Log("Terrain sizes must be equal");
                return;
            }

            for (var i = 0; i < centerHeights.GetLength(1); i++)
            {
                centerHeights[centerHeights.GetLength(0) - 1, i] = topHeights[0, i];
            }
        }

        if (left != null)
        {
            var leftTerrain = left.GetComponent<Terrain>();
            var leftHeights = leftTerrain.terrainData.GetHeights(0, 0, leftTerrain.terrainData.heightmapWidth, leftTerrain.terrainData.heightmapHeight);
            if (leftHeights.GetLength(0) != centerHeights.GetLength(0))
            {
                Debug.Log("Terrain sizes must be equal");
                return;
            }

            for (var i = 0; i < centerHeights.GetLength(0); i++)
            {
                centerHeights[i, 0] = leftHeights[i, leftHeights.GetLength(1) - 1];
            }
        }

        centerTerrain.terrainData.SetHeights(0, 0, centerHeights);
    }
}
