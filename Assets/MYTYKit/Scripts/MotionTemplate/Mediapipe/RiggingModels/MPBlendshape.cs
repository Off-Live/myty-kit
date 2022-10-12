using System;
using System.Collections.Generic;
using System.Linq;
using MYTYKit.ThirdParty.MeFaMo;
using UnityEngine;

namespace MYTYKit.MotionTemplates.Mediapipe.Model
{
    [Serializable]
    internal class BSItem
    {
        public string name;
        public float value;
    }
    public class MPBlendshape : MPSolverModel
    {   [SerializeField] List<BSItem> blendShape = new();
        void LateUpdate()
        {
            if (m_solver == null) return;
            blendShape.Clear();
            foreach (var key in Enum.GetValues(typeof(MeFaMoConfig.FaceBlendShape)))
            {
                var keyString = key + "";
                var keyEnum = (MeFaMoConfig.FaceBlendShape)key;
                keyString = char.ToLower(keyString.First()) + keyString.Substring(1);
                if (m_solver.blendShape.ContainsKey(keyEnum))
                {
                    blendShape.Add(new BSItem
                    {
                        name = keyString,
                        value = m_solver.blendShape[keyEnum]
                    });
                }
            }
            UpdateTemplate();
            m_solver = null;

        }
       
        public override void UpdateTemplate()
        {
            if (templateList.Count == 0) return;

            foreach (var motionTemplate in templateList)
            {
                var template = (ParametricTemplate)motionTemplate;
               
                // template.SetValue("leftEye",leftEye);
                // template.SetValue("rightEye",rightEye);
                // template.SetValue("leftEyeBrow", leftEyeBrow);
                // template.SetValue("rightEyeBrow", rightEyeBrow);
                // template.SetValue("leftPupilX", leftPupil.x);
                // template.SetValue("leftPupilY", leftPupil.y);
                // template.SetValue("rightPupilX", rightPupil.x);
                // template.SetValue("rightPupilY", rightPupil.y);
                // template.SetValue("mouthX", mouthX);
                // template.SetValue("mouthY", mouthY);
                //
            }
            
        }
    }
}