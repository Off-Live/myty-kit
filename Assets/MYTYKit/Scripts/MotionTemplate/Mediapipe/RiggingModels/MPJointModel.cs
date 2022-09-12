using UnityEngine;

namespace MYTYKit.MotionTemplate.Mediapipe.Model
{
    public class MPJointModel:RiggingModel, IAnchorBrigde
    {
        protected TransformAnchor anchor;
        protected bool isAnchorSet;
        protected Vector3 up, lookAt;
        public void SetAnchor(IMYTYAnchor anchor)
        {
            this.anchor = anchor as TransformAnchor;
            isAnchorSet = true;
        }

        public void UpdateAnchor()
        {
            if (!isAnchorSet) return;
            anchor.up = up;
            anchor.lookAt = lookAt;
        }
    }
}