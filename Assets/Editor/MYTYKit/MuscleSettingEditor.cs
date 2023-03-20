using System;
using System.Collections.Generic;
using System.Linq;
using MYTYKit.Components;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MYTYKit
{
    [CustomEditor(typeof(MuscleSetting))]
    public class MuscleSettingEditor:Editor
    {
        List<MinMaxSlider> m_sliders = new();
        public override VisualElement CreateInspectorGUI()
        {
            serializedObject.Update();
            var setting = (MuscleSetting)target;
            if (serializedObject.FindProperty("muscleLimits").arraySize == 0)
            {
                setting.muscleLimits = HumanTrait.MuscleName.ToList().Select(name => new MuscleSetting.MuscleLimit() { name = name }).ToList();
                serializedObject.Update();
            }
            m_sliders.Clear();
            var container = new VisualElement();
            var foldout = new Foldout();
            foldout.value = false;
            foldout.text = "Muscles";
            
            Enumerable.Range(0, setting.muscleLimits.Count).Select(idx =>
            {
                var rootElem = new Foldout();
                rootElem.text = $"{setting.muscleLimits[idx].name}({idx})";
                rootElem.value = false;

                var minField = new FloatField();
                var maxField = new FloatField();

                var minValue = HumanTrait.GetMuscleDefaultMin(idx) * setting.muscleLimits[idx].minScale;
                var maxValue = HumanTrait.GetMuscleDefaultMax(idx) * setting.muscleLimits[idx].maxScale;
                
                var sliderElem = new VisualElement();
                sliderElem.style.flexDirection = FlexDirection.Row;
                sliderElem.style.alignContent = Align.Stretch;
                
                var slider = new MinMaxSlider(minValue,maxValue, -180, 180);
                slider.style.flexGrow = 1;
                slider.RegisterValueChangedCallback(evt => SliderValueChanged(evt, idx,slider, minField, maxField));
                m_sliders.Add(slider);
                minField.SetValueWithoutNotify(minValue);
                minField.style.width = 50;
                minField.isDelayed = true;
                maxField.SetValueWithoutNotify(maxValue);
                maxField.style.width = 50;
                maxField.isDelayed = true;
                minField.RegisterValueChangedCallback(evt => MinValueChanged(evt, slider, minField));
                maxField.RegisterValueChangedCallback(evt => MaxValueChanged(evt, slider, maxField));
                
                var btn = new Button(() => ResetMuscle(idx, slider));
                btn.text = "Reset";
                sliderElem.Add(minField);
                sliderElem.Add(slider);
                sliderElem.Add(maxField);
                sliderElem.Add(btn);
                
                rootElem.Add(sliderElem);
                return rootElem;
            }).ToList().ForEach( visualElem => foldout.Add(visualElem));

            var resetBtn = new Button(() => ResetAll());
            resetBtn.text = "Reset";
            
            container.Add(foldout);
            container.Add(resetBtn);
            return container;
        }

        void ResetMuscle(int idx, MinMaxSlider slider)
        {
            slider.value = new Vector2(HumanTrait.GetMuscleDefaultMin(idx), HumanTrait.GetMuscleDefaultMax(idx));
        }

        void MinValueChanged(ChangeEvent<float> evt, MinMaxSlider slider, FloatField field)
        {
            var newValue = evt.newValue;
            if(newValue>0) newValue = 0.0f;
            field.SetValueWithoutNotify(newValue);
            var sliderValue = slider.value;
            slider.value = (new Vector2(newValue, sliderValue.y));
        }
        void MaxValueChanged(ChangeEvent<float> evt, MinMaxSlider slider, FloatField field)
        {
            var newValue = evt.newValue;
            if(newValue<0) newValue = 0.0f;
            field.SetValueWithoutNotify(newValue);
            var sliderValue = slider.value;
            slider.value = (new Vector2(sliderValue.x, newValue));
        }

        void SliderValueChanged(ChangeEvent<Vector2> evt, int idx, MinMaxSlider slider, FloatField minField, FloatField maxField)
        {
            var newValue = evt.newValue;
            var min = newValue.x > 0.0f ? 0.0f : newValue.x;
            var max = newValue.y < 0.0f ? 0.0f : newValue.y;
            slider.SetValueWithoutNotify(new Vector2(min, max));
            minField.SetValueWithoutNotify(min);
            maxField.SetValueWithoutNotify(max);
            UpdateMuscle(idx, min, max);
        }

        void UpdateMuscle(int idx, float min, float max)
        {
            var muscleLimitProp = serializedObject.FindProperty("muscleLimits").GetArrayElementAtIndex(idx);
            muscleLimitProp.FindPropertyRelative("minScale").floatValue = min / HumanTrait.GetMuscleDefaultMin(idx);
            muscleLimitProp.FindPropertyRelative("maxScale").floatValue = max / HumanTrait.GetMuscleDefaultMax(idx);
            serializedObject.ApplyModifiedProperties();
        }

        void ResetAll()
        {
            Enumerable.Range(0,serializedObject.FindProperty("muscleLimits").arraySize).ToList().ForEach(idx =>  
                m_sliders[idx].value = new Vector2(HumanTrait.GetMuscleDefaultMin(idx), HumanTrait.GetMuscleDefaultMax(idx))
            );
        }
    }
}