using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(AvatarSelector))]
public class AvatarSelectorEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();
        IntegerField idField = new IntegerField();
        PropertyField templateField = new PropertyField();
        idField.label = "Avatar ID";
        idField.BindProperty(serializedObject.FindProperty("id"));
        idField.isDelayed = true;
        idField.RegisterValueChangedCallback((ChangeEvent<int> e) =>
        {
            (target as AvatarSelector).Configure();
        });

        templateField.BindProperty(serializedObject.FindProperty("templates"));
        root.Add(templateField);
        root.Add(idField);
        return root;
    }
   
}
