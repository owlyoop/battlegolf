using ProceduralNoiseProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("NoiseGenerators/Voronoi")]
public class VoronoiNoiseNode : NoiseGeneratorNode
{
    public VoronoiNoise vnoise;

    public VORONOI_DISTANCE voronoiDistance;
    public VORONOI_COMBINATION voronoiCombination;

    public float frequency = 1;
    public float amplitude = 1f;


    // Use this for initialization
    protected override void Init()
    {
        vnoise = new VoronoiNoise(seed, frequency);
        vnoise.Distance = voronoiDistance;
        vnoise.Combination = voronoiCombination;
        base.Init();
	}
    public override float Evaluate(Vector3 point)
    {
        vnoise.Distance = voronoiDistance;
        vnoise.Combination = voronoiCombination;
        vnoise.Frequency = frequency;
        vnoise.Amplitude = amplitude;
        return vnoise.Sample3D(point.x, point.y, point.z);
    }



    public override void RandomizeNoiseSeed(int seed)
    {
        if (vnoise != null)
            vnoise.UpdateSeed(seed);
    }
    // Return the correct value of an output port when requested
    public override object GetValue(NodePort port)
    {
        return Evaluate(worldGraph.noiseGenPoint);
	}

}