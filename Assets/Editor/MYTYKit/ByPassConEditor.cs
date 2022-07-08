using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(ByPassController))]
public class ByPassConEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        var rigTargetProp = serializedObject.FindProperty("rigTarget");
        if (rigTargetProp.arraySize == 0)
        {
            rigTargetProp.arraySize = 1;
            serializedObject.ApplyModifiedProperties();
        }

        var rigTargetField = new ObjectField();
        var positionField = new PropertyField(serializedObject.FindProperty("position"));
        var scaleField = new PropertyField(serializedObject.FindProperty("scale"));
        var rotationField = new PropertyField(serializedObject.FindProperty("rotation"));

        rigTargetField.label = "Rigging Target";
        rigTargetField.BindProperty(rigTargetProp.GetArrayElementAtIndex(0));
        rigTargetField.objectType = typeof(GameObject);

        root.Add(rigTargetField);
        root.Add(positionField);
        root.Add(scaleField);
        root.Add(rotationField);

        return root;
    }
}
