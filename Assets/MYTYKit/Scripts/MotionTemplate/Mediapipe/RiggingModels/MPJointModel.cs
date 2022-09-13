using UnityEngine;

namespace MYTYKit.MotionTemplate.Mediapipe.Model
{
    public class MPJointModel:RiggingModel, IMTBridge
    {
        protected AnchorTemplate anchorTemplate;
        protected bool isTemplateSet;
        protected Vector3 up, lookAt;
        public void SetMotionTemplate(IMotionTemplate template)
        {
            this.anchorTemplate = template as AnchorTemplate;
            isTemplateSet = true;
        }

        public void UpdateTemplate()
        {
            if (!isTemplateSet) return;
            anchorTemplate.up = up;
            anchorTemplate.lookAt = lookAt;
        }
    }
}