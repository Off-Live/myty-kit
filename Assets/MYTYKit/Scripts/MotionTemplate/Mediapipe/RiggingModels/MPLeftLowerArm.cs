using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionTemplate.Mediapipe.Model
{
    public class MPLeftLowerArm : MPJointModel
    {
        Vector3 m_lastUp;

        void Start()
        {
            m_lastUp = Vector3.up;
        }

        void LateUpdate()
        {
            if (rawPoints == null) return;

            var upperArm = rawPoints[13] - rawPoints[11];
            var lowerArm = rawPoints[15] - rawPoints[13];

            upperArm.Normalize();
            lowerArm.Normalize();

            up = Vector3.Cross(upperArm, lowerArm);
            if (up.sqrMagnitude < 1.0e-6)
            {
                up = m_lastUp;
            }

            up.Normalize();
            lookAt = Vector3.Cross(lowerArm, up);
            lookAt.Normalize();

            m_lastUp = up;
            UpdateTemplate();
        }
    }
}