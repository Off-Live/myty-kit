using UnityEngine;

namespace MYTYKit.MotionAdapters{
    public class NativeAdapter : MonoBehaviour
    {
        public int stabilizeWindow = 8;
        public int smoothWindow = 4;


        private Vector3[] m_vec3FilterArray;
        private Vector3[] m_vec3StabilizeArray;
        private Vector3 m_vec3LastValue;

        private Vector2[] m_vec2FilterArray;
        private Vector2[] m_vec2StabilizeArray;
        private Vector2 m_vec2LastValue;

        private float[] m_floatFilterArray;
        private float[] m_floatStabilizeArray;
        private float m_floatLastValue;


        private bool m_first = true;

        public virtual void Start()
        {
            m_vec3FilterArray = new Vector3[smoothWindow];
            m_vec3StabilizeArray = new Vector3[stabilizeWindow];


            m_vec2FilterArray = new Vector2[smoothWindow];
            m_vec2StabilizeArray = new Vector2[stabilizeWindow];

            m_floatFilterArray = new float[smoothWindow];
            m_floatStabilizeArray = new float[stabilizeWindow];
        }

        private Vector3 SmoothFilter(Vector3 newVal)
        {
            for (int i = 0; i < smoothWindow - 1; i++)
            {
                m_vec3FilterArray[i] = m_vec3FilterArray[i + 1];
            }

            m_vec3FilterArray[smoothWindow - 1] = newVal;

            Vector3 sum = Vector3.zero;
            for (int i = 0; i < smoothWindow; i++)
            {
                sum += m_vec3FilterArray[i];
            }

            return sum / smoothWindow;
        }

        protected void Stabilize(Vector3 newVal)
        {
            if (m_first)
            {
                m_vec3LastValue = newVal;
                for (int i = 0; i < smoothWindow; i++)
                {
                    m_vec3FilterArray[i] = newVal;
                }

                m_first = false;
            }

            newVal = SmoothFilter(newVal);
            for (int i = 1; i <= stabilizeWindow; i++)
            {
                m_vec3StabilizeArray[i - 1] = m_vec3LastValue + (newVal - m_vec3LastValue) / stabilizeWindow * i;
            }

        }

        protected Vector3 GetStabilizedVec3()
        {
            var ret = m_vec3StabilizeArray[0];
            for (int i = 0; i < stabilizeWindow - 1; i++)
            {
                m_vec3StabilizeArray[i] = m_vec3StabilizeArray[i + 1];
            }

            m_vec3LastValue = ret;
            return ret;
        }



        private Vector2 SmoothFilter(Vector2 newVal)
        {
            for (int i = 0; i < smoothWindow - 1; i++)
            {
                m_vec2FilterArray[i] = m_vec2FilterArray[i + 1];
            }

            m_vec2FilterArray[smoothWindow - 1] = newVal;

            Vector2 sum = Vector2.zero;
            for (int i = 0; i < smoothWindow; i++)
            {
                sum += m_vec2FilterArray[i];
            }

            return sum / smoothWindow;
        }

        protected void Stabilize(Vector2 newVal)
        {
            if (m_first)
            {
                m_vec2LastValue = newVal;
                for (int i = 0; i < smoothWindow; i++)
                {
                    m_vec2FilterArray[i] = newVal;
                }

                m_first = false;
            }

            newVal = SmoothFilter(newVal);
            for (int i = 1; i <= stabilizeWindow; i++)
            {
                m_vec2StabilizeArray[i - 1] = m_vec2LastValue + (newVal - m_vec2LastValue) / stabilizeWindow * i;
            }

        }

        protected Vector2 GetStabilizedVec2()
        {
            var ret = m_vec2StabilizeArray[0];
            for (int i = 0; i < stabilizeWindow - 1; i++)
            {
                m_vec2StabilizeArray[i] = m_vec2StabilizeArray[i + 1];
            }

            m_vec2LastValue = ret;
            return ret;
        }

        private float SmoothFilter(float newVal)
        {
            for (int i = 0; i < smoothWindow - 1; i++)
            {
                m_floatFilterArray[i] = m_floatFilterArray[i + 1];
            }

            m_floatFilterArray[smoothWindow - 1] = newVal;

            float sum = 0;
            for (int i = 0; i < smoothWindow; i++)
            {
                sum += m_floatFilterArray[i];
            }

            return sum / smoothWindow;
        }

        protected void Stabilize(float newVal)
        {
            if (m_first)
            {
                m_floatLastValue = newVal;
                for (int i = 0; i < smoothWindow; i++)
                {
                    m_floatFilterArray[i] = newVal;
                }

                m_first = false;
            }

            newVal = SmoothFilter(newVal);
            for (int i = 1; i <= stabilizeWindow; i++)
            {
                m_floatStabilizeArray[i - 1] = m_floatLastValue + (newVal - m_floatLastValue) / stabilizeWindow * i;
            }

        }

        protected float GetStabilizedFloat()
        {
            var ret = m_floatStabilizeArray[0];
            for (int i = 0; i < stabilizeWindow - 1; i++)
            {
                m_floatStabilizeArray[i] = m_floatStabilizeArray[i + 1];
            }

            m_floatLastValue = ret;
            return ret;
        }

    }

}
