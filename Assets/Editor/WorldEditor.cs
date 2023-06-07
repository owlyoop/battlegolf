using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WorldGenerator))]
public class WorldEditor : Editor
{
    WorldGenerator world;
    Editor shapeEditor;
    Editor colourEditor;

    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
            if (check.changed)
            {
                //world.GenerateWorld();
            }
        }

        if (GUILayout.Button("Generate World"))
        {
            world.GenerateWorld(4);
        }
        if (GUILayout.Button("Delete World"))
        {
            world.DestroyWorldInEditor();
        }

        //DrawSettingsEditor(world.shapeSettings, world.OnShapeSettingsUpdated, ref world.shapeSettingsFoldout, ref shapeEditor);
    }

    void DrawSettingsEditor(Object settings, System.Action onSettingsUpdated, ref bool foldout, ref Editor editor)
    {
        if (settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                if (foldout)
                {
                    CreateCachedEditor(settings, null, ref editor);
                    editor.OnInspectorGUI();

                    if (check.changed)
                    {
                        if (onSettingsUpdated != null)
                        {
                            //onSettingsUpdated();
                        }
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        world = (WorldGenerator)target;
    }

}
