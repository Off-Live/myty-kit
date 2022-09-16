    using System;
    using UnityEditor;
    using UnityEngine;

    public class ImportSigResolver
    {
        public static bool DetectAndAttachImporter(GameObject go, AssetBundle bundle)
        {
            var textAsset = bundle.LoadAsset<TextAsset>("AvatarImportSig.txt");
            if (textAsset == null)
            {
                if (go.GetComponent<MYTYAvatarImporter>() == null) go.AddComponent<MYTYAvatarImporter>();
                return true;
            }

            var importerType = Type.GetType(textAsset.text);
            if (importerType == null || !importerType.IsSubclassOf(typeof(IMYTYAvatarImporter))) return false;

            if (go.GetComponent(importerType) == null) go.AddComponent(importerType);
            return true;
        }
    }
