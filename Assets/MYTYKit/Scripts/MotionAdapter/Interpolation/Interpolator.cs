using System.Collections.Generic;
using UnityEngine;
namespace MYTYKit.MotionAdapters.Interpolation
{
    public abstract class Interpolator
    {
        protected List<Vector3> m_points;

        public void SetPoints(List<Vector3> points)
        {
            m_points = points;
            PrepareMethod();
        }

        public int GetIntervalCount()
        {
            if (m_points == null || m_points.Count == 0) return 0;
            return m_points.Count - 1;
        }
        
        public abstract Vector3 Interpolate(int intervalIndex, float t);
        public abstract int GetInterestedIntervalIndex();
        protected abstract void PrepareMethod();
        
    }
}