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
        EditorGUILayout.LabelField(string.Format("Coord:\t(Lat {0:F2} Lon {1:F2})", t.Latitude, t.Longitude));
        EditorGUILayout.LabelField(string.Format("Time:\t{0}", t.LocalTimeHuman));
        EditorGUILayout.LabelField(string.Format("Sun Inc:\t{0:F1} ° (Lat {1:F1}°)", t.SunAngle * Mathf.Rad2Deg, t.Latitude - Sun.Latitude));
        EditorGUILayout.LabelField(string.Format("Temp:\t{0:F1} °C", t.Temperature));
        EditorGUI.indentLevel -= 1;
    }
}
