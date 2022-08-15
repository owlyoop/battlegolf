using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using XNodeEditor;
using UnityEditor;

[CustomNodeEditor(typeof(PreviewableNode))]
public class PreviewableNodeEditor : NodeEditor
{
    public override void OnBodyGUI()
    {
        base.OnBodyGUI();
        PreviewableNode node = target as PreviewableNode;

        node.isPreviewDropdown = EditorGUILayout.Foldout(node.isPreviewDropdown, "Preview");

        if (node.isPreviewDropdown)
        {
            if (GUILayout.Button("Update Preview"))
            {
                node.UpdatePreview();
            }
            GUILayout.Label(new GUIContent(node.previewTex));

            node.previewDirection = (PreviewableNode.TexPreviewDirection)EditorGUILayout.EnumPopup(node.previewDirection);

            node.previewOffset = EditorGUILayout.IntField("Preview Offset",node.previewOffset);
            
        }
    }
}
