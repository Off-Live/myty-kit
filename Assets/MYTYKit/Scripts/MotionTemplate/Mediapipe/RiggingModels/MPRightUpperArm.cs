using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionTemplate.Mediapipe.Model
{
    public class MPRightUpperArm : MPJointModel
    {
        Vector3 m_lastUp;

        void Start()
        {
            m_lastUp = Vector3.up;
        }

        void LateUpdate()
        {
            if (rawPoints == null) return;

            var upperArm = rawPoints[12] - rawPoints[14];
            var lowerArm = rawPoints[14] - rawPoints[16];

            upperArm.Normalize();
            lowerArm.Normalize();

            up = Vector3.Cross(lowerArm, upperArm);
            if (up.sqrMagnitude < 1.0e-6)
            {
                up = m_lastUp;
            }

            up.Normalize();
            lookAt = Vector3.Cross(upperArm, up);
            lookAt.Normalize();

            m_lastUp = up;
            UpdateAnchor();
        }
    }
}