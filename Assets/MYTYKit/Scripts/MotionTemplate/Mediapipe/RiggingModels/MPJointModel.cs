using UnityEngine;

namespace MYTYKit.MotionTemplate.Mediapipe.Model
{
    public class MPJointModel:RiggingModel, IMTBrigde
    {
        protected TransformAnchor anchor;
        protected bool isAnchorSet;
        protected Vector3 up, lookAt;
        public void SetMotionTemplate(IMotionTemplate anchor)
        {
            this.anchor = anchor as TransformAnchor;
            isAnchorSet = true;
        }

        public void UpdateTemplate()
        {
            if (!isAnchorSet) return;
            anchor.up = up;
            anchor.lookAt = lookAt;
        }
    }
}