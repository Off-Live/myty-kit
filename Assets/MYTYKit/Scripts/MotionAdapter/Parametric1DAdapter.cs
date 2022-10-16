using MYTYKit.Controllers;
using UnityEngine;
using MYTYKit.MotionTemplates;

namespace MYTYKit.MotionAdapters
{
    public class Parametric1DAdapter : DampingAndStabilizingVec3Adapter, ITemplateObserver
    {
        public ParametricTemplate template;
        public string paramName;
        public MYTYController con;

        protected override void Start()
        {
            base.Start();
            ListenToMotionTemplate();
            SetNumInterpolationSlot(1);
        }
        public void TemplateUpdated()
        {
            AddToHistory(new Vector3(template.GetValue(paramName),0,0));
        }

        public void ListenToMotionTemplate()
        {
            template.SetUpdateCallback(TemplateUpdated);
        }

        void Update()
        {
            var input = con as IFloatInput;
            if (input == null) return;
            input.SetInput(GetResult().x);
        }
    }
}
