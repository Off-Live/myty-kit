using System;
using System.Collections.Generic;
using System.Linq;
using MYTYKit.Components;
using MYTYKit.MotionTemplates;
using Newtonsoft.Json.Linq;
using UnityEngine;



namespace MYTYKit.MotionAdapters
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MuscleSetting),typeof(MYTYAvatarBinder), typeof(Animator))]
    public class MYTY3DAvatarDriver : DampingAndStabilizingVec3Adapter
    {
        public PointsTemplate poseWorldPoints; // the current version supports mediapipe pose module only 
        public AnchorTemplate head;
        public ParametricTemplate blendShape;
        public MYTYAvatarBinder binder;
        public MYTYIKTarget leftHandTarget;
        public MYTYIKTarget rightHandTarget;
        
        public float visibleThreshold = 0.75f;
        public float lookAtOffset = 0.0f;

        public bool applyBodyMocap = true;
        public bool applyFaceCapture = true;
        public MuscleSetting muscleSetting;
        public List<BlendShapeSetting> blendShapeSetting;
        
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
        Transform m_humanoidAvatarRoot;
        HumanPoseHandler m_humanPoseHandler;
        HumanPose m_humanPose;
        bool m_isInitialized = false;
        Vector3 m_initialHipPosition;
        Quaternion m_initialHipRotation;
        Dictionary<string, List<BlendShapeSetting.BlendShapeItem>> m_bsCacheDict;

      
        
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
            m_initialHipPosition = hipTf.localPosition;
            m_initialHipRotation = hipTf.localRotation;
            m_isInitialized = true;

        }
        public void DeserializeFromJObject(JObject jObj)
        {
            m_bsCacheDict = new();
            visibleThreshold = (float)jObj[nameof(visibleThreshold)];
            lookAtOffset = (float)jObj[nameof(lookAtOffset)];
            isDamping = (bool)jObj[nameof(isDamping)];
            isStabilizing = (bool)jObj[nameof(isStabilizing)];
            isUseDampedInputToStabilizer = (bool)jObj[nameof(isUseDampedInputToStabilizer)];
            dampingFactor = (float)jObj[nameof(dampingFactor)];
            dampingWindow = (int)jObj[nameof(dampingWindow)];
            muscleSetting.muscleLimits = jObj["muscleSetting"].ToObject<List<MuscleSetting.MuscleLimit>>();

            m_bsCacheDict = jObj["blendShapeSetting"].ToList().ToDictionary(token => (string)token["name"],
                token => token["blendShapes"].ToObject<List<BlendShapeSetting.BlendShapeItem>>());

        }

        public void CheckAndSetupBlendShape(Transform traitRoot)
        {
            var chileSmrs = traitRoot.GetComponentsInChildren<SkinnedMeshRenderer>();
            chileSmrs.Where(smr=> m_bsCacheDict.ContainsKey(smr.name)).ToList().ForEach(smr =>
            {
                if (blendShapeSetting == null) blendShapeSetting = new();
                if (blendShapeSetting.Exists(setting => setting.mesh == smr)) return;

                var setting = gameObject.AddComponent<BlendShapeSetting>();
                setting.mesh = smr;
                setting.blendShapes = m_bsCacheDict[smr.name];
                blendShapeSetting.Add(setting);
            });
        }

        void Start()
        {
            muscleSetting = GetComponent<MuscleSetting>();
            binder = GetComponent<MYTYAvatarBinder>();
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
            if (applyBodyMocap)
            {
                var leftUpperArmVector = (m_leftElbowHint - m_leftShoulder).normalized;
                var leftUpperArmLength = (m_anim.GetBoneTransform(HumanBodyBones.LeftUpperArm).position -
                                          m_anim.GetBoneTransform(HumanBodyBones.LeftLowerArm).position).magnitude;
                var leftElbowPos = m_anim.GetBoneTransform(HumanBodyBones.LeftUpperArm).position +
                                   leftUpperArmVector * leftUpperArmLength;
                var leftLowerArmVector = (leftHandTarget.transform.localPosition - m_leftElbowHint).normalized;
                var leftLowerArmLength = (m_anim.GetBoneTransform(HumanBodyBones.LeftLowerArm).position -
                                          m_anim.GetBoneTransform(HumanBodyBones.LeftHand).position).magnitude;
                var leftHandPos = leftElbowPos + leftLowerArmVector * leftLowerArmLength;

                var rightUpperArmVector = (m_rightElbowHint - m_rightShoulder).normalized;
                var rightUpperArmLength = (m_anim.GetBoneTransform(HumanBodyBones.RightUpperArm).position -
                                           m_anim.GetBoneTransform(HumanBodyBones.RightLowerArm).position).magnitude;
                var rightElbowPos = m_anim.GetBoneTransform(HumanBodyBones.RightUpperArm).position +
                                    rightUpperArmVector * rightUpperArmLength;
                var rightLowerArmVector = (rightHandTarget.transform.localPosition - m_rightElbowHint).normalized;
                var rightLowerArmLength = (m_anim.GetBoneTransform(HumanBodyBones.RightLowerArm).position -
                                           m_anim.GetBoneTransform(HumanBodyBones.RightHand).position).magnitude;
                var rightHandPos = rightElbowPos + rightLowerArmVector * rightLowerArmLength;


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

            }

            if (applyFaceCapture)
            {
                m_anim.SetLookAtWeight(1.0f);
                m_anim.SetLookAtPosition(m_anim.GetBoneTransform(HumanBodyBones.Head).position + m_lookAt);

                ApplyBlendshape();
            }

        }

        void ApplyBlendshape()
        {
            if (blendShape == null) return;
            if (blendShapeSetting == null) return;
            var bsNames = BlendShapeSetting.GetAllBlendShapeNames();
            blendShapeSetting.Where(setting=> setting.mesh==null).ToList().ForEach(setting=> Destroy(setting));
            blendShapeSetting.RemoveAll(setting => setting.mesh==null);
            blendShapeSetting.ForEach(setting =>
            {
                bsNames.ForEach(name =>
                {
                    var val = blendShape.GetValue(name);
                    var index = setting.mesh.sharedMesh.GetBlendShapeIndex(setting.GetMappedBSName(name));
                 
                    if (index >= 0)
                    {
                        setting.mesh.SetBlendShapeWeight(index, val*100);
                    }
                });
            });
            
        }
        
        float CalculateMuscleValue(ref HumanPose pose, int muscleIdx, float angle)
        {
            var limit = 0.0f;
            if (angle > 0) limit = HumanTrait.GetMuscleDefaultMax(muscleIdx);
            else limit = HumanTrait.GetMuscleDefaultMin(muscleIdx);
            return angle / Mathf.Abs(limit);
        }
        
        void LateUpdate()
        {
            if (!m_isInitialized) return;
            m_humanPoseHandler.GetHumanPose(ref m_humanPose);
            if (applyBodyMocap)
            {
                //chest
                m_humanPose.muscles[5] = CalculateMuscleValue(ref m_humanPose, 5, m_chestTwist);
                m_humanPose.muscles[4] = CalculateMuscleValue(ref m_humanPose, 4, m_chestLR);

            }

            if (applyFaceCapture)
            {
                //head
                m_humanPose.muscles[10] = CalculateMuscleValue(ref m_humanPose, 10, m_headLR / 2);
                m_humanPose.muscles[13] = CalculateMuscleValue(ref m_humanPose, 13, m_headLR / 2);
            }

            Enumerable.Range(0,m_humanPose.muscles.Length).ToList().ForEach(idx =>
            {
                m_humanPose.muscles[idx] = Mathf.Clamp(m_humanPose.muscles[idx], -1, 1);
                if (m_humanPose.muscles[idx] < 0) m_humanPose.muscles[idx] *= muscleSetting.muscleLimits[idx].minScale;
                else m_humanPose.muscles[idx] *= muscleSetting.muscleLimits[idx].maxScale; 
            });
            
            m_humanPoseHandler.SetHumanPose(ref m_humanPose);
            if (applyBodyMocap)
            {
                var tf = m_anim.GetBoneTransform(HumanBodyBones.Hips);
                tf.localPosition = m_initialHipPosition;
                tf.localRotation = m_initialHipRotation;
            }

            if(binder!=null) binder.Apply();
            
        }

        public JObject SerializeToJObject()
        {
            
            return JObject.FromObject(new
            {
                visibleThreshold,
                lookAtOffset,
                isDamping,
                isStabilizing,
                isUseDampedInputToStabilizer,
                dampingFactor,
                dampingWindow,
                muscleSetting = muscleSetting.muscleLimits,
                blendShapeSetting = blendShapeSetting.Select( setting=> JObject.FromObject(new
                {
                    setting.mesh.name,
                    setting.blendShapes 
                }) ).ToArray()
            });
        }

        
    }
}