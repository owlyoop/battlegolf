using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class AbsNode : Node
{
    public enum AbsMode { PositiveOnly, NegativeOnly}
    public AbsMode absType = AbsMode.PositiveOnly;

    [Input] public float a;
    [Output] public float result;
    // Use this for initialization
    protected override void Init()
    {
		base.Init();
	}

	// Return the correct value of an output port when requested
	public override object GetValue(NodePort port)
    {
        float a = GetInputValue<float>("a", this.a);
        result = 0f;
        if (port.fieldName == "result")
        {
            if (absType == AbsMode.PositiveOnly)
            {
                if (a < 0)
                    result = Mathf.Abs(a);
            }
            else if (absType == AbsMode.NegativeOnly)
            {
                if (a > 0)
                    result = -a;
            }
        }
        return result;
	}
}