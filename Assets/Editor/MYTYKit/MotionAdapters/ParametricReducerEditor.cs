using MYTYKit.MotionAdapters;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MYTYKit
{
    [CustomEditor(typeof(ParametricReducer))]
    public class ParametricReducerEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var rootElem = new VisualElement();
            var foldOut = new Foldout();
            var templateProp = new PropertyField();
            var confProp = new PropertyField();

            templateProp.BindProperty(serializedObject.FindProperty("template"));
            confProp.BindProperty(serializedObject.FindProperty("configuration"));
            
            rootElem.Add(templateProp);
            rootElem.Add(confProp);
            
            foldOut.text = "Advanced Options";
            foldOut.value = false;
            
            AdvancedOptionHelper.BuildAdvancedOption(foldOut,serializedObject);
            
            rootElem.Add(foldOut);
            return rootElem;
        } 
    }
}