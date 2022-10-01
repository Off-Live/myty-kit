using System.Collections;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using FaceBlendShape = MeFaMoConfig.FaceBlendShape;

[CustomEditor(typeof(MeFaMoSolver))]
public class BlendShapeEditor : Editor
{
    Dictionary<FaceBlendShape, Slider> m_sliderMap = new();
    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        var meshProp = new PropertyField();
        var normProp = new PropertyField();
        meshProp.BindProperty(serializedObject.FindProperty("faceMesh"));
        normProp.BindProperty(serializedObject.FindProperty("isNormalize"));
        root.Add(meshProp);
        root.Add(normProp);
        var solver = target as MeFaMoSolver;

        foreach (var pair  in solver.blendShape)
        {
            var slider = new Slider();
            slider.lowValue = 0.0f;
            slider.highValue = 1.0f;
            slider.value = pair.Value;
            slider.label = pair.Key + "";
            m_sliderMap[pair.Key] = slider;
            root.Add(slider);
        }
        
        solver.SetInspectorCallback(UpdateUI);
        return root;
    }

    public void UpdateUI()
    {
        var solver = target as MeFaMoSolver;

        foreach (var pair  in solver.blendShape)
        {
            var slider = m_sliderMap[pair.Key];
            slider.value = pair.Value;
        }
    }

    
}
