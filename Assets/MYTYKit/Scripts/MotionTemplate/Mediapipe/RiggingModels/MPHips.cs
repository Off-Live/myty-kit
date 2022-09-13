using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionTemplate.Mediapipe.Model
{
    public class MPHips : MPJointModel
    {
        void LateUpdate()
        {
            if (rawPoints == null) return;
            var hipLr = rawPoints[23] - rawPoints[24];
            up = Vector3.up;
            hipLr.Normalize();
            lookAt = Vector3.Cross(hipLr, up);
            up = Vector3.Cross(lookAt, hipLr);
            UpdateTemplate();
        }
    }
}
