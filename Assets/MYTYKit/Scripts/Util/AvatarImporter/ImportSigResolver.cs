    using System;
    using UnityEngine;

    namespace MYTYKit.AvatarImporter
    {
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
                if (importerType == null || !typeof(IMYTYAvatarImporter).IsAssignableFrom(importerType)) return false;

                if (go.GetComponent(importerType) == null) go.AddComponent(importerType);
                return true;
            }
        }
    }
