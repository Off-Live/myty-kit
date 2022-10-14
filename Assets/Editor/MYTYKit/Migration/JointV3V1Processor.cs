using MYTYKit.MotionAdapters;
using MYTYKit.MotionTemplates;
using UnityEngine;
using UnityEditor;

namespace MYTYKit
{
    public static class JointV3V1Processor
    {
        public static void MigrateJointV3ToV1Adapter(this Migration migration)
        {
            var adapters = Object.FindObjectsOfType<JointVec3ToVec1Adapter>();
            var mtMapper = Object.FindObjectOfType<MotionTemplateMapper>();
            foreach (var target in adapters)
            {
                var go = target.gameObject;
                var fromAdapter = go.GetComponent<JointVec3ToVec1Adapter>();
                var toAdapter = go.AddComponent<JointVec3ToVec1AdapterV2>();

                toAdapter.stabilizeWindow = fromAdapter.stabilizeWindow;
                toAdapter.smoothWindow = fromAdapter.smoothWindow;
                if(fromAdapter.joint != null) toAdapter.joint = mtMapper.GetTemplate(fromAdapter.joint.name) as AnchorTemplate;
                toAdapter.from = fromAdapter.from;
                toAdapter.component = fromAdapter.component;
                toAdapter.negate = fromAdapter.negate;
                if (toAdapter.component == ComponentIndex.X)
                {
                    toAdapter.negate = !fromAdapter.negate;
                }
                toAdapter.controller = fromAdapter.controller;
                toAdapter.stabilizeTime = fromAdapter.stabilizeTime;
                EditorUtility.SetDirty(toAdapter);
                
                Object.DestroyImmediate(fromAdapter);
            }
        }
    }
}