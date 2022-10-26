using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using System.Collections.Generic;
using MYTYKit.Controllers;

namespace MYTYKit
{
    [CustomEditor(typeof(Bone2DController))]
    public class Bone2DControllerEditor : UnityEditor.Editor
    {
        [SerializeField] StyleSheet m_styleSheet;
        public override VisualElement CreateInspectorGUI()
        {
            var rootElem = new VisualElement();
            var targetList = new ListView();
            
            targetList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            targetList.styleSheets.Add(m_styleSheet);

            targetList.makeItem = () => { return new ObjectField(); };

            targetList.bindItem = (e, i) =>
            {
                (e as ObjectField).value = targetList.itemsSource[i] as GameObject;
                (e as ObjectField).AddToClassList("noEditableObjField");
                (e as ObjectField).AddToClassList("itemSize");

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

            var flipXBtn = new Button();
            var flipYBtn = new Button();

            flipXBtn.text = "Filp X pivots";
            flipYBtn.text = "Flip Y pivots";

            flipXBtn.clicked += () =>
            {
                var con = target as Bone2DController;
                con.FlipX();
            };
            flipYBtn.clicked += () =>
            {
                var con = target as Bone2DController;
                con.FlipY();
            };
            
            rootElem.Add(new Label("Rigged Bones : "));
            rootElem.Add(targetList);
            rootElem.Add(flipXBtn);
            rootElem.Add(flipYBtn);

            return rootElem;
        }
    }
}
