using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;

[CustomEditor(typeof(MYTYSpriteResolver))]
public class MYTYSpriteResolverEditor : Editor
{
    [SerializeField]
    private StyleSheet uss;
    public override VisualElement CreateInspectorGUI()
    {
        var rootElem = new VisualElement();
        var slaField = new PropertyField();
        var categoryField = new DropdownField();
        var labelDropDownField = new DropdownField();
        var imageArea = new Image();
        rootElem.styleSheets.Add(uss);
        slaField.BindProperty(serializedObject.FindProperty("m_spriteLibraryAsset"));
        categoryField.label = "Category";
        labelDropDownField.label = "Label";
        var resolver = target as MYTYSpriteResolver;

        slaField.RegisterValueChangeCallback((SerializedPropertyChangeEvent e) =>
        {
            categoryField.choices.Clear();
            labelDropDownField.choices.Clear();
            imageArea.sprite = null;

            RefreshCategory(categoryField);
            RefreshLabel(labelDropDownField);
            imageArea.sprite = resolver.spriteLibraryAsset.GetSprite(resolver.GetCategory(), resolver.GetLabel());
            resolver.SetCategoryAndLabel(resolver.GetCategory(), resolver.GetLabel());
        });

        categoryField.RegisterValueChangedCallback((ChangeEvent<string> e) =>
        {
            Debug.Log("test 1");
            labelDropDownField.choices.Clear();
            imageArea.sprite = null;
            RefreshLabel(labelDropDownField);
            resolver.SetCategoryAndLabel(e.newValue, "");
        });

        labelDropDownField.RegisterValueChangedCallback((ChangeEvent<string> e) =>
        {
            Debug.Log("changed " + e.newValue);
            imageArea.sprite = resolver.spriteLibraryAsset.GetSprite(resolver.GetCategory(), e.newValue);
            resolver.SetCategoryAndLabel(resolver.GetCategory(), e.newValue);
        });


        RefreshCategory(categoryField);

        categoryField.value = resolver.GetCategory();
        categoryField.AddToClassList("sprite_resolver_drop_down");

        RefreshLabel(labelDropDownField);

        labelDropDownField.value = resolver.GetLabel();
        labelDropDownField.AddToClassList("sprite_resolver_drop_down");
        imageArea.sprite = resolver.spriteLibraryAsset.GetSprite(resolver.GetCategory(), resolver.GetLabel());
        imageArea.AddToClassList("imageArea");

        rootElem.Add(slaField);
        rootElem.Add(categoryField);
        rootElem.Add(labelDropDownField);
        rootElem.Add(imageArea);

        return  rootElem;
    }

    void RefreshCategory(DropdownField categoryField)
    {
        var resolver = target as MYTYSpriteResolver;
        var categories = resolver.spriteLibraryAsset.GetCategoryNames();

        foreach (var cat in categories)
        {
            categoryField.choices.Add(cat);
        }
        
    }

    void RefreshLabel(DropdownField labelDropDownField)
    {
        var resolver = target as MYTYSpriteResolver;
        var labels = resolver.spriteLibraryAsset.GetCategoryLabelNames(resolver.GetCategory());
        foreach (var label in labels)
        {
            labelDropDownField.choices.Add(label);
        }

    }



}

