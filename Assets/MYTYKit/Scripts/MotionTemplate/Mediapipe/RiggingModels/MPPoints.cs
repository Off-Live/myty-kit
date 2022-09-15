using UnityEngine;

namespace MYTYKit.MotionTemplate.Mediapipe.Model
{
    public class MPPoints : MPBaseModel
    {
        public override void UpdateTemplate()
        {
            if (templateList.Count == 0) return;
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