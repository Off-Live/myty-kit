using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RiggedMask2D))]
public class RiggedMask2DEditor : Editor
{
    // Start is called before the first frame update
    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();
        RiggedMask2D mask = (RiggedMask2D)target;
        if (GUILayout.Button("Fit to source sprite"))
        {
            mask.Fit();
        }
    }
}
