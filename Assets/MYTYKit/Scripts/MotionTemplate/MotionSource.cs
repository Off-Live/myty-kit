
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
public class MTBrigdeItem
{
    public string name;
    public GameObject anchorBrigde;
}
public class MotionSource : MonoBehaviour
{
    [SerializeField] List<MotionCategory> m_motionCategories = new();
    [SerializeField] List<MTBrigdeItem> m_templateBrigdeMap = new();
    
    [SerializeField] MotionTemplateMapper motionTemplateMapper;

    void Start()
    {
        UpdateMotionAndAnchors();
    }

    public List<string> GetCategoryList()
    {
        List<string> ret = new();
        foreach (var category in m_motionCategories)
        {
            ret.Add(category.name);
        }

        return ret;
    }

    public List<RiggingModel> GetRiggingModelsInCategory(string categoryName)
    {
        foreach (var category in m_motionCategories)
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
        for (int i = 0; i < m_motionCategories.Count; i++)
        {
            if (categoryName == m_motionCategories[i].name)
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
            m_motionCategories.Add(newItem);
        }
        else
        {
            var list = m_motionCategories[index1].riggingModels;
            list.Add(model);
        }
    }

    public void Clear()
    {
        m_motionCategories.Clear();
    }

    public void UpdateMotionAndAnchors()
    {
        foreach (var brigdeItem in m_templateBrigdeMap)
        {
            var anchor = motionTemplateMapper.GetAnchor(brigdeItem.name);
            if (anchor == null) return;

            var bridge = brigdeItem.anchorBrigde.GetComponent<IMTBrigde>();
            if (bridge == null) return;
            bridge.SetMotionTemplate(anchor);
        }
    }
}