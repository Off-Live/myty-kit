
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(MotionSource))]
public class MotionSourceEditor:Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        var categoryField = new PropertyField();
        var templateBridgeField = new PropertyField();
        var motionTemplateField = new PropertyField();
        var categoryButton = new Button();
        var bridgeButton = new Button();
        
        categoryField.BindProperty(serializedObject.FindProperty("motionCategories"));
        templateBridgeField.BindProperty(serializedObject.FindProperty("templateBridgeMap"));
        motionTemplateField.BindProperty(serializedObject.FindProperty("motionTemplateMapper"));
        categoryButton.text = "Autobuild Category";
        categoryButton.clicked += () =>
        {
            var motionSource = target as MotionSource;
            motionSource.SetupMotionTemplate();
        };

        bridgeButton.text = "Autofill bridge name";
        bridgeButton.clicked += () =>
        {
            var motionSource = target as MotionSource;
            motionSource.SetupBridge();
        };
        
        root.Add(categoryField);
        root.Add(templateBridgeField);
        root.Add(motionTemplateField);
        root.Add(categoryButton);
        root.Add(bridgeButton);
        return root;
    }
}
