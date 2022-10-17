using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionTemplates.Mediapipe.Model
{
    public class MPRightThumbDistal : MPJointModel
    {
        protected override void Process()
        {
            if (rawPoints == null) return;

            var proximal = rawPoints[2] - rawPoints[1];
            var indexprox = rawPoints[5] - rawPoints[1];
            var thumbPlane = Vector3.Cross(proximal, indexprox);
            var distal = rawPoints[4] - rawPoints[3];

            thumbPlane.Normalize();
            indexprox.Normalize();
            proximal.Normalize();
            distal.Normalize();

            up = distal;
            lookAt = Vector3.Cross(thumbPlane, proximal);
        }
    }
}
