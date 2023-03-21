using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace MYTYKit
{
    [Serializable]
    public class LayerEffectEntry
    {
        public string name;
        public Material material;
    }

    
    public class LayerEffectList : ScriptableObject
    {
        public List<LayerEffectEntry> layerEffects;
    }
}
