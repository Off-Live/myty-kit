using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionTemplates.Mediapipe.Model
{
    public class MPHips : MPJointModel
    {
        protected override void Process()
        {
            if (rawPoints == null) return;
            var hipLr = rawPoints[23] - rawPoints[24];
            up = Vector3.up;
            hipLr.Normalize();
            lookAt = Vector3.Cross(hipLr, up);
            up = Vector3.Cross(lookAt, hipLr);
            lookAt.z = -lookAt.z;
            up.z = -up.z;
        }
    }
}
