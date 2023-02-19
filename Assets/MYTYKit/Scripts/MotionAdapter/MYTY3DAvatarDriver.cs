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
        
        public float visibleThreshold = 0.75f;
        public float lookAtOffset = 0.0f;
        
        public bool isStartAuto = false;
        public MuscleSetting muscleSetting;

        public Transform leftHandTf;
        public Transform rightHandTf;
        public Transform leftElbowTf;
        public Transform rightElbowTf;
        public Transform leftShoulderTf;
        public Transform rightShoulderTf;
    
        public Transform humanoidAvatarRoot
        {
            set => m_humanoidAvatarRoot = value;
        }
        
        KalmanFilterVec3[] m_filters;
        
        
        public float m_chestTwist = 0.0f;
        public float m_chestLR = 0.0f;
        float m_headLR = 0.0f;
        Vector3 m_lookAt;
        Vector3 m_headUp;
        Vector3 m_leftShoulder;
        Vector3 m_rightShoulder;
        Vector3 m_leftElbowHint;
        Vector3 m_rightElbowHint;

        Animator m_anim;

        Transform m_humanoidAvatarRoot;
        HumanPoseHandler m_humanPoseHandler;
        HumanPose m_humanPose;

        bool m_isInitialized = false;

        Vector3 m_initialHipPosition;
        Quaternion m_initialHipRotation;

        MYTYAvatarBinder binder;
       
        void Start()
        {
            if(isStartAuto) Initialize();
            binder = GetComponent<MYTYAvatarBinder>();
        }
        
        
        public void Initialize()
        {
            base.Start();
            SetNumInterpolationSlot(35); //33 for pose, 2 for head
            head.SetUpdateCallback(UpdateHead);
            poseWorldPoints.SetUpdateCallback(UpdatePose);

            m_filters = new KalmanFilterVec3[35];
            Enumerable.Range(0,35).ToList().ForEach(idx => m_filters[idx] = new KalmanFilterVec3());

            m_anim = GetComponent<Animator>();
            
            m_humanPoseHandler = new HumanPoseHandler(m_anim.avatar, m_humanoidAvatarRoot);
            m_humanPose = new HumanPose();
            
            var hipTf = m_anim.GetBoneTransform(HumanBodyBones.Hips);
            m_initialHipPosition = hipTf.position;
            m_initialHipRotation = hipTf.rotation;
            m_isInitialized = true;

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
            if (!m_isInitialized) return;
            leftHandTarget.transform.localPosition = GetResult(15);
            rightHandTarget.transform.localPosition = GetResult(16);

            m_leftShoulder = GetResult(11);
            m_rightShoulder = GetResult(12);

            var shoulderLR = (m_leftShoulder - m_rightShoulder).normalized;
            var shoulderLRTwist = (shoulderLR - Vector3.up * Vector3.Dot(shoulderLR, Vector3.up)).normalized;
        
            m_chestLR = Mathf.Rad2Deg * Mathf.Asin(Vector3.Cross(shoulderLRTwist, shoulderLR).magnitude) *
                     Mathf.Sign(m_leftShoulder.y - m_rightShoulder.y);
            m_chestTwist = -Mathf.Rad2Deg * Mathf.Asin(Vector3.Cross(shoulderLRTwist, Vector3.left).y);
        
            m_leftElbowHint = GetResult(13);
            m_rightElbowHint = GetResult(14);

            var lookAtVector = GetResult(33).normalized;
        
            m_lookAt=  lookAtVector + Vector3.up*lookAtOffset;
            m_headUp = GetResult(34).normalized;

            var headLRPlaneVector = m_headUp - lookAtVector * Vector3.Dot(m_headUp, lookAtVector);
            m_headLR = Mathf.Rad2Deg *
                     Mathf.Asin(Vector3.Dot(Vector3.Cross(headLRPlaneVector, Vector3.up), lookAtVector));
        }

        void OnAnimatorIK(int layerIndex)
        {
            if (!m_isInitialized) return;
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
            
        }
        
        void LateUpdate()
        {
            if (!m_isInitialized) return;
            m_humanPoseHandler.GetHumanPose(ref m_humanPose);
            
            
            Enumerable.Range(0,m_humanPose.muscles.Length).ToList().ForEach(idx =>
            {
                if (m_humanPose.muscles[idx] < 0) m_humanPose.muscles[idx] *= muscleSetting.muscleLimits[idx].minScale;
                else m_humanPose.muscles[idx] *= muscleSetting.muscleLimits[idx].maxScale; 
            });

            //if(m_humanPose.muscles[39]>0) m_humanPose.muscles[37] = m_humanPose.muscles[39];
            // 1 2   4 5  7 8  
            var twistLimit = 0.0f;
            if (m_chestTwist > 0)
            {
                twistLimit = HumanTrait.GetMuscleDefaultMax(5) * muscleSetting.muscleLimits[5].maxScale;
            }
            else
            {
                twistLimit = HumanTrait.GetMuscleDefaultMin(5) * muscleSetting.muscleLimits[5].minScale;
            }

            m_humanPose.muscles[5] = m_chestTwist/Mathf.Abs(twistLimit);

            var lrLimit = 0.0f;
            if (m_chestLR > 0) lrLimit = HumanTrait.GetMuscleDefaultMax(4) * muscleSetting.muscleLimits[4].maxScale;
            else lrLimit = HumanTrait.GetMuscleDefaultMin(4) * muscleSetting.muscleLimits[4].minScale;
            m_humanPose.muscles[4] = m_chestLR/Mathf.Abs(lrLimit);
            
            //10 13

            var neckLimit= 0.0f;
            var headLimit = 0.0f;
            if (m_headLR > 0)
            {
                neckLimit = HumanTrait.GetMuscleDefaultMax(10) * muscleSetting.muscleLimits[10].maxScale;
                headLimit = HumanTrait.GetMuscleDefaultMax(13) * muscleSetting.muscleLimits[13].maxScale;
            }
            else
            {
                neckLimit = HumanTrait.GetMuscleDefaultMin(10) * muscleSetting.muscleLimits[10].minScale;
                headLimit = HumanTrait.GetMuscleDefaultMin(13) * muscleSetting.muscleLimits[13].minScale;
            }

            m_humanPose.muscles[10] = m_headLR / Mathf.Abs(neckLimit) / 2;
            m_humanPose.muscles[13] = m_headLR / Mathf.Abs(headLimit) / 2;

            
            Enumerable.Range(0,m_humanPose.muscles.Length).ToList().ForEach(idx =>
            {
                m_humanPose.muscles[idx] = Mathf.Clamp(m_humanPose.muscles[idx], -1, 1);
            });

            m_humanPoseHandler.SetHumanPose(ref m_humanPose);
            var tf = m_anim.GetBoneTransform(HumanBodyBones.Hips);
            tf.position = m_initialHipPosition;
            tf.rotation = m_initialHipRotation;
            if(binder!=null) binder.Apply();
            
        }
        
    }
}