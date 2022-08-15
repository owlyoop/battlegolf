using ProceduralNoiseProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("NoiseGenerators/Worley")]
public class WorleyNoiseNode : NoiseGeneratorNode
{
    
    public WorleyNoise wnoise;

    public VORONOI_DISTANCE distance;
    public VORONOI_COMBINATION combination;

    public bool solidCells = false;

    public float jitter = 1;
    public float frequency = 0.1f;
    public float amplitude = 1;


    private void Awake()
    {
        Init();
    }

    private void OnValidate()
    {
        worldGraph = graph as WorldGenGraph;
    }

    protected override void Init()
    {
        wnoise = new WorleyNoise(seed, frequency, jitter);
        wnoise.Distance = distance;
        wnoise.Combination = combination;
        wnoise.Frequency = frequency;
        wnoise.Amplitude = amplitude;
        wnoise.Jitter = jitter;
        base.Init();
	}

    public override float Evaluate(Vector3 point)
    {
        if (wnoise == null)
        {
            return 0;
        }
        wnoise.Frequency = frequency;
        wnoise.Amplitude = amplitude;
        wnoise.Jitter = jitter;
        wnoise.Distance = distance;
        wnoise.Combination = combination;

        float noiseValue = 0;

        if (solidCells)
        {
            noiseValue = wnoise.Solid3D(point.x, point.y, point.z);

            float sum = 0;
            /*for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    for (int z = -1; z < 2; z++)
                    {
                        sum += wnoise.Solid3D(point.x + x, point.y + y, point.z + z);
                    }
                }
            }

            noiseValue = sum * 0.03703703703f;*/
        }

        else
        {
            noiseValue = wnoise.Sample3D(point.x, point.y, point.z);
        }
        
        Mathf.Clamp(noiseValue, -1f, 1f);
        return noiseValue;
    }

    public override void RandomizeNoiseSeed(int seed)
    {
        if (wnoise != null)
            wnoise.UpdateSeed(seed);
    }

    // Return the correct value of an output port when requested
    public override object GetValue(NodePort port)
    {
        if (worldGraph.noiseGenPoint == null)
            Debug.Log("noisegenpoint null");
        return Evaluate(worldGraph.noiseGenPoint);
    }
}