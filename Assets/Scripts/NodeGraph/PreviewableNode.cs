using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateNodeMenu("")]
public class PreviewableNode : Node
{

    [HideInInspector] public Texture2D previewTex;
    [HideInInspector] public bool isPreviewDropdown = false;
    [HideInInspector] public int previewTextureSize = 100;
    [HideInInspector] public int previewOffset;

    protected WorldGenGraph worldGraph;

    public enum TexPreviewDirection { Top, Front, Right }
    [HideInInspector] public TexPreviewDirection previewDirection;

    public virtual void UpdatePreview()
    {

    }

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
        worldGraph = graph as WorldGenGraph;
        previewTextureSize = 100;
        base.Init();
	}

	// Return the correct value of an output port when requested
	public override object GetValue(NodePort port)
    {
		return null; // Replace this
	}
}