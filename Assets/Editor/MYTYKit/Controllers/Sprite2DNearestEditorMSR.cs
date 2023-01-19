using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using MYTYKit.Components;
using MYTYKit.Controllers;

namespace MYTYKit
{
    [CustomEditor(typeof(Sprite2DNearestControllerMSR))]
    public class Sprite2DNearestEditorMSR : UnityEditor.Editor
    {
        public StyleSheet styleSheet;
        public override VisualElement CreateInspectorGUI()
        {
            var rootElem = new VisualElement();
            var targetList = new ListView();
            targetList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            targetList.styleSheets.Add(styleSheet);

            targetList.makeItem = () =>
            {
                return new ObjectField();
            };

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
