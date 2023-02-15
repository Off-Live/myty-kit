using System;
using System.Collections.Generic;
using System.Linq;
using MYTYKit.Components;
using MYTYKit.MotionTemplates;
using UnityEngine;



namespace MYTYKit.MotionAdapters
{
    public class MYTY3DAvatarDriver : DampingAndStabilizingVec3Adapter
    {
        public PointsTemplate poseWorldPoints; // the current version supports mediapipe pose module only 
        public AnchorTemplate head;

        public MYTYIKTarget leftHandTarget;
        public MYTYIKTarget rightHandTarget;

        public Transform humanoidAvatarRoot;
        
        public float visibleThreshold = 0.75f;
        public float lookAtOffset = 0.0f;
        
        
        public Transform leftHandTf;
        public Transform rightHandTf;
        public Transform leftElbowTf;
        public Transform rightElbowTf;
        public Transform leftShoulderTf;
        public Transform rightShoulderTf;
        public MuscleSetting muscleSetting;
        
        KalmanFilterVec3[] m_filters;
        
        
        float m_chestTwist = 0.0f;
        float m_chestLR = 0.0f;
        float m_headLR = 0.0f;
        Vector3 m_lookAt;
        Vector3 m_headUp;
        Vector3 m_leftShoulder;
        Vector3 m_rightShoulder;
        Vector3 m_leftElbowHint;
        Vector3 m_rightElbowHint;

        Animator m_anim;

        HumanPoseHandler m_humanPoseHandler;
        HumanPose m_humanPose;

        protected override void Start()
        {
            base.Start();
            SetNumInterpolationSlot(35); //33 for pose, 2 for head
            head.SetUpdateCallback(UpdateHead);
            poseWorldPoints.SetUpdateCallback(UpdatePose);

            m_filters = new KalmanFilterVec3[35];
            Enumerable.Range(0,35).ToList().ForEach(idx => m_filters[idx] = new KalmanFilterVec3());

            m_anim = GetComponent<Animator>();
            if(m_anim==null) CreateAnimator();

            m_humanPoseHandler = new HumanPoseHandler(m_anim.avatar, humanoidAvatarRoot);
            m_humanPose = new HumanPose();
            
        }

        void CreateAnimator()
        {
            
        }
        
        void UpdateHead()
        {
            AddToHistory(m_filters[33].Update(head.lookAt),33);
            AddToHistory(m_filters[34].Update(head.up),34);
        }

        void UpdatePose()
        {
            var pointsBuf = new List<Vector3>(poseWorldPoints.points); 
            pointsBuf = pointsBuf.Select(point => new Vector3(point.x, point.y, -point.z)).ToList();
            Enumerable.Range(0, pointsBuf.Count).ToList().ForEach(idx =>
            {
                if (poseWorldPoints.visibilities[idx] > visibleThreshold)
                {
                
                    //AddToHistory(pointsBuf[idx], idx);
                    AddToHistory(m_filters[idx].Update(pointsBuf[idx]), idx);
                }

                leftHandTarget.Visible = poseWorldPoints.visibilities[15] > visibleThreshold;
                rightHandTarget.Visible = poseWorldPoints.visibilities[16] > visibleThreshold;
            });
        }
        
        void FixedUpdate()
        {
            leftHandTarget.transform.localPosition = GetResult(15);
            rightHandTarget.transform.localPosition = GetResult(16);

            m_leftShoulder = GetResult(11);
            m_rightShoulder = GetResult(12);

            var shoulderLR = (m_leftShoulder - m_rightShoulder).normalized;
            var shoulderLRTwist = (shoulderLR - Vector3.up * Vector3.Dot(shoulderLR, Vector3.up)).normalized;
        
            m_chestLR =- Mathf.Rad2Deg * Mathf.Asin(Vector3.Cross(shoulderLRTwist, shoulderLR).magnitude) *
                     Mathf.Sign(m_leftShoulder.y - m_rightShoulder.y);
            m_chestTwist = -Mathf.Rad2Deg * Mathf.Asin(Vector3.Cross(shoulderLRTwist, Vector3.left).y);
        
            m_leftElbowHint = GetResult(13);
            m_rightElbowHint = GetResult(14);

            var lookAtVector = GetResult(33).normalized;
        
            m_lookAt=  lookAtVector + Vector3.up*lookAtOffset;
            m_headUp = GetResult(34).normalized;

            var headLRPlaneVector = m_headUp - lookAtVector * Vector3.Dot(m_headUp, lookAtVector);
            m_headLR = - Mathf.Rad2Deg *
                     Mathf.Asin(Vector3.Dot(Vector3.Cross(headLRPlaneVector, Vector3.up), lookAtVector));
        }

