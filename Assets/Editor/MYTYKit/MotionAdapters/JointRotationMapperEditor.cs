using MYTYKit.MotionAdapters;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MYTYKit
{
    [CustomEditor(typeof(JointRotationMapper))]
    public class JointRotationMapperEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var rootElem = new VisualElement();
            var foldOut = new Foldout();
            var jointProp = new PropertyField();
            var fromProp = new PropertyField();
            var confProp = new PropertyField();

            jointProp.BindProperty(serializedObject.FindProperty("joint"));
            fromProp.BindProperty(serializedObject.FindProperty("from"));
            confProp.BindProperty(serializedObject.FindProperty("configuration"));
            
            rootElem.Add(jointProp);
            rootElem.Add(fromProp);
            rootElem.Add(confProp);
            
            foldOut.text = "Advanced Options";
            foldOut.value = false;
            
            AdvancedOptionHelper.BuildAdvancedOption(foldOut,serializedObject);
            
            rootElem.Add(foldOut);
            return rootElem;
        }
        
        
        
        
    }
}