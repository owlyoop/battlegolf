using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("NoiseGenerators/Simplex")]
public class SimplexNoiseNode : NoiseGeneratorNode
{
    public bool isRidged = false;
    public float strength = 1;
    [Range(1, 8)] public int numOctaves;
    public float lacunarity = 1;
    public float persistance = 1;
    public float noiseScale = 0.1f;
    [Range(0, 2f)] public float ridgedAbsStrength = 1f;

    // Use this for initialization
    protected override void Init()
    {
		base.Init();
	}

    private void OnValidate()
    {
        worldGraph = graph as WorldGenGraph;
    }

    public override float Evaluate(Vector3 point)
    {
        //if (noise == null)
            //return 0;
        float currNoise = 0;
        float frequency = noiseScale / 20f;
        float amplitude = 1;


        for (int i = 0; i < numOctaves; i++)
        {
            float v = Noise.Evaluate(point * frequency + center);
            //currNoise = (0.5f - Mathf.Abs(0.5f - v)) * amplitude;
            //currNoise += (v) * .5f * amplitude;
            if (isRidged)
            {
                v = ridgedAbsStrength - Mathf.Abs(v);
                //v = v * v;
            }
            currNoise += v * amplitude;
            amplitude *= persistance;
            frequency *= lacunarity;
        }


        currNoise = Mathf.Clamp(currNoise, -1f, 1f);
        return currNoise * strength;
    }

	// Return the correct value of an output port when requested
	public override object GetValue(NodePort port)
    {
        return Evaluate(worldGraph.noiseGenPoint);
    }
}