        void OnAnimatorIK(int layerIndex)
        {
            var leftUpperArmVector = (m_leftElbowHint - m_leftShoulder).normalized;
            var leftUpperArmLength = (m_anim.GetBoneTransform(HumanBodyBones.LeftUpperArm).position -
                                  m_anim.GetBoneTransform(HumanBodyBones.LeftLowerArm).position).magnitude;
            var leftElbowPos = m_anim.GetBoneTransform(HumanBodyBones.LeftUpperArm).position +
                                       leftUpperArmVector * leftUpperArmLength;
            var leftLowerArmVector = (leftHandTarget.transform.localPosition - m_leftElbowHint).normalized;
            var leftLowerArmLength = (m_anim.GetBoneTransform(HumanBodyBones.LeftLowerArm).position -
                                  m_anim.GetBoneTransform(HumanBodyBones.LeftHand).position).magnitude;
            var leftHandPos =  leftElbowPos + leftLowerArmVector * leftLowerArmLength;
            
            var rightUpperArmVector = (m_rightElbowHint - m_rightShoulder).normalized;
            var rightUpperArmLength = (m_anim.GetBoneTransform(HumanBodyBones.RightUpperArm).position -
                                      m_anim.GetBoneTransform(HumanBodyBones.RightLowerArm).position).magnitude;
            var rightElbowPos = m_anim.GetBoneTransform(HumanBodyBones.RightUpperArm).position +
                               rightUpperArmVector * rightUpperArmLength;
            var rightLowerArmVector = (rightHandTarget.transform.localPosition - m_rightElbowHint).normalized;
            var rightLowerArmLength = (m_anim.GetBoneTransform(HumanBodyBones.RightLowerArm).position -
                                      m_anim.GetBoneTransform(HumanBodyBones.RightHand).position).magnitude;
            var rightHandPos =  rightElbowPos+ rightLowerArmVector * rightLowerArmLength;


            m_anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandTarget.weight);
            //m_anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, weight);
           
            m_anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPos);
            m_anim.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbowPos);
            m_anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, leftHandTarget.weight);
            
            //m_anim.SetIKRotation(AvatarIKGoal.LeftHand, lhIkTarget.rotation);
            
            m_anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandTarget.weight);
            //m_anim.SetIKRotationWeight(AvatarIKGoal.RightHand, weight);
            
            m_anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandPos);
            m_anim.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbowPos);
            m_anim.SetIKHintPositionWeight(AvatarIKHint.RightElbow, rightHandTarget.weight);
            //m_anim.SetIKRotation(AvatarIKGoal.LeftHand, lhIkTarget.rotation);

            m_anim.SetLookAtWeight(1.0f);
            
            m_anim.SetLookAtPosition(m_anim.GetBoneTransform(HumanBodyBones.Head).position+m_lookAt);
            m_anim.SetBoneLocalRotation(HumanBodyBones.Chest, Quaternion.AngleAxis(m_chestTwist, Vector3.up)* Quaternion.AngleAxis(m_chestLR,Vector3.forward) );
            m_anim.SetBoneLocalRotation(HumanBodyBones.Head, Quaternion.AngleAxis(m_headLR/2,Vector3.forward));
            m_anim.SetBoneLocalRotation(HumanBodyBones.Neck, Quaternion.AngleAxis(m_headLR/2,Vector3.forward));
        }
        
        void LateUpdate()
        {
            m_humanPoseHandler.GetHumanPose(ref m_humanPose);
            
            
            Enumerable.Range(0,m_humanPose.muscles.Length).ToList().ForEach(idx => 
                Mathf.Clamp(m_humanPose.muscles[idx], muscleSetting.muscleLimits[idx].min, muscleSetting.muscleLimits[idx].max));

            
            //m_humanPose.muscles[37] = m_humanPose.muscles[39]*0.5f;
            m_humanPoseHandler.SetHumanPose(ref m_humanPose);
        }
        
    }
}