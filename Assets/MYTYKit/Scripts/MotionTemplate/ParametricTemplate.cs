
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ParameterItem
{
    public string name;
    public float value; 
}
public class ParametricTemplate : MonoBehaviour, IMotionTemplate
{
    public List<ParameterItem> parameterItems = new();

    public bool Contains(string key)
    {
        return FindIndex(key) >= 0;
    }

    public void SetValue(string key, float value)
    {
        var index = FindIndex(key);
        if (index < 0)
        {
            var item = new ParameterItem()
            {
                name = key,
                value = value
            };
            parameterItems.Add(item);
        }
        else
        {
            parameterItems[index].value = value;
        }
    }

    public float GetValue(string key)
    {
        var index = FindIndex(key);
        if (index < 0)
        {
            return 0.0f;
        }
        else
        {
            return parameterItems[index].value;
        }
    }

    int FindIndex(string key)
    {
        for (int i = 0; i < parameterItems.Count; i++)
        {
            if (parameterItems[i].name == key)
            {
                return i;
            }
        }

        return -1;
    }
}
