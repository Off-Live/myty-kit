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
            // foreach (var file in files)
            // {
            //     FileInfo fileinfo = new FileInfo(file);
            //     Debug.Log(fileinfo.Name);
            // }
            var kitFullPath = Path.GetFullPath(KitPath);
            if (!Directory.Exists(kitFullPath)) return;
            CopyKitFiles();
            CopyStreamingAssets();
        }

        static void CopyKitFiles()
        {
            var kitAssetPath = "Assets/MYTYKit";
            var kitFullPath = Path.GetFullPath(KitPath);
            //if (Directory.Exists(assetPath)) return;
            FileUtil.CopyFileOrDirectory(kitFullPath,kitAssetPath);
        }

        static void CopyStreamingAssets()
        {
            var saFullPath = Path.GetFullPath(StreamingAssetsPath);
            var files = Directory.GetFiles(saFullPath);
            foreach (var file in files)
            {
                var info = new FileInfo(file);
                var targetPath = Application.streamingAssetsPath + "/" + info.Name;
                if (!File.Exists(targetPath))
                {
                    FileUtil.CopyFileOrDirectory(file, targetPath);
                }
            }

            var mytySaPath = Application.streamingAssetsPath + "/MYTYKit";
            if (!Directory.Exists(mytySaPath))
            {
                FileUtil.CopyFileOrDirectory(StreamingAssetsPath+"/MYTYKit",mytySaPath );
            }
            
        }
    }
}