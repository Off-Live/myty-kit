using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

using MYTYKit.Components;
namespace MYTYKit
{
    [CustomEditor(typeof(AvatarSelector))]
    public class AvatarSelectorEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var assetField = new ObjectField();
            var idField = new TextField();
            var configureBtn = new Button();
            var templateField = new PropertyField();
            var searchObj = new ObjectField();
            var findBtn = new Button();

            idField.label = "Avatar ID";
            idField.BindProperty(serializedObject.FindProperty("id"));
            idField.isDelayed = true;

            assetField.label = "MYTY Asset";
            assetField.BindProperty(serializedObject.FindProperty("m_mytyAssetStorage"));
            assetField.objectType = typeof(MYTYAssetScriptableObject);

            configureBtn.text = "Configure";
            configureBtn.clicked += () => { (target as AvatarSelector).Configure(); };

            searchObj.label = "Find with";
            searchObj.objectType = typeof(GameObject);

            findBtn.text = "Find next";
            findBtn.clicked += () => { (target as AvatarSelector).FindWithTraitObj(searchObj.value as GameObject); };

            templateField.BindProperty(serializedObject.FindProperty("templates"));
            root.Add(templateField);
            root.Add(assetField);
            root.Add(idField);
            root.Add(configureBtn);
            root.Add(searchObj);
            root.Add(findBtn);
            return root;
        }

    }
}