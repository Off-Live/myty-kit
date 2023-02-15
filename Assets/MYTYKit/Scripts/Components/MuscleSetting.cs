using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MYTYKit.Components
{
    
    public class MuscleSetting : MonoBehaviour
    {
        [Serializable]
        public class MuscleLimit
        {
            public string name;
            public float min = -1.0f;
            public float max = 1.0f;
        }
        public List<MuscleLimit> muscleLimits;

        void Start()
        {
            muscleLimits = HumanTrait.MuscleName.ToList().Select(name => new MuscleLimit() { name = name }).ToList();
           
        }
        //
        // public float GetMuscleMin(int idx)
        // {
        //     return muscleLimits[idx].min;
        // }
        //
        // public float GetMuscleMax(int idx)
        // {
        //     return muscleLimits[idx].max;
        // }
        //
        // public void SetMuscleMin(int idx, float min)
        // {
        //     muscleLimits[idx].min = min;
        // }
        //
        // public void SetMuscleMax(int idx, float max)
        // {
        //     muscleLimits[idx].min = max;
        // }
        //
        //
        // public void LoadMuscleSetting(string filename)
        // {
        //     
        // }
        //
        
    }
}