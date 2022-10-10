using System;
using UnityEngine;

namespace MYTYKit.MotionTemplates.Mediapipe.Model
{
    public class MPPoints : MPBaseModel
    {
        void LateUpdate()
        {
            UpdateTemplate();
        }

        public override void UpdateTemplate()
        {
            if (templateList.Count == 0) return;
            if (rawPoints == null) return;
            foreach (var motionTemplate in templateList)
            {
                var pointsTemplate = motionTemplate as PointsTemplate;
                if (pointsTemplate.points == null ||
                    pointsTemplate.points.Length != rawPoints.Length)
                {
                    pointsTemplate.points = new Vector3[rawPoints.Length];
                }

                for (int i = 0; i < pointsTemplate.points.Length; i++)
                {
                    pointsTemplate.points[i] = rawPoints[i];
                }
            }
        }
    }
}