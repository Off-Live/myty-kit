using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class TemplateItem
{
    public string name;
    public IMYTYAnchor anchor;
}
public class MotionTemplate : MonoBehaviour
{
    [SerializeField] List<TemplateItem> m_anchorMap;
    
    
}