using UnityEngine;

namespace MYTYKit.MotionTemplates.Mediapipe.Model
{
    public class MPHead : MPJointModel
    {
        
        void LateUpdate()
        {
            if (rawPoints == null) return;

            var faceLr = rawPoints[263] - rawPoints[33]; //33 263 are end point of eyes
            up = rawPoints[151] - rawPoints[200];
            faceLr.Normalize();
            up.Normalize();
            lookAt = -Vector3.Cross(up.normalized, faceLr.normalized);
            up = -Vector3.Cross(faceLr, lookAt);
            UpdateTemplate();
        }

    }
    
    
}


