using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionTemplate.Mediapipe.Model
{
    public class MPChest : MPJointModel
    {
        
        void LateUpdate()
        {
            if (rawPoints == null) return;

            var shoulderLR = rawPoints[11] - rawPoints[12];
            var hipLR = rawPoints[23] - rawPoints[24];
            var shoulderHalf = 0.5f * (rawPoints[11] + rawPoints[12]);
            var hipHalf = 0.5f * (rawPoints[23] + rawPoints[24]);
            up = shoulderHalf - hipHalf;
            up.Normalize();
            shoulderLR.Normalize();
            hipLR.Normalize();

            lookAt = -Vector3.Cross(up, shoulderLR);
            up = -Vector3.Cross(shoulderLR, lookAt);
            UpdateTemplate();

        }
    }
}
