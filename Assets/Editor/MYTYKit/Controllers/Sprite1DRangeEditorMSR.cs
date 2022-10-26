using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using MYTYKit.Components;
using MYTYKit.Controllers;
namespace MYTYKit
{
    [CustomEditor(typeof(Sprite1DRangeControllerMSR))]
    public class Sprite1DRangeEditorMSR : UnityEditor.Editor
    {
        [SerializeField] StyleSheet m_styleSheet;
        public override VisualElement CreateInspectorGUI()
        {
            var rootElem = new VisualElement();
            var targetList = new ListView();
            
            targetList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            targetList.styleSheets.Add(m_styleSheet);
        
            targetList.makeItem = () =>
            {
                return new ObjectField();
            };

            targetList.bindItem = (e, i) =>
            {
                (e as ObjectField).value = targetList.itemsSource[i] as GameObject;
                (e as ObjectField).AddToClassList("noEditableObjField");
                (e as ObjectField).AddToClassList("itemSize");

            };

            var listSource = new List<GameObject>();
            var spritesProps = serializedObject.FindProperty("spriteObjects");
            for (int i = 0; i < spritesProps.arraySize; i++)
            {
                if (spritesProps.GetArrayElementAtIndex(i).objectReferenceValue == null)
                {
                    listSource.Add(null);
                }
                else listSource.Add((spritesProps.GetArrayElementAtIndex(i).objectReferenceValue as MYTYSpriteResolver).gameObject);
            }

            targetList.itemsSource = listSource;

            rootElem.Add(new Label("Rigged Sprites : "));
            rootElem.Add(targetList);

            return rootElem;
        }
    }
}
