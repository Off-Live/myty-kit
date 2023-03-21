using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MYTYKit
{
    [Serializable]
    public class ShaderMapEntry
    {
        public string name;
        public Material material;
    }
    
    public class ShaderMapAsset : ScriptableObject
    {
        public List<ShaderMapEntry> shaderMapList;
    }
}
