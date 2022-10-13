using UnityEngine;

namespace MYTYKit.MotionTemplates.Mediapipe.Model
{
    public class MPHead : MPJointModel
    {
        
        void LateUpdate()
        {
            if (rawPoints == null) return;

            var faceLr = rawPoints[234] - rawPoints[454]; //33 263 are end point of eyes
            up =  rawPoints[10] - rawPoints[152];
            faceLr.Normalize();
            up.Normalize();
            
            lookAt = Vector3.Cross(up, faceLr).normalized;
            up = Vector3.Cross(faceLr, lookAt).normalized;
            lookAt.z = -lookAt.z;
            up.z = -up.z;
            UpdateTemplate();
        }

    }
    
    
}


