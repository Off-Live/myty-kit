using System.Collections.Generic;
using MYTYKit.MotionAdapters;
using MYTYKit.MotionAdapters.Reduce;
using MYTYKit.MotionTemplates;
using UnityEditor;
using UnityEngine;

namespace MYTYKit
{
    public static class TmpMig
    {
        public static void MigrateJointV3ToV2AdapterV2(this Migration migration)
        {
            var adapters = Object.FindObjectsOfType<JointVec3ToVec2AdapterV2>();
            var mtMapper = Object.FindObjectOfType<MotionTemplateMapper>();
            foreach (var target in adapters)
            {
                var go = target.gameObject;
                var fromAdapter = go.GetComponent<JointVec3ToVec2AdapterV2>();
                var toAdapter = go.AddComponent<JointRotationMapper>();
                
                if(fromAdapter.joint != null) toAdapter.joint = mtMapper.GetTemplate(fromAdapter.joint.name) as AnchorTemplate;
                toAdapter.from = fromAdapter.from;
                toAdapter.isDamping = fromAdapter.isDamping;
                toAdapter.isStabilizing = fromAdapter.isStabilizing;
                toAdapter.isUseDampedInputToStabilizer = fromAdapter.isUseDampedInputToStabilizer;
                toAdapter.damplingFactor = fromAdapter.damplingFactor;
                toAdapter.dampingWindow = fromAdapter.dampingWindow;
                toAdapter.stabilizeMethod = fromAdapter.stabilizeMethod; 
                
                switch (fromAdapter.plane)
                {
                    case ProjectionPlane.XY:
                        toAdapter.configuration.Add(new JointRotationMapper.MapItem()
                        {
                            isInverted = false,
                            sourceComponent = ComponentIndex.X,
                            targetComponent = fromAdapter.flip? ComponentIndex.Y: ComponentIndex.X,
                            targetController = fromAdapter.controller
                        });
                        toAdapter.configuration.Add(new JointRotationMapper.MapItem()
                        {
                            isInverted = false,
                            sourceComponent = ComponentIndex.Y,
                            targetComponent = fromAdapter.flip? ComponentIndex.X: ComponentIndex.Y,
                            targetController = fromAdapter.controller
                        });
                    
                        break;
                    case ProjectionPlane.XZ:
                        toAdapter.configuration.Add(new JointRotationMapper.MapItem()
                        {
                            isInverted = false,
                            sourceComponent = ComponentIndex.X,
                            targetComponent = fromAdapter.flip? ComponentIndex.Y: ComponentIndex.X,
                            targetController = fromAdapter.controller
                        });
                        toAdapter.configuration.Add(new JointRotationMapper.MapItem()
                        {
                            isInverted = false,
                            sourceComponent = ComponentIndex.Z,
                            targetComponent = fromAdapter.flip? ComponentIndex.X: ComponentIndex.Y,
                            targetController = fromAdapter.controller
                        });
                        break;
                    case ProjectionPlane.YZ:
                        toAdapter.configuration.Add(new JointRotationMapper.MapItem()
                        {
                            isInverted = false,
                            sourceComponent = ComponentIndex.Y,
                            targetComponent = fromAdapter.flip? ComponentIndex.Y: ComponentIndex.X,
                            targetController = fromAdapter.controller
                        });
                        toAdapter.configuration.Add(new JointRotationMapper.MapItem()
                        {
                            isInverted = false,
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

        public static void MigrateParametric1D(this Migration migration)
        {
            var adapters = Object.FindObjectsOfType<Parametric1DAdapter>();
            var mtMapper = Object.FindObjectOfType<MotionTemplateMapper>();
            foreach (var target in adapters)
            {
                var go = target.gameObject;
                var fromAdapter = go.GetComponent<Parametric1DAdapter>();
                var toAdapter = go.AddComponent<ParametricReducer>();
                var reducer = go.AddComponent<LinearCombination>();

                toAdapter.isDamping = fromAdapter.isDamping;
                toAdapter.isStabilizing = fromAdapter.isStabilizing;
                toAdapter.isUseDampedInputToStabilizer = fromAdapter.isUseDampedInputToStabilizer;
                toAdapter.damplingFactor = fromAdapter.damplingFactor;
                toAdapter.dampingWindow = fromAdapter.dampingWindow;
                toAdapter.stabilizeMethod = fromAdapter.stabilizeMethod;

                toAdapter.template = fromAdapter.template;
                toAdapter.configuration.Add(new ParametricReducer.ReduceItem()
                {
                    reducer = reducer,
                    paramNames = new List<string>() { fromAdapter.paramName },
                    controller = fromAdapter.con,
                    component = ComponentIndex.X
                });

                reducer.weights = new List<float>() { 1.0f };

                EditorUtility.SetDirty(toAdapter);
                Object.DestroyImmediate(fromAdapter);
            }
        }
        public static void MigrateParametric2D(this Migration migration)
        {
            var adapters = Object.FindObjectsOfType<Parametric2DAdapter>();
            var mtMapper = Object.FindObjectOfType<MotionTemplateMapper>();
            foreach (var target in adapters)
            {
                var go = target.gameObject;
                var fromAdapter = go.GetComponent<Parametric2DAdapter>();
                var toAdapter = go.AddComponent<ParametricReducer>();
                var reducer = go.AddComponent<LinearCombination>();

                toAdapter.isDamping = fromAdapter.isDamping;
                toAdapter.isStabilizing = fromAdapter.isStabilizing;
                toAdapter.isUseDampedInputToStabilizer = fromAdapter.isUseDampedInputToStabilizer;
                toAdapter.damplingFactor = fromAdapter.damplingFactor;
                toAdapter.dampingWindow = fromAdapter.dampingWindow;
                toAdapter.stabilizeMethod = fromAdapter.stabilizeMethod;

                toAdapter.template = fromAdapter.template;
                toAdapter.configuration.Add(new ParametricReducer.ReduceItem()
                {
                    reducer = reducer,
                    paramNames = new List<string>() { fromAdapter.xParamName },
                    controller = fromAdapter.con,
                    component = ComponentIndex.X
                });
                toAdapter.configuration.Add(new ParametricReducer.ReduceItem()
                {
                    reducer = reducer,
                    paramNames = new List<string>() { fromAdapter.yParamName },
                    controller = fromAdapter.con,
                    component = ComponentIndex.Y
                });

                reducer.weights = new List<float>() { 1.0f };

                EditorUtility.SetDirty(toAdapter);
                Object.DestroyImmediate(fromAdapter);
            }
        }

    }
}