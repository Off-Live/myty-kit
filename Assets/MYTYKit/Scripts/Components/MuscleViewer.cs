using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MYTYKit.Components
{
    public class MuscleViewer : MonoBehaviour
    {
        [Serializable]
        public class Muscle
        {
            public string name;
            public float value;
        }

        public Avatar avatar;
        public Transform avatarRoot;
        
        public float value;
        public int id;
        public string muscleName;
        public List<Muscle> muscles;
        
        HumanPoseHandler m_humanPoseHandler;
        HumanPose m_humanPose;

        void Start()
        {
            muscles = HumanTrait.MuscleName.ToList().Select(name => new Muscle() { name = name }).ToList();
            m_humanPoseHandler = new HumanPoseHandler(avatar, avatarRoot);
            m_humanPose = new HumanPose();
         
        }


        void LateUpdate()
        {
          
            m_humanPoseHandler.GetHumanPose(ref m_humanPose);
            Enumerable.Range(0, m_humanPose.muscles.Length).ToList().ForEach(idx =>
            {
                //m_humanPose.muscles[id] = value;
                muscleName = muscles[id].name; 
                muscles[idx].value = m_humanPose.muscles[idx];
                
            });
            
            //m_humanPoseHandler.SetHumanPose(ref m_humanPose);
            // var tf = m_anim.GetBoneTransform(HumanBodyBones.Hips);
            // rootpos = m_anim.GetBoneTransform(HumanBodyBones.Hips).position;
            // rootrot = m_anim.GetBoneTransform(HumanBodyBones.Hips).rotation;
            // m_posDiff = rootpos- m_initialHipPosition;
            // m_rotDiff = m_initialHipRotation * rootrot.GetConjugate() ;
            // tf.rotation = m_rotDiff * rootrot;
            // tf.position = m_initialHipPosition;
        }
    }
}