using UnityEngine;

namespace MYTYKit.MotionTemplate.Mediapipe.Model
{
    public class MPFace : RiggingModel, IMTBrigde
    {
        ParametricTemplate m_template;
        bool m_isAnchorSet;
        
        public float leftEye;
        public float rightEye;

        public float leftEyeBrow;
        public float rightEyeBrow;

        public Vector2 leftPupil;
        public Vector2 rightPupil;

        public float mouthX;
        public float mouthY;
        void LateUpdate()
        {
            var faceLM = rawPoints;
            if (faceLM == null || faceLM.Length != 478) return;

            FaceSolver.GetEyeOpen(out leftEye, out _, true, faceLM);
            FaceSolver.GetEyeOpen(out rightEye, out _, false, faceLM);
            //FaceSolver.StabilizeBlink(out leftEye, out rightEye, leftEye, rightEye, 0.0f, false);
            //if (Mathf.Min(leftEye, rightEye) > 0.5)
            //{
            //    leftEye = rightEye;
            //}
        

            leftPupil = FaceSolver.GetPupilPosition(faceLM, true);
            rightPupil = FaceSolver.GetPupilPosition(faceLM, false);

            leftPupil = new Vector2(Mathf.Clamp(leftPupil.x / 0.3f, -1.0f,1.0f), -Mathf.Clamp(leftPupil.y / 0.3f,-1.0f,1.0f));
            rightPupil = new Vector2(Mathf.Clamp(rightPupil.x / 0.3f,-1.0f,1.0f), -Mathf.Clamp(rightPupil.y / 0.3f,-1.0f,1.0f));

            leftEyeBrow = FaceSolver.GetBrowRaise(faceLM, true);
            rightEyeBrow = FaceSolver.GetBrowRaise(faceLM, false);

            FaceSolver.CalcMouth(faceLM,
                out mouthX,
                out mouthY);
           
            UpdateTemplate();
        }
        public void SetMotionTemplate(IMotionTemplate anchor)
        {
            m_template = anchor as ParametricTemplate;
            if(m_template!=null) m_isAnchorSet = true;
        }

        public void UpdateTemplate()
        {
            if (!m_isAnchorSet) return;
            m_template.SetValue("leftEye",leftEye);
            m_template.SetValue("rightEye",rightEye);
            m_template.SetValue("leftEyeBrow", leftEyeBrow);
            m_template.SetValue("rightEyeBrow", rightEyeBrow);
            m_template.SetValue("leftPupilX", leftPupil.x);
            m_template.SetValue("leftPupilY", leftPupil.y);
            m_template.SetValue("rightPupilX", rightPupil.x);
            m_template.SetValue("rightPupilY", rightPupil.y);
            m_template.SetValue("mouthX", mouthX);
            m_template.SetValue("mouthY", mouthY);
        }
    }
}