using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class MathNode : PreviewableNode
{
    [Input] public float a;
    [Input] public float b;

    [Output] public float result;

    public enum MathType { Add, Subtract, Multiply, Divide, Atan2 }
    public MathType mathType = MathType.Add;


    // Use this for initialization
    protected override void Init()
    {
		base.Init();
	}

	// Return the correct value of an output port when requested
	public override object GetValue(NodePort port)
    {
        float a = GetInputValue<float>("a", this.a);
        float b = GetInputValue<float>("b", this.b);

        // After you've gotten your input values, you can perform your calculations and return a value
        result = 0f;
        if (port.fieldName == "result")
            switch (mathType)
            {
                case MathType.Add: default: result = a + b; break;
                case MathType.Subtract: result = a - b; break;
                case MathType.Multiply: result = a * b; break;
                case MathType.Divide: result = a / b; break;
                case MathType.Atan2: 
                    Mathf.Clamp(a, -1, 1); 
                    Mathf.Clamp(b, -1, 1);
                    //result = Mathf.Atan2(b, a);
                    result = (((Mathf.Atan2(b, a) - 0) * (1 - 0)) / ((Mathf.PI * 2) - 0)) + 0;
                    //result = (result * 2) - 1;
                    break;
            }
        return result;
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
                switch(previewDirection)
                {
                    case TexPreviewDirection.Top: default: worldGraph.noiseGenPoint = new Vector3(x, previewOffset, y); break;
                    case TexPreviewDirection.Front: worldGraph.noiseGenPoint = new Vector3(previewOffset, y, x); break;
                    case TexPreviewDirection.Right: worldGraph.noiseGenPoint = new Vector3(x, y, previewOffset); break;
                }
                
                float a = GetInputValue<float>("a", this.a);
                float b = GetInputValue<float>("b", this.b);
                float val = 0;
                switch (mathType)
                {
                    case MathType.Add: default: val = a + b; break;
                    case MathType.Subtract: val = a - b; break;
                    case MathType.Multiply: val = a * b; break;
                    case MathType.Divide: val = a / b; break;
                    case MathType.Atan2:
                        Mathf.Clamp(a, -1, 1);
                        Mathf.Clamp(b, -1, 1);
                        //result = Mathf.Atan2(b, a);
                        val = (((Mathf.Atan2(b, a) - 0) * (1 - 0)) / ((Mathf.PI * 2) - 0)) + 0;
                        //val = (val * 2) - 1;
                        break;
                }

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