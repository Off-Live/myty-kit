using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RiggingObject))]
public class RiggingObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var pc = (RiggingObject)target;

        //if (pc.processor != null)
        //{
        //    var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none);
        //    int selectedIndex = EditorGUI.Popup(rect, "Model", pc.GetLastSelected(), pc.processor.GetModelList());
        //    if (selectedIndex >= 0)
        //    {
        //        var model = serializedObject.FindProperty("_selectedModel");
        //        model.stringValue = pc.processor.GetModelList()[selectedIndex];
        //        pc.SetLastSelected(selectedIndex);
        //        serializedObject.ApplyModifiedProperties();
        //    }
        //}
    }
}
