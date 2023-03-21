using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using MYTYKit.MotionTemplates;
using UnityEngine;

namespace MYTYKit
{
    
    
    [CustomEditor(typeof(MotionTemplateMapper))]
    public class MotionTemplateEditor:Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var templateField = new PropertyField();
            var button = new Button();
            templateField.BindProperty(serializedObject.FindProperty("templates"));
        
            button.text = "Autobuild";
            button.clicked += () =>
            {
                var motionTemplate = target as MotionTemplateMapper;
                motionTemplate.SetupWithDescendants();
            };
        
            root.Add(templateField);
            root.Add(button);
            return root;
        }

        [MenuItem("MYTY Kit/Create Default Motion Template", false, 25)]
        static void CreateMotionTemplate()
        {
            var mtAsset = AssetDatabase.LoadAssetAtPath<MotionTemplateAsset>(MYTYPath.MotionTemplateAssetPath);
            if (mtAsset == null)
                AssetDatabase.CopyAsset(MYTYPath.MotionTemplatePackagePath, MYTYPath.MotionTemplateAssetPath);
            
            mtAsset = AssetDatabase.LoadAssetAtPath<MotionTemplateAsset>(MYTYPath.MotionTemplateAssetPath);
            Debug.Assert(mtAsset!=null);

            var go = Instantiate(mtAsset.motionTemplateObject);
            go.name = "DefaultMotionTemplate";
        }
    }
    
}