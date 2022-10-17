
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionTemplates
{
    [Serializable]
    public class MotionCategory
    {
        public string name;
        public List<MotionTemplateBridge> bridges;
    }

    [Serializable]
    public class MTBridgeItem
    {
        public string name;
        public MotionTemplateBridge templateBridge;
    }

    public class MotionSource : MonoBehaviour
    {
        [SerializeField] List<MotionCategory> motionCategories = new();
        [SerializeField] List<MTBridgeItem> templateBridgeMap = new();

        public List<MotionTemplateMapper> motionTemplateMapperList = new();

        void Start()
        {
            UpdateMotionAndTemplates();
        }

        public List<string> GetCategoryList()
        {
            List<string> ret = new();
            foreach (var category in motionCategories)
            {
                ret.Add(category.name);
            }

            return ret;
        }

        public List<MotionTemplateBridge> GetBridgesInCategory(string categoryName)
        {
            foreach (var category in motionCategories)
            {
                if (category.name == categoryName)
                {
                    return category.bridges;
                }
            }

            return null;
        }

        public void AddMotionTemplateBridge(string categoryName, MotionTemplateBridge bridge)
        {
            var index = -1;
            for (int i = 0; i < motionCategories.Count; i++)
            {
                if (categoryName == motionCategories[i].name)
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                var newItem = new MotionCategory()
                {
                    name = categoryName,
                    bridges = new()
                };
                newItem.bridges.Add(bridge);
                motionCategories.Add(newItem);
            }
            else
            {
                var list = motionCategories[index].bridges;
                list.Add(bridge);
            }
        }

        public void Clear()
        {
            motionCategories.Clear();
        }

        public void UpdateMotionAndTemplates()
        {
            foreach (var brigdeItem in templateBridgeMap)
            {
                brigdeItem.templateBridge.ClearMotionTemplate();
            }

            foreach (var brigdeItem in templateBridgeMap)
            {
                foreach (var motionTemplateMapper in motionTemplateMapperList)
                {
                    var template = motionTemplateMapper.GetTemplate(brigdeItem.name);
                    if (template == null) continue;

                    var bridge = brigdeItem.templateBridge;
                    if (bridge == null) continue;

                    bridge.AddMotionTemplate(template);
                }

            }
        }
    }
}