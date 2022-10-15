using UnityEngine;

namespace MYTYKit.MotionAdapters.Interpolation
{
    public class LinearInterpolation : Interpolator
    {
        public override Vector3 Interpolate(int intervalIndex, float t)
        {
            return (1 - t) * m_points[intervalIndex] + t * m_points[intervalIndex + 1];
        }

        public override int GetInterestedIntervalIndex()
        {
            var numInterval = GetIntervalCount();
            if (numInterval < 1) return -1;
            return numInterval - 1;
        }

        protected override void PrepareMethod()
        {
            
        }
    }
}