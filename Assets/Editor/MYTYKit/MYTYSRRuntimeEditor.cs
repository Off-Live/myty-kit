using MYTYKit.Components;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MYTYKit
{

    [CustomEditor(typeof(MYTYSpriteResolverRuntime))]
    public class MYTYSRRuntimeEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var options = new DropdownField();
            var currentLabel = new TextField();
            var imageArea = new Image();
            options.label = "Label";

            var sr = (MYTYSpriteResolverRuntime)target;

            options.choices = sr.labels;
            options.RegisterValueChangedCallback(evt => sr.SetLabel(evt.newValue));

            currentLabel.BindProperty(serializedObject.FindProperty("currentLabel"));
            currentLabel.RegisterValueChangedCallback( _ =>
            {
                imageArea.sprite = sr.sprite;
            });
            root.Add(options);
            root.Add(currentLabel);
            root.Add(imageArea);
            return root;

        }
    }
}