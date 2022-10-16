using MYTYKit.Controllers;
using MYTYKit.MotionAdapters;
using MYTYKit.MotionAdapters.Interpolation;
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
                var toAdapter = go.AddComponent<JointRotationMapper>();
                
                if(fromAdapter.joint != null) toAdapter.joint = mtMapper.GetTemplate(fromAdapter.joint.name) as AnchorTemplate;
                toAdapter.from = fromAdapter.from;
                
                if (fromAdapter.controller != null &&
                    (fromAdapter.controller.GetType().IsSubclassOf(typeof(SpriteController)) ||
                     fromAdapter.controller.GetType().IsSubclassOf(typeof(MSRSpriteController))))
                {
                    toAdapter.stabilizeMethod = InterpolationMethod.LinearInterpolation;
                }
                
                toAdapter.configuration.Add(new JointRotationMapper.MapItem()
                {
                    isInverted = fromAdapter.component== ComponentIndex.X ? !fromAdapter.negate : fromAdapter.negate,
                    sourceComponent = fromAdapter.component,
                    targetComponent = ComponentIndex.X,
                    targetController = fromAdapter.controller
                });

                EditorUtility.SetDirty(toAdapter);
                
                Object.DestroyImmediate(fromAdapter);
            }
        }
    }
}