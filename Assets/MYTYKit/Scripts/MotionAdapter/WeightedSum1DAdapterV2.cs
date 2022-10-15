using System.Collections.Generic;
using System.Security.Permissions;
using MYTYKit.Controllers;
using UnityEngine;

using MYTYKit.MotionTemplates;

namespace MYTYKit.MotionAdapters
{
    public class WeightedSum1DAdapterV2 : DampingAndStabilizingVec3Adapter, ITemplateObserver
    {

        public ParametricTemplate template;
        public List<string> paramNames;
        public List<float> weights;
        public MYTYController controller;

        public void TemplateUpdated()
        {
            var weightedSum = 0.0f;
            for (int i = 0; i < weights.Count; i++)
            {
                weightedSum += weights[i] * template.GetValue(paramNames[i]);
            }
            AddToHistory(new Vector3(weightedSum,0,0));
        }
        void Update()
        {
            if (paramNames.Count != weights.Count) return;
            var input = controller as IFloatInput;
            if (input == null) return;

            input.SetInput(GetResult().x);
        }
    }
}
