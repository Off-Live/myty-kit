using UnityEditor;
using UnityEngine.Windows;

namespace MYTYKit
{
    public class MiscMenu
    {
        [MenuItem("MYTY Kit/Update Kit", false, 40)]
        public static void UpdateKit()
        {
            var path = EditorUtility.OpenFilePanel("Choose package", "", "unitypackage");

            if (path == "")
            {
                return;
            }
            Directory.Delete("Assets/Editor/MYTYKit");
            Directory.Delete("Assets/MYTYKit");
            AssetDatabase.Refresh();
            AssetDatabase.ImportPackage(path, true);
        }
    }
}