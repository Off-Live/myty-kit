using UnityEngine;

namespace MYTYKit.MotionTemplate.Mediapipe.Model
{
    public class MPRightThumbIntermediate : MPJointModel
    {
        void LateUpdate()
        {
            if (rawPoints == null) return;

            var proximal = rawPoints[2] - rawPoints[1];
            var indexprox = rawPoints[5] - rawPoints[1];
            var thumbPlane = Vector3.Cross(proximal, indexprox);
            var intermediate = rawPoints[3] - rawPoints[2];

            thumbPlane.Normalize();
            indexprox.Normalize();
            proximal.Normalize();
            intermediate.Normalize();

            up = intermediate;
            lookAt = Vector3.Cross(thumbPlane, proximal);
            UpdateAnchor();
        }
    }
}