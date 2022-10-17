
using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.Components
{

    public static class ARFeatureSelector
    {
        public static void ConfigureARFeature(this AvatarSelector selector, string[] featureTrait)
        {
            selector.Configure();
            var index = -1;
            for (int i = 0; i < selector.mytyAssetStorage.traits.Count; i++)
            {
                if (selector.id == selector.mytyAssetStorage.traits[i].id) index = i;
            }

            var traitItem = selector.mytyAssetStorage.traits[index];
            GameObject activeInstance = null;
            foreach (var template in selector.templates)
            {
                if (template.PSBPath == traitItem.filename) activeInstance = template.instance;
            }

            if (activeInstance != null)
            {
                var childCount = activeInstance.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    Process(activeInstance.transform.GetChild(i).gameObject, new List<string>(), featureTrait);
                }
            }
        }

        static void Process(GameObject templateNode, List<string> history, string[] traits)
        {
            int childCount = templateNode.transform.childCount;
            string name = templateNode.name;
            history.Add(name);

            string key = HistoryToKey(history);

            bool active = false;

            for (int i = 0; i < traits.Length; i++)
            {
                string trait = "/" + traits[i];
                if (key == trait || key.StartsWith(trait + "/"))
                {
                    active = true;
                }
            }

            if (!active)
            {
                templateNode.SetActive(active);

                for (int i = 0; i < childCount; i++)
                {
                    Process(templateNode.transform.GetChild(i).gameObject, history, traits);
                }
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