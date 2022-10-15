using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MYTYKit.Components;

namespace MYTYKit.Controllers
{
    public class Sprite2DNearstControllerMSR : MSRSpriteController, IVec2Input
    {
        public Vector2 bottomLeft = new Vector2(0, 0);
        public Vector2 topRight = new Vector2(1, 1);
        public Vector2 value = new Vector2(0, 0);

        public List<Label2D> labels;

        [SerializeField] private string currentLabel;

        void Update()
        {
            if (spriteObjects == null || labels == null) return;
            UpdateLabel();
        }

        public void UpdateLabel()
        {

            var selected = "";
            var minDist = float.MaxValue;
            if (labels == null || labels.Count == 0) return;
            foreach (var label2D in labels)
            {
                var dist = (label2D.point - value).magnitude;
                if (dist < minDist)
                {
                    selected = label2D.label;
                    minDist = dist;
                }
            }

            if (selected.Length > 0)
            {
                foreach (var spriteResolver in spriteObjects)
                {
                    if (spriteResolver == null) continue;
                    var catName = spriteResolver.GetCategory();

                    spriteResolver.SetCategoryAndLabel(catName, selected);

                    currentLabel = selected;
                }
            }
        }

        public void SetInput(Vector2 val)
        {
            value = val;
        }

    }
}
