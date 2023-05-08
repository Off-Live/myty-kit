using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;


namespace MYTYKit
{
    public class SpriteLibraryFactoryRuntime
    {
        public static SpriteLibraryAsset CreateLibraryRuntime(GameObject psb)
        {
            var spriteLibrary = ScriptableObject.CreateInstance<SpriteLibraryAsset>();
            
            for (int i = 0; i < psb.transform.childCount; i++)
            {
                if (psb.transform.GetChild(i).name.ToUpper().StartsWith("BONE") &&
                    i == psb.transform.childCount - 1) continue;
                Traverse(spriteLibrary, psb.transform.GetChild(i).gameObject, new List<string>());
            }

            return spriteLibrary;
        }

        static void Traverse(SpriteLibraryAsset spriteLibrary, GameObject templateNode, List<string> history)
        {
            int childCount = templateNode.transform.childCount;

            string name = templateNode.name;
            int sufIdx = name.LastIndexOf("_");

            if (sufIdx >= 0)
            {
                string surfix = name.Substring(sufIdx + 1);
                if (int.TryParse(surfix, out _))
                {
                    name = name.Substring(0, sufIdx);
                }
            }

            templateNode.name = name;


            if (childCount > 0)
            {
                history.Add(name);
                for (int i = 0; i < childCount; i++)
                {
                    Traverse(spriteLibrary, templateNode.transform.GetChild(i).gameObject, history);
                }

                history.RemoveAt(history.Count - 1);
            }
            else
            {
                var renderer = templateNode.GetComponent<SpriteRenderer>();

                if (renderer != null)
                {
                    string category = "";
                    for (int i = 0; i < history.Count - 1; i++)
                    {
                        category += history[i] + "/";
                    }

                    if (history.Count > 0) category += history[history.Count - 1];
                    else category = "/";


                    if (renderer.sprite != null)
                    {
                        spriteLibrary.AddCategoryLabel(renderer.sprite, category, renderer.sprite.name);
                    }
                }
            }


        }
    }
}
