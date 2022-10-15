using System;
using System.Collections.Generic;

namespace MYTYKit.MotionAdapters.Interpolation
{
    public enum InterpolationMethod
    {
        LinearInterpolation, BezierInterpolation
    }
    public static class InterpolationConfig
    {
        public static Dictionary<InterpolationMethod, Type> interpMap = new()
        {
            { InterpolationMethod.LinearInterpolation, typeof(LinearInterpolation) },
            { InterpolationMethod.BezierInterpolation, typeof(BezierInterpolation) }
        };
    }
}