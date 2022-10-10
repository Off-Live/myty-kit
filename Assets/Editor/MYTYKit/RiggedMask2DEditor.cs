using UnityEditor;
using UnityEngine;

using MYTYKit.Components;

namespace MYTYKit
{
    [CustomEditor(typeof(RiggedMask2D))]
    public class RiggedMask2DEditor : UnityEditor.Editor
    {
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
}
