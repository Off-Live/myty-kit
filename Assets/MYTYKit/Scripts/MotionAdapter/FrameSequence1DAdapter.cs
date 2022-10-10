using MYTYKit.Controllers;
using UnityEngine;

namespace MYTYKit.MotionAdapters
{
    public class FrameSequence1DAdapter : NativeAdapter
    {
        public MYTYController controller;

        public float start = 0.0f;
        public float end = 1.0f;
        public bool repeat = true;
        public bool swing = false;

        public float stepCount = 10;
        public float unitTime = 0.05f;


        private float m_curValue = 0.0f;
        private float m_elapsed = 0.0f;

        private bool m_stopped = false;
        private bool m_forward = true;

        private IFloatInput m_input;

        public override void Start()
        {
            base.Start();
            m_input = controller as IFloatInput;
            if (m_input == null) return;
            m_input.SetInput(m_curValue);

        }

        void Update()
        {
            if (m_input == null) return;
            if (m_stopped) return;

            if (m_elapsed < unitTime)
            {
                m_elapsed += Time.deltaTime;
                return;
            }

            m_elapsed = 0.0f;

            var step = (end - start) / stepCount;

            if (!m_forward) step *= -1;
            m_curValue += step;

            m_input.SetInput(m_curValue);

            if (m_forward)
            {
                if (Mathf.Abs(m_curValue - end) < 1.0e-6)
                {

                    if (!repeat) m_stopped = true;
                    else
                    {
                        if (swing) m_forward = false;
                        else m_curValue = start;
                    }
                }
            }
            else
            {
                if (Mathf.Abs(m_curValue - start) < 1.0e-6)
                {
                    m_forward = true;
                }
            }


        }
    }
}