
using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.Components
{

    public static class ARFeatureSelector
    {
        public static int GetCurrentTemplateIndex(this AvatarSelector selector)
        {
            var index = -1;
            for (int i = 0; i < selector.mytyAssetStorage.traits.Count; i++)
            {
                var searchId = "";
                if (selector.mytyAssetStorage.traits[i].tokenId=="")
                {
                    searchId = "" + selector.mytyAssetStorage.traits[i].id;
                }
                else
                {
                    searchId = selector.mytyAssetStorage.traits[i].tokenId;
                }
                if (selector.id == searchId) index = i;
            }
        
            var traitItem = selector.mytyAssetStorage.traits[index];

            int templateIndex = -1;
            for (var i=0;i<selector.templates.Count;i++)
            {
                if (selector.templates[i].PSBPath == traitItem.filename) templateIndex = i;
            }

            return templateIndex;
        }
        public static void ConfigureARFeature(this AvatarSelector selector, ARFaceItem[] faceItems)
        {
            
            selector.Configure(); 
            
            GameObject activeInstance = null;
            var templateIndex = GetCurrentTemplateIndex(selector);
            if (templateIndex < 0) return;

            activeInstance = selector.templates[templateIndex].instance;
            var childCount = activeInstance.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Process(activeInstance.transform.GetChild(i).gameObject, new List<string>(), faceItems[templateIndex].traits.ToArray());
            }
        }

        static void Process(GameObject templateNode, List<string> history, string[] traits)
        {
            int childCount = templateNode.transform.childCount;
            string name = templateNode.name;
            history.Add(name);

            string key = HistoryToKey(history);

            if (childCount == 0)
            {
                bool active = false;

                for (int i = 0; i < traits.Length; i++)
                {
                    string trait = "/" + traits[i];
                    if (key == trait || key.StartsWith(trait + "/"))
                    {
                        active = true;
                    }
                }

                if (!active) templateNode.SetActive(false);
                
            }

            for (int i = 0; i < childCount; i++)
            {
                Process(templateNode.transform.GetChild(i).gameObject, history, traits);
            }
            

            history.RemoveAt(history.Count - 1);
        }

        static string HistoryToKey(List<string> history)
        {
            string ret = "";
            foreach (var elem in history)
            {
                ret += "/" + elem;
            }

            return ret;
        }
    }
}