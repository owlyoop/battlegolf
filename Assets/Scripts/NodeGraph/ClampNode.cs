using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class ClampNode : Node
{
    public float min = -0.5f;
    public float max = 0.5f;

    [Input] public float a;
    [Output] public float result;

    public override object GetValue(NodePort port)
    {
        a = GetInputValue<float>("a", this.a);
        return Mathf.Clamp(a, min, max);
    }
}
