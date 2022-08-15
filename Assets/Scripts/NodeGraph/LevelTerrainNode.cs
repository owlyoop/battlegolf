using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class LevelTerrainNode : PreviewableNode
{
    public int startRoofHeight = 16; //The starting Y value the terrain levelling should start at.
    public int startFloorHeight = 12;
    public float roofStrength = 1;
    public float floorStrength = 1;

    //public int floorOffset;
    //public int noiseWeight;

    [Input] public float noiseValueInput;
    [Output] public float noiseValueOutput;

	// Use this for initialization
	protected override void Init()
    {
		base.Init();
	}

    float Evaluate(Vector3 pos)
    {
        float noiseValue = 0;
        float maxHeight = ((float)MarchingCubesData.ChunkHeight * (float)worldGraph.endNode.height);
        float maxWidth = ((float)MarchingCubesData.ChunkWidth * (float)worldGraph.endNode.width);
        float maxLength = ((float)MarchingCubesData.ChunkWidth * (float)worldGraph.endNode.length);
        float difference = maxHeight - (float)startRoofHeight;

        if (pos.y >= maxHeight)
        {
            return -1f;
        }
        if (pos.y <= 0)
        {
            return 1f;
        }
        if (pos.x >= maxWidth)
            return -1f;
        if (pos.x <= 0)
            return -1f;
        if (pos.z >= maxLength)
            return -1f;
        if (pos.z <= 0)
            return -1f;
        
        if (pos.y >= startRoofHeight)
        {
            noiseValue = Mathf.InverseLerp((float)startRoofHeight, maxHeight, pos.y);
            if (noiseValue <= 0.5f)
                noiseValue = noiseValue * (noiseValue * 2f);
            noiseValue = -noiseValue * roofStrength;
        }

        if (pos.y <= startFloorHeight)
        {
            noiseValue += Mathf.InverseLerp((float)startFloorHeight, 0, pos.y);
            if (noiseValue <= 0.5f)
                noiseValue = noiseValue * (noiseValue * 2f);
            noiseValue *= floorStrength;
        }

        return noiseValue + GetInputValue<float>("noiseValueInput", this.noiseValueInput);
    }

	// Return the correct value of an output port when requested
	public override object GetValue(NodePort port)
    {
        return Evaluate(worldGraph.noiseGenPoint);
	}

    public override void UpdatePreview()
    {
        int s = previewTextureSize;
        int h = (MarchingCubesData.ChunkHeight * worldGraph.endNode.height);
        Texture2D tex = new Texture2D(s, s);
        float[,] values = new float[s, s];


        if (previewDirection != TexPreviewDirection.Top)
        {
            tex = new Texture2D(s, h);
            values = new float[s, h];

            for (int a = 0; a < s; a++)
            {
                for (int b = 0; b < h; b++)
                {
                    float val = 0;
                    if (previewDirection == TexPreviewDirection.Front)
                    {
                        worldGraph.noiseGenPoint = new Vector3(previewOffset, b, a);
                        val = Evaluate(worldGraph.noiseGenPoint);
                    }
                    else if (previewDirection == TexPreviewDirection.Right)
                    {
                        worldGraph.noiseGenPoint = new Vector3(a, b, previewOffset);
                        val = Evaluate(worldGraph.noiseGenPoint);
                    }
                    val = Mathf.Clamp(val, -1f, 1f);
                    values[a, b] = val;
                }
            }

            for (int x = 0; x < s; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float val = values[x, y];
                    float norm = (val + 1f) / (2f);
                    if (norm >= 0.5f)
                        tex.SetPixel(x, y, new Color(norm, 1, norm, 1f));
                    else
                        tex.SetPixel(x, y, new Color(0, 0, norm, 1f));
                }
            }
        }
        else
        {
            for (int a = 0; a < s; a++)
            {
                for (int b = 0; b < s; b++)
                {
                    worldGraph.noiseGenPoint = new Vector3(a, previewOffset, b);
                    float val = Evaluate(worldGraph.noiseGenPoint);

                    values[a, b] = val;
                }
            }

            for (int x = 0; x < s; x++)
            {
                for (int y = 0; y < s; y++)
                {
                    float val = values[x, y];
                    float norm = (val + 1f) / (2f);
                    if (norm >= 0.5f)
                        tex.SetPixel(x, y, new Color(norm, 1, norm, 1f));
                    else
                        tex.SetPixel(x, y, new Color(0, 0, norm, 1f));
                }
            }
        }

        

        tex.Apply();
        previewTex = tex;
    }
}