using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MYTYKit
{
    
    public class PackageExporter
    {
        public const string ScenePath = "Assets/MYTYAsset/ExportedScene/MYTYAvatarScene.unity";
        public const string PackagePath = "Assets/MYTYAsset/";
        [MenuItem("MYTY Kit/Export to unitypackage",false,2)]
        static void ExportScene()
        {
            var currentScenePath = EditorSceneManager.GetActiveScene().path;
            
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            AssetDatabase.CopyAsset(currentScenePath, ScenePath);
            AssetDatabase.Refresh();
            var exportedScene = EditorSceneManager.OpenScene(ScenePath);
            var mediapipeExtension = GameObject.Find("MediapipeMotionPack");
            if(mediapipeExtension!=null) Object.DestroyImmediate(mediapipeExtension);
            EditorSceneManager.SaveScene(exportedScene);
            AssetDatabase.ExportPackage(new [] {ScenePath, MYTYPath.AssetPath},PackagePath+About.GetProductFileName()+".unitypackage" , ExportPackageOptions.Recurse);
            EditorSceneManager.OpenScene(currentScenePath);    
        }
    }

}