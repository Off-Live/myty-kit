using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace MYTYKit
{
    public class MYTYUtil
    {
        public static readonly string PrefabPath = "Assets/MYTYAsset/ExportedPrefab";
        public static readonly string BundlePath = "Assets/MYTYAsset/ExportedBundle";
        public static readonly string SelectorPrefab = "AvatarSelector.prefab";
        public static readonly string BoneJson = "bone.json";
        public static readonly string ControllersJson = "controllers.json";
        public static readonly string MotionAdaptersJson = "motions.json";
        public static readonly string RootControllers = "rootCons.json";
        public static readonly string AssetPath = "Assets/MYTYAsset/ImportedAvatarAssets";


        public static void BuildAssetPath(string path)
        {
#if UNITY_EDITOR
            var tokens = path.Split("/", StringSplitOptions.RemoveEmptyEntries);
            var prevPath = "";
            var currPath = tokens[0];
            Debug.Assert(tokens[0] == "Assets");
            var index = 0;
            do
            {
                if (!Directory.Exists(currPath)) break;
                if (index == tokens.Length - 1) return; // the path exists
                prevPath = currPath;
                currPath += "/" + tokens[index + 1];
                index++;
            } while (index < tokens.Length);

            Debug.Assert(index > 0);

            for (int i = index; i < tokens.Length; i++)
            {
                AssetDatabase.CreateFolder(prevPath, tokens[i]);
                prevPath += "/" + tokens[i];
            }
#endif
        }

        public static GameObject GetRoot(GameObject go, System.Type componentType)
        {
            GameObject curr = go;
            while (curr.transform.parent != null)
            {
                if (curr.GetComponent(componentType.Name) != null) return curr;
                curr = curr.transform.parent.gameObject;
            }

            if (curr.GetComponent(componentType.Name) != null) return curr;
            else return null;
        }

        public static string GetGameObjectPath(GameObject ancestor, GameObject decendent, out bool success)
        {
            string path = "";

            while (decendent != ancestor)
            {
                GameObject parent;
                if (decendent.transform.parent == null)
                {
                    success = false;
                    return "";
                }

                parent = decendent.transform.parent.gameObject;
                for (int i = 0; i < parent.transform.childCount; i++)
                {
                    if (parent.transform.GetChild(i).gameObject == decendent)
                    {
                        path = "/" + i + path;
                    }
                }

                decendent = parent;
            }

            success = true;
            return path;
        }

        public static GameObject FindDecendent(GameObject go, string path)
        {
            var tokens = path.Split("/", System.StringSplitOptions.RemoveEmptyEntries);
            GameObject curr = go;

            for (int i = 0; i < tokens.Length; i++)
            {
                var childIndex = int.Parse(tokens[i]);
                curr = curr.transform.GetChild(childIndex).gameObject;
            }

            return curr;
        }

    }

    public static class TransformExtension
    {
        public static List<Transform> GetChildrenList(this Transform tf)
        {
            return Enumerable.Range(0, tf.childCount).Select(idx => tf.GetChild(idx)).ToList();
        }
    }
}
