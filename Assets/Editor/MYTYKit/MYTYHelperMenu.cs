using MYTYKit.Components;
using MYTYKit.MotionAdapters;
using MYTYKit.MotionTemplates;
using UnityEditor;
using UnityEngine;

namespace MYTYKit
{
    public class MYTYHelperMenu
    {
        const string MotionTemplatePath = "Assets/MYTYKit/MotionTemplate/DefaultMotionTemplate.prefab";
        const string MotionSourcePath = "Assets/MYTYKit/MotionTemplate/Motion Source Samples/MediapipeMotionPack.prefab";
        const string AnimationControllerPath = "Assets/MYTYKit/Animation/DefaultAnimationController.controller"; 
        [MenuItem("MYTY Kit/Create Mocap Object(Mediapipe)", false, 41)]
        public static void CreateMediapipeObjects()
        {
            var mtGo = PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    MotionTemplatePath)) as GameObject;
            var msGo = PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    MotionSourcePath)) as GameObject;

            var mapper = mtGo.GetComponent<MotionTemplateMapper>();
            var motionSource = msGo.GetComponentInChildren<MotionSource>();
            motionSource.motionTemplateMapperList = new() { mapper };
        }

        [MenuItem("MYTY Kit/Setup 3D Avatar Exporter to Scene", false, 41)]
        public static void CreateAvatarExporterPipeline()
        {
            var ikRoot = new GameObject("IKTargets");
            var lhTargetGo = new GameObject("LH");
            lhTargetGo.transform.parent = ikRoot.transform;
            var lhTarget = lhTargetGo.AddComponent<MYTYIKTarget>();
            var rhTargetGo = new GameObject("RH");
            rhTargetGo.transform.parent = ikRoot.transform;
            var rhTarget = rhTargetGo.AddComponent<MYTYIKTarget>();
            
            
            var mtGo = PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    MotionTemplatePath)) as GameObject;
            var msGo = PrefabUtility.InstantiatePrefab(
                AssetDatabase.LoadAssetAtPath<GameObject>(
                    MotionSourcePath)) as GameObject;

            var mapper = mtGo.GetComponent<MotionTemplateMapper>();
            var motionSource = msGo.GetComponentInChildren<MotionSource>();
            motionSource.motionTemplateMapperList = new() { mapper };

            var avatarBuilderGo = new GameObject("AvatarBuilder");
            var avatarBuilder = avatarBuilderGo.AddComponent<HumanoidAvatarBuilder>();

            var avatarDescRootGo = new GameObject("Put all 3D models here");
            var avatarDesc = avatarDescRootGo.AddComponent<MYTYAvatarDesc>();
            var animator = avatarDescRootGo.AddComponent<Animator>();
            avatarDescRootGo.AddComponent<MYTYAvatarBinder>();
            var muscleSetting = avatarDescRootGo.AddComponent<MuscleSetting>();
            var driver = avatarDescRootGo.AddComponent<MYTY3DAvatarDriver>();

            avatarDesc.avatarBuilder = avatarBuilder;

            animator.runtimeAnimatorController =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AnimationControllerPath);
            
            driver.muscleSetting = muscleSetting;
            driver.leftHandTarget = lhTarget;
            driver.rightHandTarget = rhTarget;
            
            driver.head = mapper.GetTemplate("Head") as AnchorTemplate;
            driver.blendShape = mapper.GetTemplate("BlendShape") as ParametricTemplate;
            driver.poseWorldPoints = mapper.GetTemplate("BodyPoints") as PointsTemplate;
            
        }
        
    }
    
    
}