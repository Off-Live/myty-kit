using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace MYTYKit
{
    
    public class PackageExporter
    {
        const string ScenePath = "Assets/MYTYAsset/ExportedScene/MYTYAvatarScene.unity";
        const string PackagePath = "Assets/MYTYAsset/mytyavatar.unitypackage";
        [MenuItem("MYTY Kit/Export to unitypackage",false,2)]
        static void ExportScene()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            AssetDatabase.CopyAsset(EditorSceneManager.GetActiveScene().path,
                ScenePath);
            AssetDatabase.Refresh();
            AssetDatabase.ExportPackage(new [] {ScenePath},PackagePath , ExportPackageOptions.IncludeDependencies | ExportPackageOptions.Recurse);    
        }
    }

}