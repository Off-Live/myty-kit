using UnityEngine;

namespace MYTYKit.MotionTemplates.Mediapipe.Model
{
    public class MPLeftLittleIntermediate : MPJointModel
    {
        protected override void Process()
        {
            if (rawPoints == null) return;

            var palmAxis1 = rawPoints[5] - rawPoints[0];
            var palmAxis2 = rawPoints[17] - rawPoints[0];
            var palmPlane = Vector3.Cross(palmAxis1, palmAxis2);
            var intermediate = rawPoints[19] - rawPoints[18];
            var proximal = rawPoints[18] - rawPoints[17];

            palmPlane.Normalize();
            intermediate.Normalize();
            proximal.Normalize();

            var axis = Vector3.Cross(palmPlane, proximal);
            axis.Normalize();
            up = intermediate;
            lookAt = Vector3.Cross(axis, intermediate);
        }
    }
}
