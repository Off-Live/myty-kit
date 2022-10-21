using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEditor;

namespace MYTYKit.Controllers
{
    [Serializable]
    public class Interval
    {
        public string label;
        public float min;
        public float max;
    }

    public class Sprite1DRangeController : SpriteController, IFloatInput,IComponentWiseInput
    {
        public float min = 0;
        public float max = 1;
        public float value = 0;

        public List<Interval> intervals;

        [SerializeField] private string currentLabel;


        void Update()
        {
            if (spriteObjects == null || intervals == null) return;
            UpdateLabel();

        }

        public void UpdateLabel()
        {

            if (max < min) return;
            float scaledValue = min + (max - min) * value;
            if (spriteObjects == null) return;
            foreach (var spriteResolver in spriteObjects)
            {
                if (spriteResolver == null) return;
                var selected = "";
                foreach (var interval in intervals)
                {
                    if (interval.min <= scaledValue && interval.max >= scaledValue)
                    {
                        selected = interval.label;
                        break;
                    }
                }

                if (selected.Length > 0)
                {
                    spriteResolver.SetCategoryAndLabel(spriteResolver.GetCategory(), selected);
                    currentLabel = selected;
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
    }
}