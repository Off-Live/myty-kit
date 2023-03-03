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

        const string MotionSourcePath =
            "Assets/MYTYKit/MotionTemplate/Motion Source Samples/MediapipeMotionPack.prefab";
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

    }
}