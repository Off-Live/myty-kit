using System;
using System.Linq;
using UnityEngine;

namespace MYTYKit.MotionTemplates.Mediapipe.Model
{
    public class MPPoints : MPBaseModel
    {
        protected override void Process()
        {
            
        }

        protected override void UpdateTemplate()
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

                if (pointsTemplate.visibilities == null ||
                    pointsTemplate.visibilities.Length != visibilities.Length)
                {
                    pointsTemplate.visibilities = new float[visibilities.Length];
                }
                
                Debug.Assert(pointsTemplate.points.Length==pointsTemplate.visibilities.Length);
                
                Enumerable.Range(0, pointsTemplate.points.Length).ToList().ForEach(idx =>
                {
                    pointsTemplate.points[idx] = rawPoints[idx];
                    pointsTemplate.visibilities[idx] = visibilities[idx];
                });
                pointsTemplate.NotifyUpdate();
            }
        }
    }
}