using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionTemplates.Mediapipe.Model
{
    public class MPRightLowerLeg : MPJointModel
    {
        Vector3 m_lastLA;
        void Start()
        {
            m_lastLA = Vector3.forward;
        }


        protected override void Process()
        {
            if (rawPoints == null) return;
            var upperLeg = rawPoints[24] - rawPoints[26];
            var lowerLeg = rawPoints[26] - rawPoints[28];
            upperLeg.Normalize();
            lowerLeg.Normalize();

            up = lowerLeg;
            var axis = Vector3.Cross(lowerLeg, upperLeg);
            if (axis.magnitude < 1.0e-6)
            {
                lookAt = m_lastLA;
            }
            else
            {
                lookAt = Vector3.Cross(axis, up);
                lookAt.Normalize();
                m_lastLA = lookAt;
            }
        }
    }
}
