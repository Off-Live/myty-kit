
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[Serializable]
public class MotionCategory
{
    public string name;
    public List<RiggingModel> riggingModels;
}

[Serializable]
public class MTBridgeItem
{
    public string name;
    public GameObject templateBridge;
}
public class MotionSource : MonoBehaviour
{
    [SerializeField] List<MotionCategory> motionCategories = new();
    [SerializeField] List<MTBridgeItem> templateBridgeMap = new();
    
    public MotionTemplateMapper motionTemplateMapper;

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

    public List<RiggingModel> GetRiggingModelsInCategory(string categoryName)
    {
        foreach (var category in motionCategories)
        {
            if (category.name == categoryName)
            {
                return category.riggingModels;
            }  
        }

        return null;
    }

    public void AddRiggingModel(string categoryName, RiggingModel model)
    {
        var index1 = -1;
        for (int i = 0; i < motionCategories.Count; i++)
        {
            if (categoryName == motionCategories[i].name)
            {
                index1 = i;
                break;
            }
        }
        
        if (index1 < 0)
        {
            var newItem = new MotionCategory()
            {
                name = categoryName,
                riggingModels = new()
            };
            newItem.riggingModels.Add(model);
            motionCategories.Add(newItem);
        }
        else
        {
            var list = motionCategories[index1].riggingModels;
            list.Add(model);
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
            var anchor = motionTemplateMapper.GetTemplate(brigdeItem.name);
            if (anchor == null) return;

            var bridge = brigdeItem.templateBridge.GetComponent<IMTBridge>();
            if (bridge == null) return;
            bridge.SetMotionTemplate(anchor);
        }
    }
}