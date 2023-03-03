using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using MYTYKit.Components;
using System;
using Newtonsoft.Json.Linq;

namespace MYTYKit.Controllers{
    public class Sprite1DRangeControllerMSR : MSRSpriteController, IFloatInput, IComponentWiseInput
    {
        [Serializable]
        public class Interval
        {
            public string label;
            public float min;
            public float max;
        }
        public float min = 0;
        public float max = 1;
        public float value = 0;
        
        public List<Interval> intervals;

        [SerializeField] private string currentLabel;

        string m_lastLabel = "";
        void Update()
        {
            if (spriteObjects == null || intervals == null) return;
            if (max < min) return;
            
            float scaledValue = min + (max - min) * value;
            var selected = "";
            foreach (var interval in intervals)
            {
                if (interval.min <= scaledValue && interval.max >= scaledValue)
                {
                    selected = interval.label;
                    break;
                }
            }

            if (m_lastLabel != selected)
            {
                m_lastLabel = selected;
                UpdateLabel();
            }
        }

        public void UpdateLabel()
        {

            if (spriteObjects == null) return;
            foreach (var spriteResolver in spriteObjects)
            {
                if (spriteResolver == null) continue;
                if (m_lastLabel.Length > 0)
                {
                    spriteResolver.SetCategoryAndLabel(spriteResolver.GetCategory(), m_lastLabel);
                    currentLabel = m_lastLabel;
                }

            }
        }

        public void SetInput(float val)
        {
            value = val;
        }
        public void SetComponent(float value, int componentIdx)
        {
            this.value = value;
        }

        public override JObject SerializeToJObject(Dictionary<Transform, int> tfMap)
        {
            var baseJo = base.SerializeToJObject(tfMap);
            baseJo.Merge(JObject.FromObject(new
            {
                type = GetType().Name,
                min,
                max,
                intervals
            }));
            return baseJo;
        }
    }
}
