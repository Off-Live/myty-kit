using System;
using System.IO;
using System.Linq;
using MYTYKit.Components;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MYTYKit
{
    public class MYTY3DAvatarExporter : EditorWindow
    {
        public VisualTreeAsset uiTemplate;
        [MenuItem("MYTY Kit/Export 3D Asset Metadata", false, 41)]
        static void ShowWindow()
        {
            var wnd = CreateInstance<MYTY3DAvatarExporter>();
            wnd.titleContent = new GUIContent("3D Asset Metadata Export");
            wnd.minSize = wnd.maxSize = new Vector2(450, 300);
            wnd.ShowUtility();
        }
        
        [MenuItem("MYTY Kit/Export 3D Asset Metadata", true, 1)]
        static bool ValidateShowGUI()
        {
            var selector = FindObjectOfType<MYTYAvatarDesc>();
            return selector != null;
        }

        void CreateGUI()
        {
            uiTemplate.CloneTree(rootVisualElement);
            var filenameField = rootVisualElement.Q<TextField>();
            var descField = rootVisualElement.Q<ObjectField>();
            descField.objectType = typeof(MYTYAvatarDesc);

            var desc = FindObjectOfType<MYTYAvatarDesc>();

            descField.RegisterValueChangedCallback(evt =>
            {
                var target = evt.newValue as MYTYAvatarDesc;
                if (target.mainBody == null)
                {
                    EditorUtility.DisplayDialog("MYTY Kit", "The Main Body is not assigned.", "Ok");
                    descField.SetValueWithoutNotify(evt.previousValue);
                    return;
                }

                var name = target.mainBody.name;

                Path.GetInvalidFileNameChars().ToList().ForEach(c => name.Replace(c, '_'));
                filenameField.value = name+".json";
            });

            rootVisualElement.Q<Button>().clicked += () =>
            {
                var target = descField.value as MYTYAvatarDesc;
                MYTYUtil.BuildAssetPath(MYTYUtil.MetadataPath);
                File.WriteAllText(MYTYUtil.MetadataPath+"/"+filenameField.value, target.ExportToJson());
                foreach (SceneView sceneView in SceneView.sceneViews)
                {
                    sceneView.ShowNotification(new GUIContent($"Metadata is saved at {MYTYUtil.MetadataPath+"/"+filenameField.value}"));
                }
            };

                
            if(desc!=null) descField.value = desc;

        }
    }
}