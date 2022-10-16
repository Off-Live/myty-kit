using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionTemplates.Mediapipe.Model
{
    public class MPRightLittleDistal : MPJointModel
    {
        protected override void Process()
        {
            if (rawPoints == null) return;

            var palmAxis1 = rawPoints[5] - rawPoints[0];
            var palmAxis2 = rawPoints[17] - rawPoints[0];
            var palmPlane = -Vector3.Cross(palmAxis1, palmAxis2);
            var distal = rawPoints[20] - rawPoints[19];
            var proximal = rawPoints[18] - rawPoints[17];

            palmPlane.Normalize();
            distal.Normalize();
            proximal.Normalize();

            var axis = Vector3.Cross(palmPlane, proximal);
            axis.Normalize();
            up = distal;
            lookAt = Vector3.Cross(axis, distal);
        }
    }
}
