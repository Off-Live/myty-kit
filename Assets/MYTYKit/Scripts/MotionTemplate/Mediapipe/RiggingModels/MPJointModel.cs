using UnityEngine;

namespace MYTYKit.MotionTemplate.Mediapipe.Model
{
    public class MPJointModel: MPBaseModel
    {
        protected Vector3 up, lookAt;
        
        public override void UpdateTemplate()
        {
            if (templateList.Count == 0) return;
            foreach (var motionTemplate in templateList)
            {
                var anchorTemplate = (AnchorTemplate)motionTemplate;
                anchorTemplate.up = up;
                anchorTemplate.lookAt = lookAt;
            }
        }
    }
}