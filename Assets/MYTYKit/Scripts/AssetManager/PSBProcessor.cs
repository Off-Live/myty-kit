#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MYTYKit
{
    public class PSBPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths, bool didDomainReload)
        {
            var mytyManager = GameObject.FindObjectOfType<MYTYAssetTemplate>();

            if (mytyManager == null) return;

            mytyManager.ApplyAssetImported(importedAssets, movedFromAssetPaths);
        }
    }

    public class PSBModprocessor : AssetModificationProcessor
    {
        static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions option)
        {
            if (!path.EndsWith(".psb")) return AssetDeleteResult.DidNotDelete;
            if (!path.StartsWith(MYTYUtil.AssetPath)) return AssetDeleteResult.DidNotDelete;
            var mytyManager = GameObject.FindObjectOfType<MYTYAssetTemplate>();
            if (mytyManager == null) return AssetDeleteResult.DidNotDelete;

            mytyManager.DeleteAsset(path);
            return AssetDeleteResult.DidNotDelete;
        }
    }
}
#endif
