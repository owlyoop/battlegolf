using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class WorldGenEndNode : Node
{
    public int width;
    public int length;
    public int height;

    public float surfaceLevel;

    //public WorldGenerator.IsoAlgorithm isoSurfaceAlgorithm;

    [Input] public float finalNoiseValue;
    //[Input] public TerrainMap finalTerrainMap;

    public List<NoiseGeneratorNode> noiseNodes = new List<NoiseGeneratorNode>();
    public List<FilterNode> filterNodes = new List<FilterNode>();

    WorldGenGraph worldGraph;

	// Use this for initialization
	protected override void Init()
    {
		base.Init();
        worldGraph = graph as WorldGenGraph;
        worldGraph.noiseGenPoint = new Vector3(0, 0, 0);
        GetAllNoiseNodes();
        GetAllFilterNodes();
	}


    public void GetAllNoiseNodes()
    {
        noiseNodes.Clear();
        foreach (Node n in this.graph.nodes)
        {
            if (n is NoiseGeneratorNode)
            {
                noiseNodes.Add((NoiseGeneratorNode)n);
            }
        }

        if (noiseNodes.Count > 0)
        {
            foreach(NoiseGeneratorNode n in noiseNodes)
            {
                if (n != null)
                    n.RandomizeNoiseSeed(n.seed);
            }
        }
    }

    public void GetAllFilterNodes()
    {
        filterNodes.Clear();
        foreach(Node n in this.graph.nodes)
        {
            if (n is FilterNode)
            {
                filterNodes.Add((FilterNode)n);
            }
        }
        if (filterNodes.Count > 0)
        {
            foreach(FilterNode n in filterNodes)
            {
                if (n != null)
                {
                    n.CreateTerrainMap();
                }
            }
        }
    }

    public float GenerateTerrainMap(Vector3 point)
    {
        worldGraph.noiseGenPoint = point;

        /*if (isoSurfaceAlgorithm == WorldGenerator.IsoAlgorithm.DualContouring)
        {
            float n = GetInputValue<float>("finalNoiseValue");
            return 0.5f + (0.5f * n);
        }
        else */
        return GetInputValue<float>("finalNoiseValue");
    }

	// Return the correct value of an output port when requested
	public override object GetValue(NodePort port)
    {
		return null; // Replace this
	}

    
}