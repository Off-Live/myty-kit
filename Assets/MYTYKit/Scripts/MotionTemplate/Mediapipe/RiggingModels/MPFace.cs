using MYTYKit.ThirdParty.KalidoKit;
using UnityEngine;

namespace MYTYKit.MotionTemplates.Mediapipe.Model
{
    public class MPFace : MPBaseModel
    {
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
       
        public override void UpdateTemplate()
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
                
            }
            
        }
    }
}