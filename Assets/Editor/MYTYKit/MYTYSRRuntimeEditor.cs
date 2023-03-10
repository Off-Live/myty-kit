using MYTYKit.Components;
using UnityEditor;
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
            options.label = "Label";

            var sr = (MYTYSpriteResolverRuntime)target;

            options.choices = sr.labels;
            options.RegisterValueChangedCallback(evt => sr.SetLabel(evt.newValue));

            root.Add(options);
            return root;

        }
    }
}