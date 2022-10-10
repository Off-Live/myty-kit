using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using System.Collections.Generic;

[CustomEditor(typeof(Bone1DController))]
public class Bone1DControllerEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        var rootElem = new VisualElement();
        var targetList = new ListView();
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/MYTYKit/UI/Bone2DCon.uss");

        targetList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
        targetList.styleSheets.Add(styleSheet);

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
