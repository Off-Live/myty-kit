using MYTYKit.ThirdParty.MeFaMo;
using UnityEngine;

using FaceBlendShape = MYTYKit.ThirdParty.MeFaMo.MeFaMoConfig.FaceBlendShape;

namespace MYTYKit.MotionTemplates.Mediapipe.Model
{
    public class MPSimpleFace : MPSolverModel
    {
        public float leftEye;
        public float rightEye;

        public float leftEyeBrow;
        public float rightEyeBrow;

        public Vector2 leftPupil;
        public Vector2 rightPupil;

        public float mouthX;
        public float mouthY;

       

        protected override void Process()
        {
            if (m_solver == null) return;
            leftPupil = m_solver.leftPupil;
            rightPupil = m_solver.rightPupil;

            leftEye = 1.0f - m_solver.blendShape[FaceBlendShape.EyeBlinkLeft];
            rightEye = 1.0f - m_solver.blendShape[FaceBlendShape.EyeBlinkRight];

            var mouthPouker = m_solver.blendShape[FaceBlendShape.MouthPucker];
            var mouthLeftHalf = m_solver.blendShape[FaceBlendShape.MouthSmileLeft] +
                                m_solver.blendShape[FaceBlendShape.MouthStretchLeft];
            var mouthRightHalf = m_solver.blendShape[FaceBlendShape.MouthSmileRight] +
                                m_solver.blendShape[FaceBlendShape.MouthStretchRight];
            var mouthNeutralX = 0.4f;

            if (mouthPouker > 0)
            {
                mouthX = (1 - mouthPouker) * mouthNeutralX;
            }
            else
            {
                mouthX = mouthNeutralX + (mouthLeftHalf + mouthRightHalf) * (1 - mouthNeutralX) * 0.5f;
            }
            mouthY = m_solver.blendShape[FaceBlendShape.JawOpen];

            var eyebrowNeutral = 0.5f;
            leftEyeBrow = (1.0f- m_solver.blendShape[FaceBlendShape.BrowDownLeft]/0.4f) * eyebrowNeutral +
                          m_solver.blendShape[FaceBlendShape.BrowOuterUpLeft]*(1.0f-eyebrowNeutral);
            rightEyeBrow = (1.0f- m_solver.blendShape[FaceBlendShape.BrowDownRight]/0.4f) * eyebrowNeutral +
                             m_solver.blendShape[FaceBlendShape.BrowOuterUpRight]*(1.0f-eyebrowNeutral);
            m_solver = null;
        }

        protected override void UpdateTemplate()
        {
            if (templateList.Count == 0) return;

            foreach (var motionTemplate in templateList)
            {
                var template = (ParametricTemplate)motionTemplate;
               
                template.SetValue("leftEye",leftEye);
                template.SetValue("rightEye",rightEye);
                template.SetValue("leftEyeBrow", leftEyeBrow);
                template.SetValue("rightEyeBrow", rightEyeBrow);
                template.SetValue("leftPupilX", leftPupil.x);
                template.SetValue("leftPupilY", leftPupil.y);
                template.SetValue("rightPupilX", rightPupil.x);
                template.SetValue("rightPupilY", rightPupil.y);
                template.SetValue("mouthX", mouthX);
                template.SetValue("mouthY", mouthY);
                template.NotifyUpdate();
                
            }
            
        }
    }
}