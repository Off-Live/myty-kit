using MYTYKit.Controllers;
using UnityEngine;

using MYTYKit.MotionTemplates;

namespace MYTYKit.MotionAdapters
{
    public class Parametric1DAdapter : NativeAdapter
    {
        public ParametricTemplate template;
        public string paramName;
        public MYTYController con;

        public float stabilizeTime = 0.1f;
        float m_elapsed = 0;

        void Update()
        {
            var input = con as IFloatInput;
            if (input == null) return;

            m_elapsed += Time.deltaTime;
            if (m_elapsed < stabilizeTime)
            {
                input.SetInput(GetStabilizedFloat());
                return;
            }

            m_elapsed = 0;

            float val = template.GetValue(paramName);
            Stabilize(val);
            input.SetInput(GetStabilizedFloat());

        }
    }
}
