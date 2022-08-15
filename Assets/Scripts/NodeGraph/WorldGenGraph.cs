using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateAssetMenu]
public class WorldGenGraph : NodeGraph
{

    public WorldGenEndNode endNode;

    [HideInInspector]
    public Vector3 noiseGenPoint;
    public void GetEndNode()
    {
        foreach (Node n in nodes)
        {
            if (n is WorldGenEndNode)
            {
                endNode = (WorldGenEndNode)n;
                break;
            }
        }
    }

    private void Awake()
    {
        
    }

    public void GetWorldGenerator()
    {

    }

    //repeating code. i dont care
    public int GetNumChunksLength()
    {
        foreach (Node n in nodes)
        {
            if (n is WorldGenEndNode)
            {
                return ((WorldGenEndNode)n).length;
            }
        }
        return 0; //TODO: make this an actual error message
    }

    public int GetNumChunksWidth()
    {
        foreach (Node n in nodes)
        {
            if (n is WorldGenEndNode)
            {
                return ((WorldGenEndNode)n).width;
            }
        }
        return 0; //TODO: make this an actual error message
    }

    public int GetNumChunksHeight()
    {
        foreach (Node n in nodes)
        {
            if (n is WorldGenEndNode)
            {
                return ((WorldGenEndNode)n).height;
            }
        }
        return 0; //TODO: make this an actual error message
    }


    /*public WorldGenerator.IsoAlgorithm GetIsoSurfaceType()
    {
        return endNode.isoSurfaceAlgorithm;
    }*/
}

[System.Serializable]
public class TerrainMap
{
    float[,,] terrainMap;

    public void Initialize(WorldGenGraph worldGraph)
    {
        int width = (MarchingCubesData.ChunkWidth * worldGraph.GetNumChunksWidth()) + 1;
        int length = (MarchingCubesData.ChunkWidth * worldGraph.GetNumChunksLength()) + 1;
        int height = (MarchingCubesData.ChunkHeight * worldGraph.GetNumChunksHeight()) + 1;
        terrainMap = new float[width, height, length];
    }

    public float GetValue(int x, int y, int z)
    {
        return terrainMap[x,y,z];
    }

    public void SetValue(float value, int x, int y, int z)
    {
        terrainMap[x, y, z] = value;
    }

    public float[,,] GetTerrainMap()
    {
        return terrainMap;
    }

    //0 is x, 1 is y, 2 is z
    public int GetLength(int dimension)
    {
        if (dimension == 0)
            return terrainMap.GetLength(0);
        else if (dimension == 1)
            return terrainMap.GetLength(1);
        else if (dimension == 2)
            return terrainMap.GetLength(2);
        else return terrainMap.GetLength(0);
    }
}