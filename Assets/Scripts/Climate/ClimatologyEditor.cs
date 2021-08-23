using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Climatology))]
public class ClimatologyEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var t = target as Climatology;
        EditorGUILayout.LabelField("Current Climate");
        EditorGUI.indentLevel += 1;
        EditorGUILayout.LabelField(string.Format("{0:F1} °C", t.Temperature));
        EditorGUI.indentLevel -= 1;
    }
}
