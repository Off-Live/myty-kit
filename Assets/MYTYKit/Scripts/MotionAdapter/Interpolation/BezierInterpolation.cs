using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

namespace MYTYKit.MotionAdapters.Interpolation
{
    public class BezierInterpolation : Interpolator
    {
        Vector3[] m_controlPointA;
        Vector3[] m_controlPointB;
    
        protected override void PrepareMethod()
        {
            if (GetIntervalCount() < 2) return;
            var n = m_points.Count - 1;
            var matC = Matrix<float>.Build.DenseDiagonal(n, 4);
            for (var i = 0; i < n-1; i++)
            {
                matC[i, i + 1] = 1;
                matC[i + 1, i] = 1;
            }

            matC[0, 0] = 2;
            matC[n - 1, n - 1] = 7;
            matC[n - 1, n - 2] = 2;

            var matP = Matrix<float>.Build.Dense(n, 3);
            for (var i = 1; i < n - 1; i++)
            {
                var rowVector = 2 * (2 * m_points[i] + m_points[i + 1]);
                matP.SetRow(i,new []{rowVector.x, rowVector.y, rowVector.z});
            }

            var topRow = m_points[0] + 2 * m_points[1];
            var bottomRow = 8 * m_points[n - 1] + m_points[n];
            matP.SetRow(0,new []{topRow.x, topRow.y, topRow.z});
            matP.SetRow(n-1,new []{bottomRow.x, bottomRow.y, bottomRow.z});

            var matA = matC.Solve(matP);
            m_controlPointA = new Vector3[n];
            m_controlPointB = new Vector3[n];
            
            for (var i = 0; i < n; i++)
            {
                m_controlPointA[i] = new Vector3(matA[i,0], matA[i,1], matA[i,2]);
            }

            for (var i = 0; i < n-1; i++)
            {
                m_controlPointB[i] = 2 * m_points[i + 1] - m_controlPointA[i + 1];
            }

            m_controlPointB[n - 1] = (m_controlPointA[n - 1] + m_points[n]) / 2;
        }

        Vector3 InterpolateCubicBezier(float t, Vector3 p0, Vector3 c0, Vector3 c1, Vector3 p1)
        {
            return Mathf.Pow(1-t,3)*p0 + 3*Mathf.Pow(1-t,2)*t*c0 + 3* (1-t)*Mathf.Pow(t,2)*c1+Mathf.Pow(t,3)*p1;
        }

        public override Vector3 Interpolate(int intervalIndex, float t)
        {
            return InterpolateCubicBezier(t,m_points[intervalIndex], m_controlPointA[intervalIndex],
                m_controlPointB[intervalIndex], m_points[intervalIndex + 1]);
            
        }

        public override int GetInterestedIntervalIndex()
        { 
            var numInterval = GetIntervalCount();
            if (numInterval < 2) return -1;
            return numInterval - 2;
        }
    }
}