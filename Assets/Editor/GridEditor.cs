using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Grid))]
public class GridEditor : Editor
{
    Grid grid;

    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            base.OnInspectorGUI();
            if (check.changed)
            {
                grid.GenerateGrid();
            }
        }

        if (GUILayout.Button("Generate Grid"))
        {
            grid.GenerateGrid();
        }

    }

    private void OnEnable()
    {
        grid = (Grid)target;
    }
}
