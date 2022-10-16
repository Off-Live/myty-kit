using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionTemplates.Mediapipe.Model
{
    public class MPLeftLowerArm : MPJointModel
    {
        Vector3 m_lastLookAt;

        void Start()
        {
            m_lastLookAt = Vector3.up;
        }

        protected override void Process()
        {
            if (rawPoints == null) return;

            var upperArm = rawPoints[13] - rawPoints[11];
            var lowerArm = rawPoints[15] - rawPoints[13];

            upperArm.Normalize();
            lowerArm.Normalize();
            up = lowerArm.normalized;
            lookAt = -Vector3.Cross(upperArm, lowerArm);
            if (lookAt.sqrMagnitude < 1.0e-6)
            {
                lookAt = m_lastLookAt;
            }
            lookAt.Normalize();
            m_lastLookAt = lookAt;
            lookAt.z = -lookAt.z;
            up.z = -up.z;
        }
    }
}