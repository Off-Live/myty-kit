using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[Serializable]
public class MTItem
{
    public string name;
    public MotionTemplate template;
}
public class MotionTemplateMapper : MonoBehaviour
{
    [SerializeField] List<MTItem> templates;

    public MotionTemplate GetTemplate(string name)
    {
        foreach (var item in templates)
        {
            if (item.name == name)
            {
                return item.template;
            }
        }
        return null;
    }

    public void SetTemplate(string name, MotionTemplate templateObj)
    {
        foreach (var item in templates)
        {
            if (item.name == name)
            {
                item.template = templateObj;
                return;
            }
        }
        
        templates.Add(new MTItem()
        {
            name = name,
            template = templateObj
        });
    }
    
    public List<string> GetNames()
    {
        List<string> ret = new();
        foreach (var item in templates)
        {
            ret.Add(item.name);
        }

        return ret;
    }
    
    public void Clear()
    {
        templates.Clear();
    }

    
}