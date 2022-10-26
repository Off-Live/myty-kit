using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionTemplates.Mediapipe.Model
{
    public class MPNeck : MPJointModel
    {
        protected override void Process()
        {
            if (rawPoints == null) return;
            var shoulderLR = rawPoints[11] - rawPoints[12];
            var shoulderHalf = (rawPoints[11] + rawPoints[12]) / 2;
            var headCenter = (rawPoints[7] + rawPoints[8]) / 2;
            up = headCenter - shoulderHalf;
            up.Normalize();
            shoulderLR.Normalize();
            lookAt = Vector3.Cross(shoulderLR, up);
            lookAt.z = -lookAt.z;
            up.z = -up.z;
        }
    }
}
