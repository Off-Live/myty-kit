using System.IO;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MYTYKit
{
    public class About : EditorWindow
    {
        public VisualTreeAsset UITemplate;

        [MenuItem("MYTY Kit/About", false, 100)]
        public static void ShowWindow()
        {
            var wnd = GetWindow<About>();
            wnd.titleContent = new GUIContent("About");
        }

        void CreateGUI()
        {
            UITemplate.CloneTree(rootVisualElement);
            maxSize = new Vector2(300, 120);
            minSize = maxSize;
            var versionField = rootVisualElement.Q<Label>("LBLVersion");
            versionField.text = GetVersion();

        }
        
        public static string GetVersion()
        {
            var packageJson = Path.GetFullPath("Packages/com.offlive.myty.myty-kit/package.json");
            if (!File.Exists(packageJson)) packageJson = Path.GetFullPath("Assets/package.json");
            var jsonText= File.ReadAllText(packageJson);
            var json = JObject.Parse(jsonText);
            return (string)json["version"];
            
        }

        public static string GetProductFileName()
        {
            return $"{Application.productName}_Kit_v{GetVersion()}";
        }
    }
}
