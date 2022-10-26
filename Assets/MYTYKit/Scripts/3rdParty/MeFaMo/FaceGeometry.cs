using System;
using UnityEngine;

namespace MYTYKit.ThirdParty.MeFaMo
{
    public class FaceGeometry
    {
        Vector3[] m_canonicalMetricLandmarks = new Vector3[468];
        float[] m_landmarkWeights = new float[468];
        Vector3[] m_normalizedLandmarks = new Vector3[468];
        PerspectiveCameraFrustum m_pcf;


        public FaceGeometry(Vector3[] landmarks, PerspectiveCameraFrustum pcf)
        {
            for (int i = 0; i < 468; i++)
            {
                m_normalizedLandmarks[i] = landmarks[i];
                m_landmarkWeights[i] = 0.0f;
                m_canonicalMetricLandmarks[i] = new Vector3(
                    (float)MeFaMoConfig.canonical_metric_landmarks[i * 5],
                    (float)MeFaMoConfig.canonical_metric_landmarks[i * 5 + 1],
                    (float)MeFaMoConfig.canonical_metric_landmarks[i * 5 + 2]
                );

            }

            var keyIndices = MeFaMoConfig.procrustes_landmark_basis.Keys;
            foreach (var key in keyIndices)
            {
                m_landmarkWeights[key] = (float)MeFaMoConfig.procrustes_landmark_basis[key];
            }
        

            m_pcf = pcf;
        }

        public Vector3[] GetMetricLandmarks()
        {
            var screenLandmarks = ProjectXY(m_normalizedLandmarks);
            var depthOffset = 0.0f;
            for (int i = 0; i < screenLandmarks.Length; i++)
            {
                depthOffset += screenLandmarks[i].z;
            }

            depthOffset /= screenLandmarks.Length;

            var intermediateLandmarks = new Vector3[screenLandmarks.Length];
            Array.Copy(screenLandmarks, intermediateLandmarks, screenLandmarks.Length);
            ChangeHandedness(intermediateLandmarks);
            var firstIterationScale = EstimateScale(intermediateLandmarks);
        
            Array.Copy(screenLandmarks, intermediateLandmarks, screenLandmarks.Length);
            MoveAndRescaleZ(depthOffset,firstIterationScale,intermediateLandmarks);
            intermediateLandmarks = UnprojectXY(intermediateLandmarks);
            ChangeHandedness(intermediateLandmarks);
            var secondIterationScale = EstimateScale(intermediateLandmarks);

            var metricLandmarks = new Vector3[screenLandmarks.Length];
            Array.Copy(screenLandmarks,metricLandmarks,screenLandmarks.Length);
            var totalScale = firstIterationScale * secondIterationScale;
            MoveAndRescaleZ(depthOffset,totalScale,metricLandmarks);
            metricLandmarks = UnprojectXY(metricLandmarks);
            ChangeHandedness(metricLandmarks);

            var poseTransformMat =
                ProcrustesSolver.SolveWeightedOrthogonalProblem(m_canonicalMetricLandmarks, metricLandmarks,
                    m_landmarkWeights);
            var invPoseTransformMat = poseTransformMat.inverse;

            for (int i = 0; i < metricLandmarks.Length; i++)
            {
                metricLandmarks[i] = invPoseTransformMat.MultiplyPoint3x4(metricLandmarks[i]);
            }
            ChangeHandedness(metricLandmarks);

            return metricLandmarks;
        }

        Vector3[] ProjectXY(Vector3[] landmarks)
        {
            var newLandmarks = new Vector3[landmarks.Length];
            Array.Copy(landmarks,newLandmarks, landmarks.Length);
            var x_scale = m_pcf.right - m_pcf.left;
            var y_scale = m_pcf.top - m_pcf.bottom;
            var x_translation = m_pcf.left;
            var y_translation = m_pcf.bottom;

            for (int i = 0; i < newLandmarks.Length; i++)
            {
                newLandmarks[i].y = 1.0f - newLandmarks[i].y;
            }

            for (int i = 0; i < newLandmarks.Length; i++)
            {
                newLandmarks[i] = new Vector3(
                    newLandmarks[i].x * x_scale + x_translation,
                    newLandmarks[i].y * y_scale + y_translation,
                    newLandmarks[i].z * x_scale
                );
            }

            return newLandmarks;
        }

        void ChangeHandedness(Vector3[] landmarks)
        {
            for (int i = 0; i < landmarks.Length; i++)
            {
                landmarks[i].z *= -1.0f;
            }
        }

        void MoveAndRescaleZ(float depthOffset, float scale, Vector3[] landmarks)
        {
            for (int i = 0; i < landmarks.Length; i++)
            {
                landmarks[i].z = (landmarks[i].z - depthOffset + m_pcf.near) / scale;
            }
        }

        Vector3[] UnprojectXY(Vector3[] landmarks)
        {
            var newLandmarks = new Vector3[landmarks.Length];
            Array.Copy(landmarks,newLandmarks, landmarks.Length);
            for (int i = 0; i < newLandmarks.Length; i++)
            {
                newLandmarks[i].x = newLandmarks[i].x * newLandmarks[i].z / m_pcf.near;
                newLandmarks[i].y = newLandmarks[i].y * newLandmarks[i].z / m_pcf.near;
            }

            return newLandmarks;
        }

        float EstimateScale(Vector3[] landmarks)
        {
            var transformMat =
                ProcrustesSolver.SolveWeightedOrthogonalProblem(m_canonicalMetricLandmarks, landmarks, m_landmarkWeights);
            return transformMat.GetColumn(0).magnitude;
        }
    
    
    
    
    }
}
