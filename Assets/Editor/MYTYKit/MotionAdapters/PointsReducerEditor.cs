using MYTYKit.MotionAdapters;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MYTYKit
{
    [CustomEditor(typeof(PointsReducer))]
    public class PointsReducerEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var rootElem = new VisualElement();
            var foldOut = new Foldout();
            var templateProp = new PropertyField();
            var indicesProp = new PropertyField();
            var reducerProp = new PropertyField();
            var confProp = new PropertyField();

            templateProp.BindProperty(serializedObject.FindProperty("template"));
            indicesProp.BindProperty(serializedObject.FindProperty("indices"));
            reducerProp.BindProperty(serializedObject.FindProperty("reducer"));
            confProp.BindProperty(serializedObject.FindProperty("configuration"));
            
            rootElem.Add(templateProp);
            rootElem.Add(indicesProp);
            rootElem.Add(reducerProp);
            rootElem.Add(confProp);
            
            foldOut.text = "Advanced Options";
            foldOut.value = false;
            
            AdvancedOptionHelper.BuildAdvancedOption(foldOut,serializedObject);
            
            rootElem.Add(foldOut);
            return rootElem;
        } 
    }
}