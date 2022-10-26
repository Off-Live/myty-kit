using System.IO;
using PlasticGui.WorkspaceWindow.Items;
using UnityEditor;
using UnityEngine;

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

            // var testPath = Path.GetFullPath("Packages/com.unity.2d.common");
            // var files = Directory.GetFiles(testPath);
            // // foreach (var dir in files)
            // // {
            // //     var info = new DirectoryInfo(dir);
            // //     Debug.Log(dir);
            // //     Debug.Log(info.Name);
            // // }
            // foreach (var file in files)
            // {
            //     FileInfo fileinfo = new FileInfo(file);
            //     Debug.Log(fileinfo.Name + "  "+fileinfo.Extension);
            // }
            // var kitFullPath = Path.GetFullPath(KitPath);
            // if (!Directory.Exists(kitFullPath)) return;
            // CopyKitFiles();
            // CopyStreamingAssets();
        }

        static void CopyKitFiles()
        {
            var kitAssetPath = "Assets/MYTYKit";
            var kitFullPath = Path.GetFullPath(KitPath);
            var subAssetDir = new string[]
            {
                "CmdTools","LayerEffect","MotionAdapter","MotionTemplate"
            };

            foreach (var dir in subAssetDir)
            {
                RecursiveCopy(kitFullPath+"/"+dir, kitAssetPath+"/"+dir);
            }
        }

        static void CopyStreamingAssets()
        {
            var saFullPath = Path.GetFullPath(StreamingAssetsPath);
            RecursiveCopy(saFullPath, StreamingAssetsPath);
        }

        static void RecursiveCopy(string fromAbsolutePath, string toPath)
        {
            var files = Directory.GetFiles(fromAbsolutePath);
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