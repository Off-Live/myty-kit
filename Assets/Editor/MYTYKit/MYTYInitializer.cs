using System.IO;
using MYTYKit.Components;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MYTYKit
{
    [InitializeOnLoad]
    public class MYTYInitializer
    {
        
        const string KitPath = "Packages/com.offlive.myty.myty-kit/MYTYKit";
        const string StreamingAssetsPath = "Packages/com.offlive.myty.myty-kit/StreamingAssets";
        static MYTYInitializer()
        {
            Debug.Log("MYTYKit Start Up");
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += SceneOpenedCallback;
            UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode += SceneLoadedCallback;
            var kitFullPath = Path.GetFullPath(KitPath);
            if (!Directory.Exists(kitFullPath)) return;
            CopyKitFiles();
            //CopyStreamingAssets();
            
        }

        static void SceneOpenedCallback(
            Scene _scene,
            UnityEditor.SceneManagement.OpenSceneMode _mode)
        {
            BoneControllerStorage.Save();
            Debug.Log("Rig storage save called for scene opened event");
        }

        static void SceneLoadedCallback(Scene scene, Scene mode)
        {
            BoneControllerStorage.Save();
            Debug.Log("Rig storage save called for scene loaded event");
        }
        static void CopyKitFiles()
        {
            var kitAssetPath = "Assets/MYTYKit";
            var kitFullPath = Path.GetFullPath(KitPath);
            var subAssetDir = new string[]
            {
                "CmdTools","LayerEffect","MotionAdapter","MotionTemplate","UI"
            };

            foreach (var dir in subAssetDir)
            {
                RecursiveCopy(kitFullPath+"/"+dir, kitAssetPath+"/"+dir);
            }
        }

        static void CopyStreamingAssets()
        {
            var saFullPath = Path.GetFullPath(StreamingAssetsPath);
            RecursiveCopy(saFullPath, Application.streamingAssetsPath);
        }

        static void RecursiveCopy(string fromAbsolutePath, string toPath)
        {
            var files = Directory.GetFiles(fromAbsolutePath);
            Directory.CreateDirectory(toPath);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var toFile = toPath + "/" + fileInfo.Name;
                if (fileInfo.Extension == ".meta") continue;
                if (File.Exists(toFile)) continue;
                FileUtil.CopyFileOrDirectory(file, toFile);
            }
            
            var subdirs = Directory.GetDirectories(fromAbsolutePath);
            foreach (var subdir in subdirs)
            {
                var dirInfo = new DirectoryInfo(subdir);
                RecursiveCopy(subdir,toPath+"/"+dirInfo.Name);
            }
        }
    }
}