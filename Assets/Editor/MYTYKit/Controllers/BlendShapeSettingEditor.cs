using System.Collections.Generic;
using System.Linq;
using MYTYKit.Components;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MYTYKit
{
    [CustomEditor(typeof(BlendShapeSetting))]
    public class BlendShapeSettingEditor : Editor
    {
        List<VisualElement> m_bsUIList;
        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();
            var foldout = new Foldout();
            foldout.value = false;
            foldout.text = "BlendShapes";

            var setting = (BlendShapeSetting)target;
            
            serializedObject.Update();
            if (serializedObject.FindProperty("blendShapes").arraySize == 0)
            {
                setting.Initialize();
                serializedObject.Update();
                
            }

            container.Add(new PropertyField(serializedObject.FindProperty("mesh")));
            
            m_bsUIList = Enumerable.Range(0, setting.blendShapes.Count).Select(idx =>
            {
                var rootElem = new VisualElement();
                rootElem.style.flexDirection = FlexDirection.Row;
                rootElem.style.alignItems = Align.Stretch;
                var label = new Label();
                label.text = setting.blendShapes[idx].blendShape;
                label.style.flexGrow = 1;
                var textField = new TextField();
                textField.value = setting.blendShapes[idx].nameOnAvatar;
                textField.isDelayed = true;
                textField.RegisterValueChangedCallback(evt => TextChanged(evt, idx));
                textField.style.flexGrow = 1;
                rootElem.Add(label);
                rootElem.Add(textField);
                foldout.Add(rootElem);
                return rootElem;
            }).ToList();
            
            container.Add(foldout);

            var autoBtn = new Button(() => TryToSetupBlendShape());
            autoBtn.text = "Try auto mapping";
            
            container.Add(autoBtn);
            
            return container;
        }

        void TextChanged(ChangeEvent<string> evt,int idx)
        {
            UpdateElement(idx, evt.newValue);
        }
        void UpdateElement(int idx, string value)
        {
            serializedObject.FindProperty("blendShapes").GetArrayElementAtIndex(idx)
                .FindPropertyRelative("nameOnAvatar").stringValue = value;
            serializedObject.ApplyModifiedProperties();
        }

        void TryToSetupBlendShape()
        {
            var meshRenderer = ((BlendShapeSetting)target).mesh;
            var bsDict = GetWordsForBSName();
            var bsNames = BlendShapeSetting.GetAllBlendShapeNames();
            var meshBsNames = Enumerable.Range(0, meshRenderer.sharedMesh.blendShapeCount)
                .Select(idx=> meshRenderer.sharedMesh.GetBlendShapeName(idx)).ToList();
            var doneBsNames = new List<string>();
            meshBsNames.ForEach(meshBsName =>
            {
                var lowerName = meshBsName.ToLower().Trim();
                var bsName = bsNames.First(name => lowerName.Contains(name.ToLower()));
                var index = bsNames.IndexOf(bsName);
                if (index >= 0)
                {
                    m_bsUIList[index].Q<TextField>().value = meshBsName;
                    doneBsNames.Add(meshBsName);
                }
            });
            
            meshBsNames.Where(meshBsName=> !doneBsNames.Contains(meshBsName)).ToList().ForEach(meshBsName =>
            {
                var lowerName = meshBsName.ToLower().Trim();
                var matchWords = bsDict.FindAll(words => words.TrueForAll(word => lowerName.Contains(word.ToLower())))
                    .Aggregate((max, next)=> max.Count<next.Count? next : max);
                var bsName = "";
                matchWords.ForEach(word=> bsName+=word);
                var index = bsNames.IndexOf(bsName);
                if (index >= 0) m_bsUIList[index].Q<TextField>().value = meshBsName;
            });
        }

        List<List<string>> GetWordsForBSName()
        {
            var bsNames = BlendShapeSetting.GetAllBlendShapeNames();

            return bsNames.Select(name =>
            {
                var tokenIdx = Enumerable.Range(0, name.Length).Select(idx => (idx, name[idx]))
                    .Where(pair => char.IsUpper(pair.Item2))
                    .Select(pair => pair.idx).ToList();
                tokenIdx.Insert(0, 0);
                tokenIdx.Add(name.Length);
                return Enumerable.Range(0, tokenIdx.Count - 1)
                    .Select(idx => name.Substring(tokenIdx[idx], tokenIdx[idx + 1] - tokenIdx[idx])).ToList();
            }).ToList();
        }
        
    }
}