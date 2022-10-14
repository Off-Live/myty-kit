using MYTYKit.MotionAdapters;
using MYTYKit.MotionTemplates;
using UnityEditor;
using UnityEngine;

namespace MYTYKit
{
    public static class JointV3V2Processor
    {
        public static void MigrateJointV3ToV2Adapter(this Migration migration)
        {
            var adapters = Object.FindObjectsOfType<JointVec3ToVec2Adapter>();
            var mtMapper = Object.FindObjectOfType<MotionTemplateMapper>();
            foreach (var target in adapters)
            {
                var go = target.gameObject;
                var fromAdapter = go.GetComponent<JointVec3ToVec2Adapter>();
                var toAdapter = go.AddComponent<JointVec3ToVec2AdapterV2>();

                toAdapter.stabilizeWindow = fromAdapter.stabilizeWindow;
                toAdapter.smoothWindow = fromAdapter.smoothWindow;
                if(fromAdapter.joint != null) toAdapter.joint = mtMapper.GetTemplate(fromAdapter.joint.name) as AnchorTemplate;
                toAdapter.from = fromAdapter.from;
                toAdapter.plane = fromAdapter.plane;
                toAdapter.flip = fromAdapter.flip;
                toAdapter.controller = fromAdapter.controller;
                toAdapter.stabilizeTime = fromAdapter.stabilizeTime;
                toAdapter.negate1st = fromAdapter.negate1st;
                toAdapter.negate2nd = fromAdapter.negate2nd;
                if (toAdapter.plane == ProjectionPlane.XY || toAdapter.plane == ProjectionPlane.XZ)
                {
                    if (toAdapter.flip)
                    {
                        toAdapter.negate2nd = !fromAdapter.negate2nd;
                    }
                    else
                    {
                        toAdapter.negate1st = !fromAdapter.negate1st;
                    }
                }
               
                toAdapter.min1st = fromAdapter.min1st;
                toAdapter.max1st = fromAdapter.max1st;
                toAdapter.min2nd = fromAdapter.min2nd;
                toAdapter.max2nd = fromAdapter.max2nd;
                EditorUtility.SetDirty(toAdapter);
                
                Object.DestroyImmediate(fromAdapter);
            }
        }
    }
}