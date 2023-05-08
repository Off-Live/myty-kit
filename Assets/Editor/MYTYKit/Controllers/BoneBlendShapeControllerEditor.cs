using System.Collections.Generic;
using MYTYKit.Controllers;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MYTYKit
{
    [CustomEditor(typeof(BoneBlendShapeController))]
    public class BoneBlendShapeControllerEditor:Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var rootElem = new VisualElement();
            var targetList = new ListView();
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/MYTYKit/UI/Bone2DCon.uss");
            rootElem.Add(new PropertyField(serializedObject.FindProperty("skip")));
            
            targetList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            targetList.styleSheets.Add(styleSheet);

            targetList.makeItem = () => { return new ObjectField(); };
            targetList.bindItem = (e, i) =>
            {
                (e as ObjectField).value = targetList.itemsSource[i] as GameObject;
                if (targetList.itemsSource[i] == null)
                {
                    (e as ObjectField).label = "Deleted or modified.";
                    (e as ObjectField).AddToClassList("deletedObjField");
                    (e as ObjectField).RemoveFromClassList("noEditableObjField");   
                }
                else
                {
                    (e as ObjectField).label = "";
                    (e as ObjectField).AddToClassList("noEditableObjField");
                    (e as ObjectField).RemoveFromClassList("deletedObjField");
                    (e as ObjectField).AddToClassList("itemSize");      
                }

            };

            var listSource = new List<GameObject>();
            var rigTargetProps = serializedObject.FindProperty("rigTarget");
            for (int i = 0; i < rigTargetProps.arraySize; i++)
            {
                if (rigTargetProps.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    listSource.Add(null);
                }
                else listSource.Add(rigTargetProps.GetArrayElementAtIndex(i).objectReferenceValue as GameObject);
            }

            targetList.itemsSource = listSource;
            
            rootElem.Add(new Label("Rigged Bones : "));
            rootElem.Add(targetList);

            return rootElem;
        }
    }
}