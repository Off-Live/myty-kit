using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit.Components
{
    [DisallowMultipleComponent]
    public class MuscleSetting : MonoBehaviour
    {
        [Serializable]
        public class MuscleLimit
        {
            public string name;
            public float minScale = 1.0f;
            public float maxScale = 1.0f;
        }
        public List<MuscleLimit> muscleLimits;

        void Start()
        {
            if(muscleLimits==null || muscleLimits.Count==0) muscleLimits = HumanTrait.MuscleName.ToList().Select(name => new MuscleLimit() { name = name }).ToList();
        }

        public void LoadFromJObject(JObject jObj)
        {
            
        }
    }
}