
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ParameterItem
{
    public string name;
    public float value; 
}
public class ParameterAnchor : MonoBehaviour, IMYTYAnchor
{
    public List<ParameterItem> parameterItems;
}
