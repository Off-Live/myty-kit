using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionTemplates.Mediapipe.Model
{
    public class MPRightRingDistal : MPJointModel
    {
        protected override void Process()
        {
            if (rawPoints == null) return;

            var palmAxis1 = rawPoints[5] - rawPoints[0];
            var palmAxis2 = rawPoints[17] - rawPoints[0];
            var palmPlane = -Vector3.Cross(palmAxis1, palmAxis2);
            var distal = rawPoints[16] - rawPoints[15];
            var proximal = rawPoints[14] - rawPoints[13];

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
