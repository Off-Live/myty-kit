using System.Collections.Generic;
using MYTYKit.Controllers;
using UnityEngine;

using MYTYKit.MotionTemplates;
namespace MYTYKit.MotionAdapters
{
    public class WeightedSum2DAdapterV2 : DampingAndStabilizingVec3Adapter, ITemplateObserver
    {

        public ParametricTemplate template;
        public List<string> xParamNames;
        public List<string> yParamNames;
        public List<float> weights;
        public MYTYController controller;

        public void TemplateUpdated()
        {
            var weightedSum = Vector2.zero;
            for (int i = 0; i < weights.Count; i++)
            {
                weightedSum += weights[i] *
                               new Vector2(template.GetValue(xParamNames[i]), template.GetValue(yParamNames[i]));
            }
            AddToHistory(weightedSum);
        }
        void Update()
        {
            if (xParamNames.Count != yParamNames.Count) return;
            if (xParamNames.Count != weights.Count) return;
            var input = controller as IVec2Input;
            if (input == null) return;

            input.SetInput(GetResult());
        }
    }
}
