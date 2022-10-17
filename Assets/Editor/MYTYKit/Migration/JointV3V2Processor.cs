using MYTYKit.Controllers;
using MYTYKit.MotionAdapters;
using MYTYKit.MotionAdapters.Interpolation;
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
                var toAdapter = go.AddComponent<JointRotationMapper>();
                
                if(fromAdapter.joint != null) toAdapter.joint = mtMapper.GetTemplate(fromAdapter.joint.name) as AnchorTemplate;
                toAdapter.from = fromAdapter.from;
                
                if (fromAdapter.controller != null &&
                    (fromAdapter.controller.GetType().IsSubclassOf(typeof(SpriteController)) ||
                     fromAdapter.controller.GetType().IsSubclassOf(typeof(MSRSpriteController))))
                {
                    toAdapter.stabilizeMethod = InterpolationMethod.LinearInterpolation;
                }
                
                switch (fromAdapter.plane)
                {
                    case ProjectionPlane.XY:
                        toAdapter.configuration.Add(new JointRotationMapper.MapItem()
                        {
                            isInverted = fromAdapter.flip? !fromAdapter.negate2nd : !fromAdapter.negate1st,
                            sourceComponent = ComponentIndex.X,
                            targetComponent = fromAdapter.flip? ComponentIndex.Y: ComponentIndex.X,
                            targetController = fromAdapter.controller
                        });
                        toAdapter.configuration.Add(new JointRotationMapper.MapItem()
                        {
                            isInverted = fromAdapter.flip? fromAdapter.negate1st : fromAdapter.negate2nd,
                            sourceComponent = ComponentIndex.Y,
                            targetComponent = fromAdapter.flip? ComponentIndex.X: ComponentIndex.Y,
                            targetController = fromAdapter.controller
                        });
                    
                        break;
                    case ProjectionPlane.XZ:
                        toAdapter.configuration.Add(new JointRotationMapper.MapItem()
                        {
                            isInverted = fromAdapter.flip? !fromAdapter.negate2nd : !fromAdapter.negate1st,
                            sourceComponent = ComponentIndex.X,
                            targetComponent = fromAdapter.flip? ComponentIndex.Y: ComponentIndex.X,
                            targetController = fromAdapter.controller
                        });
                        toAdapter.configuration.Add(new JointRotationMapper.MapItem()
                        {
                            isInverted = fromAdapter.flip? fromAdapter.negate1st : fromAdapter.negate2nd,
                            sourceComponent = ComponentIndex.Z,
                            targetComponent = fromAdapter.flip? ComponentIndex.X: ComponentIndex.Y,
                            targetController = fromAdapter.controller
                        });
                        break;
                    case ProjectionPlane.YZ:
                        toAdapter.configuration.Add(new JointRotationMapper.MapItem()
                        {
                            isInverted = fromAdapter.flip? fromAdapter.negate2nd : fromAdapter.negate1st,
                            sourceComponent = ComponentIndex.Y,
                            targetComponent = fromAdapter.flip? ComponentIndex.Y: ComponentIndex.X,
                            targetController = fromAdapter.controller
                        });
                        toAdapter.configuration.Add(new JointRotationMapper.MapItem()
                        {
                            isInverted = fromAdapter.flip? fromAdapter.negate1st : fromAdapter.negate2nd,
                            sourceComponent = ComponentIndex.Z,
                            targetComponent = fromAdapter.flip? ComponentIndex.X: ComponentIndex.Y,
                            targetController = fromAdapter.controller
                        });
                        break;
                }

                if (fromAdapter.flip)
                {
                    toAdapter.configuration[0].min = fromAdapter.min2nd;
                    toAdapter.configuration[0].max = fromAdapter.max2nd;
                    toAdapter.configuration[1].min = fromAdapter.min1st;
                    toAdapter.configuration[1].max = fromAdapter.max1st;
                }
                else
                {
                    toAdapter.configuration[0].min = fromAdapter.min1st;
                    toAdapter.configuration[0].max = fromAdapter.max1st;
                    toAdapter.configuration[1].min = fromAdapter.min2nd;
                    toAdapter.configuration[1].max = fromAdapter.max2nd;
                }
                
                EditorUtility.SetDirty(toAdapter);
                
                Object.DestroyImmediate(fromAdapter);
            }
        }
    }
}