using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("")]
public class FilterNode : PreviewableNode
{
    [Input] public float noiseInput;
    [Output] public float noiseOutput;
    protected WorldGenerator worldGen;

    protected float[,,] map;
    // Use this for initialization
    protected override void Init()
    {
        base.Init();
        worldGen = FindObjectOfType<WorldGenerator>();
        if (worldGen != null)
            worldGen.Initialize();
        if (worldGraph != null)
            CreateTerrainMap();
	}

    private void Awake()
    {
        Init();
    }

    private void OnValidate()
    {
        worldGraph = graph as WorldGenGraph;
    }

    public void CreateTerrainMap()
    {
        int width = (MarchingCubesData.ChunkWidth * worldGraph.GetNumChunksWidth()) + 1;
        int length = (MarchingCubesData.ChunkWidth * worldGraph.GetNumChunksLength()) + 1;
        int height = (MarchingCubesData.ChunkHeight * worldGraph.GetNumChunksHeight()) + 1;
        map = new float[width, height, length];

        Vector3 temp = worldGraph.noiseGenPoint;

        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int z = 0; z < map.GetLength(2); z++)
                {
                    worldGraph.noiseGenPoint = new Vector3(x, y, z);
                    map[x, y, z] = GetInputValue<float>("noiseInput", this.noiseInput);
                }
            }
        }

        worldGraph.noiseGenPoint = temp;
    }
    
    public override void UpdatePreview()
    {
        CreateTerrainMap();
    }
}