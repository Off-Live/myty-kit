using System.Reflection;
using MYTYKit.MotionAdapters;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MYTYKit
{
    public class AdvancedOptionHelper
    {
        public static void BuildAdvancedOption(VisualElement parent, SerializedObject so)
        {
            var adapterType = typeof(DampingAndStabilizingVec3Adapter);
            var fields = adapterType.GetFields();
            foreach (var field in fields)
            {
                var prop = new PropertyField();
                prop.BindProperty(so.FindProperty(field.Name));
                parent.Add(prop);
            }
        }
    }
}