using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


[Serializable]
public class MTItem
{
    public string name;
    public GameObject anchor;
}
public class MotionTemplateMapper : MonoBehaviour
{
    [SerializeField] List<MTItem> m_templates;

    public IMotionTemplate GetAnchor(string name)
    {
        foreach (var item in m_templates)
        {
            if (item.name == name)
            {
                return item.anchor.GetComponent<IMotionTemplate>();
            }
        }
        return null;
    }
    
}