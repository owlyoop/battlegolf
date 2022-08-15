using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class MaskNode : PreviewableNode
{

    [Input] public float a;
    [Input] public float b;
    [Input] public float mask;
    [Output] public float noiseValue;
	// Use this for initialization
	protected override void Init()
    {
		base.Init();
	}

    public float Evaluate(Vector3 position, float a, float b, float mask)
    {
        //move number into 0,1 range (from -1, 1 range)
        mask = (mask * 0.5f) + 0.5f;

        return Mathf.Lerp(a,b,mask);
    }

	// Return the correct value of an output port when requested
	public override object GetValue(NodePort port)
    {
        float a = GetInputValue<float>("a", this.a);
        float b = GetInputValue<float>("b", this.b);
        float mask = GetInputValue<float>("mask", this.mask);

        return Evaluate(worldGraph.noiseGenPoint, a, b, mask);
	}

    public override void UpdatePreview()
    {
        int s = previewTextureSize;
        Texture2D tex = new Texture2D(s, s);
        float[,] values = new float[s, s];

        for (int x = 0; x < s; x++)
        {
            for (int y = 0; y < s; y++)
            {
                float a = GetInputValue<float>("a", this.a);
                float b = GetInputValue<float>("b", this.b);
                float mask = GetInputValue<float>("mask", this.mask);
                float val = 0;
                if (previewDirection == TexPreviewDirection.Top)
                    worldGraph.noiseGenPoint = new Vector3(x, previewOffset, y);
                else if (previewDirection == TexPreviewDirection.Front)
                    worldGraph.noiseGenPoint = new Vector3(previewOffset, y, x);
                else if (previewDirection == TexPreviewDirection.Right)
                    worldGraph.noiseGenPoint = new Vector3(x, y, previewOffset);
                val = Evaluate(worldGraph.noiseGenPoint, a, b, mask);
                val = Mathf.Clamp(val, -1f, 1f);
                values[x, y] = val;
            }
        }

        for (int x = 0; x < s; x++)
        {
            for (int y = 0; y < s; y++)
            {
                float val = values[x, y];
                float norm = (val + 1f) / 2f;
                tex.SetPixel(x, y, new Color(norm, norm, norm, 1f));
            }
        }

        tex.Apply();
        previewTex = tex;
    }
}