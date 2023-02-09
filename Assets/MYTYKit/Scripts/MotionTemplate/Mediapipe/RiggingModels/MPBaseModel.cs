using UnityEngine;

namespace MYTYKit.MotionTemplates.Mediapipe.Model
{
    public abstract class MPBaseModel : MotionTemplateBridge
    {
        protected Vector3[] rawPoints;
        protected float[] visibilities;
        
        public int GetNumPoints()
        {
            if (rawPoints == null) return 0;
            return rawPoints.Length;
        }

        public void Alloc(int numPoints)
        {
            rawPoints = new Vector3[numPoints];
            visibilities = new float[numPoints];
        }

        public void SetPoint(int index, Vector3 point, float visibility)
        {
            rawPoints[index] = new Vector3(-point.x, -point.y, point.z);
            visibilities[index] = visibility;
        }
    }
}