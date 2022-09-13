using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(MotionTemplateMapper))]
public class MotionTemplateEditor:Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        var templateField = new PropertyField();
        var button = new Button();
        templateField.BindProperty(serializedObject.FindProperty("templates"));
        
        button.text = "Autobuild";
        button.clicked += () =>
        {
            var motionTemplate = target as MotionTemplateMapper;
            motionTemplate.SetupWithDescendants();
        };
        
        root.Add(templateField);
        root.Add(button);
        return root;
    }
}