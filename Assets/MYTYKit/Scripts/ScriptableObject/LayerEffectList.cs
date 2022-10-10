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


    [CreateAssetMenu(fileName = "LayerEffectList", menuName = "ScriptableObjects/LayerEffectList", order = 1)]
    public class LayerEffectList : ScriptableObject
    {
        public List<LayerEffectEntry> layerEffects;
    }
}
