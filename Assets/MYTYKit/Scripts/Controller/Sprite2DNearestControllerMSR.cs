using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Reflection.Emit;
using Newtonsoft.Json.Linq;


namespace MYTYKit.Controllers
{
    public class Sprite2DNearestControllerMSR : MSRSpriteController, IVec2Input, IComponentWiseInput
    {
        [Serializable]
        public class Label2D
        {
            public string label;
            public Vector2 point;
        }

        public Vector2 bottomLeft = new Vector2(0, 0);
        public Vector2 topRight = new Vector2(1, 1);
        public Vector2 value = new Vector2(0, 0);

        public List<Label2D> labels;

        [SerializeField] private string currentLabel;

        string m_lastLabel="";
        void Update()
        {
            if (spriteObjects == null || labels == null) return;
            
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

            if (m_lastLabel != selected)
            {
                m_lastLabel = selected;
                UpdateLabel();
            }
        }

        public void UpdateLabel()
        {
            if (m_lastLabel.Length > 0)
            {
                foreach (var spriteResolver in spriteObjects)
                {
                    if (spriteResolver == null) continue;
                    var catName = spriteResolver.GetCategory();

                    spriteResolver.SetCategoryAndLabel(catName, m_lastLabel);

                    currentLabel = m_lastLabel;
                }
            }
        }

        public void SetInput(Vector2 val)
        {
            value = val;
        }

        public void SetComponent(float value, int componentIdx)
        {
            if (componentIdx >= 2 || componentIdx < 0) return;
            this.value[componentIdx] = value;
        }

        public override JObject SerializeToJObject(Dictionary<Transform, int> tfMap)
        {
            var baseJo = base.SerializeToJObject(tfMap);
            baseJo.Merge(JObject.FromObject(new
            {
                type = GetType().Name,
                bottomLeft = new
                {
                    bottomLeft.x,
                    bottomLeft.y
                },
                topRight = new
                {
                    topRight.x,
                    topRight.y
                },
                labels = labels.Select(item => JObject.FromObject(new
                {
                    item.label,
                    point = new
                    {
                        item.point.x,
                        item.point.y
                    }
                })).ToArray()
            }));
            return baseJo;
        }
    }
}