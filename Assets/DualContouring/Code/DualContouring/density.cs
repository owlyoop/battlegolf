using Code.Noise;
using ProceduralNoiseProject;
using UnityEngine;

public static class glm
{
    //static Noise noiseGen = new Noise();
    public static float Sphere(Vector3 worldPosition, Vector3 origin, float radius)
    {
        return Vector3.Magnitude(worldPosition - origin) - radius;
    }

    public static float Cuboid(Vector3 worldPosition, Vector3 origin, Vector3 halfDimensions)
    {
        Vector3 local_pos = worldPosition - origin;
        Vector3 pos = local_pos;

        Vector3 d = new Vector3(Mathf.Abs(pos.x), Mathf.Abs(pos.y), Mathf.Abs(pos.z)) - halfDimensions;
        float m = Mathf.Max(d.x, Mathf.Max(d.y, d.z));
        return Mathf.Min(m, Vector3.Magnitude(d.magnitude > 0 ? d : Vector3.zero));
    }

    public static float FractalNoise(int octaves, float frequency, float lacunarity, float persistence, Vector3 position)
    {
        float SCALE = 1.0f / 128.0f;
        Vector3 p = position * SCALE;
        float noise = 0.0f;

        float amplitude = 1.0f;
        p *= frequency;

        for (int i = 0; i < octaves; i++)
        {
            //noise += PNoise.Perlin(p.x, p.y, p.z) * amplitude;
            noise += Noise.Evaluate(p);
            p *= lacunarity;
            amplitude *= persistence;
        }

        // move into [0, 1] range
        //return 0.5f + (0.5f * noise);
        return noise;
    }

    public static float RidgedNoise(Vector3 position, int octaves, float lacunarity, float persistance, float scale, float noiseWeight, float weightMultiplier, float floorOffset)
    {
        Noise.Randomize(1127);
        float noise = 0;
        float frequency = scale / 20f;
        float amplitude = 1;
        float weight = 1;

        for (int i = 0; i < octaves; i++)
        {
            float n = Noise.Evaluate(position * frequency);
            float v = 1 - Mathf.Abs(n);
            v = v * v;
            v *= weight;
            weight = Mathf.Max(Mathf.Min(v * weightMultiplier, 1), 0);
            noise += v * amplitude;
            amplitude *= persistance;
            frequency *= lacunarity;
        }

        float finalValue = -(position.y + floorOffset) + noise * noiseWeight + ((position.y * 32) % 1);
        finalValue = Mathf.Clamp(finalValue, -2f, 2f);
        finalValue /= 2f;
        return -finalValue;
    }

    public static float WorleyNoise(Vector3 position)
    {
        WorleyNoise wnoise = new WorleyNoise(12234, 0.05f, 1);
        wnoise.Distance = VORONOI_DISTANCE.EUCLIDIAN;
        wnoise.Combination = VORONOI_COMBINATION.D0;
        float n = wnoise.Solid3D(position.x, position.y, position.z);
        return n;
    }

    public static float Density_Func(Vector3 worldPosition)
    {
        float MAX_HEIGHT = 20.0f;
        float noise = FractalNoise(4, 0.8343f, 3.2324f, 0.88324f, new Vector3(worldPosition.x, worldPosition.y, worldPosition.z));
        float rNoise = RidgedNoise(worldPosition, 2, 4f, 2f, 1f, 5f, 1.4f, -0.1f);
        float terrain = worldPosition.y - (MAX_HEIGHT * noise);
        float wNoise = WorleyNoise(worldPosition);

        float cube = Cuboid(worldPosition, new Vector3(-4.0f, 10.0f, -4.0f), new Vector3(12.0f, 12.0f, 12.0f));
        float sphere = Sphere(worldPosition, new Vector3(15.0f, 2.5f, 1.0f), 16.0f);

        return terrain;
        //return Mathf.Max(-cube, Mathf.Min(sphere, terrain));
    }
}