using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class InvertNode : Node
{
    [Input] public float a;
    [Output] public float b;
	// Use this for initialization
	protected override void Init()
    {
		base.Init();
	}

	// Return the correct value of an output port when requested
	public override object GetValue(NodePort port)
    {
        float a = GetInputValue<float>("a", this.a);
        return -a;
	}
}