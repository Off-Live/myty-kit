using UnityEngine;

namespace MYTYKit.MotionTemplate.Mediapipe.Model
{
    public class MPPoints : RiggingModel, IMTBridge
    {
        PointsTemplate m_pointsTemplate;
        public void SetMotionTemplate(IMotionTemplate template)
        {
            m_pointsTemplate = template as PointsTemplate; 
        }

        public void UpdateTemplate()
        {
            if (m_pointsTemplate.points == null ||
                m_pointsTemplate.points.Length != rawPoints.Length)
            {
                m_pointsTemplate.points = new Vector3[rawPoints.Length];
            }

            for (int i = 0; i < m_pointsTemplate.points.Length; i++)
            {
                m_pointsTemplate.points[i] = rawPoints[i];
            }
        }
    }
}