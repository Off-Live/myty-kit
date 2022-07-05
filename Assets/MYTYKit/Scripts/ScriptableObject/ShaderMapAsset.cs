using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ShaderMapEntry
{
    public string name;
    public Material material;
}
[CreateAssetMenu(fileName = "ShaderMap", menuName = "ScriptableObjects/ShaderMapAsset", order = 1)]
public class ShaderMapAsset : ScriptableObject
{
    public List<ShaderMapEntry> shaderMapList;
}
