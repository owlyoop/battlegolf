using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("NoiseGenerators/Ridged")]
public class RidgedNoiseNode : NoiseGeneratorNode
{

    public float strength = 1;
    [Range(1, 8)] public int numOctaves = 4;
    public float lacunarity = 1;
    public float persistance = 1;
    public float noiseScale = 1;
    public float noiseWeight = 5;
    public float weightMultiplier = 1;
    public float floorOffset = 0;

    float unflooredVal;

    // Use this for initialization
    protected override void Init()
    {
        base.Init();
    }


    public override float Evaluate(Vector3 point)
    {
        float currNoise = 0;
        float frequency = noiseScale / 20f;
        float amplitude = 1;
        float weight = 1;

        for (int i = 0; i < numOctaves; i++)
        {
            float n = Noise.Evaluate(point * frequency + center);
            float v = 1 - Mathf.Abs(n);
            v = v * v;
            v *= weight;
            weight = Mathf.Max(Mathf.Min(v * weightMultiplier, 1), 0);
            currNoise += v * amplitude;
            amplitude *= persistance;
            frequency *= lacunarity;
        }

        unflooredVal = currNoise;

        /*for (int i = 0; i < numOctaves; i++)
        {
            float v = noise.Evaluate(point * frequency + center);
            currNoise += 2 * (0.5f - Mathf.Abs(0.5f - v));
            //currNoise += (v) * .5f * amplitude;
            currNoise += v * amplitude;
            amplitude *= persistance;
            frequency *= lacunarity;
        }*/

        float finalValue = -(point.y + floorOffset) + currNoise * noiseWeight + ((point.y * MarchingCubesData.ChunkHeight) % 1);
        //finalValue = Mathf.Clamp(finalValue, -2f, 2f);
        //finalValue /= 2f;
        return finalValue * strength;

        /*Debug.Log(currNoise);
        currNoise = Mathf.Clamp(currNoise, -1f, 1f);
        return currNoise * strength;*/
    }

    // Return the correct value of an output port when requested
    public override object GetValue(NodePort port)
    {
        return Evaluate(worldGraph.noiseGenPoint);
    }
}
