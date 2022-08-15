using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class CavesNode : PreviewableNode
{
    [Input] public float noiseInput;
    [Output] public float result;

    protected override void Init()
    {
        base.Init();
    }

    float Evaluate(Vector3 pos)
    {
        float y = pos.y * pos.y;
        return 0;
    }

    public override object GetValue(NodePort port)
    {
        return Evaluate(worldGraph.noiseGenPoint);
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
                switch (previewDirection)
                {
                    case TexPreviewDirection.Top: default: worldGraph.noiseGenPoint = new Vector3(x, previewOffset, y); break;
                    case TexPreviewDirection.Front: worldGraph.noiseGenPoint = new Vector3(previewOffset, y, x); break;
                    case TexPreviewDirection.Right: worldGraph.noiseGenPoint = new Vector3(x, y, previewOffset); break;
                }

                float val = GetInputValue<float>("noiseInput", this.noiseInput);

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
