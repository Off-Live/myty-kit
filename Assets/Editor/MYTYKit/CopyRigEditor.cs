using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MYTYKit
{
    [CustomEditor(typeof(CopyRig))]
    public class CopyRigEditor : Editor
    {
        public override void OnInspectorGUI()
        {

            DrawDefaultInspector();
            CopyRig obj = (CopyRig)target;
            if (GUILayout.Button("Copy"))
            {
                obj.Copy();
            }
        }
    }
}
