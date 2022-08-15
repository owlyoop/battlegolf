using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public abstract class NoiseGeneratorNode : PreviewableNode
{
    [Output] public float noiseValue;
    public int seed;
    public Vector3 center;

    //public Noise noise;

    // Use this for initialization
    protected override void Init()
    {
        //noise = new Noise(seed);
        worldGraph = graph as WorldGenGraph;
		base.Init();
	}

    private void OnValidate()
    {
        worldGraph = graph as WorldGenGraph;
    }


    /*public void GenerateTerrainMap()
    {
        for (int x = 0; x < terrainMap.GetLength(0); x++)
        {
            for (int y = 0; y < terrainMap.GetLength(1); y++)
            {
                for (int z = 0; z < terrainMap.GetLength(2); z++)
                {
                    Evaluate(x,y,z);
                }
            }
        }
    }*/

    public abstract float Evaluate(Vector3 point);

    public virtual void RandomizeNoiseSeed(int seed)
    {
        //if (noise != null)
            Noise.Randomize(seed);
        //else
        //{
            //Init();
            //noise.Randomize(seed);
        //}
    }

    public override void UpdatePreview()
    {
        RandomizeNoiseSeed(seed);
        int s = previewTextureSize;
        Texture2D tex = new Texture2D(s, s);
        float[,] values = new float[s, s];

        //float min = float.PositiveInfinity;
        //float max = float.NegativeInfinity;

        for (int a = 0; a < s; a++)
        {
            for (int b = 0; b < s; b++)
            {
                float val = 0;
                if (previewDirection == TexPreviewDirection.Top)
                    val = Evaluate(new Vector3(a, previewOffset, b));
                else if (previewDirection == TexPreviewDirection.Front)
                    val = Evaluate(new Vector3(previewOffset, b, a));
                else val = Evaluate(new Vector3(a, b, previewOffset));

                val = Mathf.Clamp(val, -1f, 1f);

                /*if (val < min)
                    min = val;

                if (val > max)
                    max = val;*/

                values[a, b] = val;
            }
        }


        for (int x = 0; x < s; x++)
        {
            for (int y = 0; y < s; y++)
            {
                float val = values[x, y];
                //float norm = (val - min) / (max - min);
                float norm = (val + 1f) / 2f;
                tex.SetPixel(x,y, new Color(norm, norm, norm, 1f));
            }
        }

        tex.Apply();
        previewTex = tex;
    }

}