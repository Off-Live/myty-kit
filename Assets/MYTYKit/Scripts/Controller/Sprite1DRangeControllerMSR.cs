using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using MYTYKit.Components;


namespace MYTYKit.Controllers{
    public class Sprite1DRangeControllerMSR : MSRSpriteController, IFloatInput
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
                    if (interval.min <= scaledValue && interval.max > scaledValue)
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

        void IFloatInput.SetInput(float val)
        {
            value = val;
        }
    }
}
