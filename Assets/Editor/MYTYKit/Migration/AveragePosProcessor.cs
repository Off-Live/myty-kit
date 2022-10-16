using System.Collections.Generic;
using MYTYKit.Controllers;
using MYTYKit.MotionAdapters;
using MYTYKit.MotionAdapters.Interpolation;
using MYTYKit.MotionAdapters.Reduce;
using MYTYKit.MotionTemplates;
using UnityEditor;
using UnityEngine;

namespace MYTYKit
{
    public static class AveragePosProcessor
    {
        public static void MigrateAveragePosAdapter(this Migration migration)
        {
            var adapters = Object.FindObjectsOfType<AveragePosFromPointsAdapter>();
            var mtMapper = Object.FindObjectOfType<MotionTemplateMapper>();
            foreach (var target in adapters)
            {
                var go = target.gameObject;
                var fromAdapter = go.GetComponent<AveragePosFromPointsAdapter>();
                var toAdapter = go.AddComponent<PointsReducer>();
                var reducer = go.AddComponent<LinearCombination>();

                if (fromAdapter.pointsModel != null)
                {   var pointsName = fromAdapter.pointsModel.name;
                    switch (pointsName)
                    {
                        case "HeadPoints":
                            toAdapter.template = mtMapper.GetTemplate("FacePoints") as PointsTemplate;
                            break;
                        
                        default:
                            var template = mtMapper.GetTemplate(pointsName) as PointsTemplate;
                            if (template != null) toAdapter.template = template;
                            break;
                    }
                }

                reducer.weights = new List<float>();

                for (var i = 0; i < fromAdapter.targetPoints.Count; i++)
                {
                    reducer.weights.Add(1.0f/fromAdapter.targetPoints.Count);
                }

                reducer.scale = fromAdapter.scale;
                reducer.offset = -fromAdapter.anchor;

                if (fromAdapter.controller != null &&
                    (fromAdapter.controller.GetType().IsSubclassOf(typeof(SpriteController)) ||
                     fromAdapter.controller.GetType().IsSubclassOf(typeof(MSRSpriteController))))
                {
                    toAdapter.stabilizeMethod = InterpolationMethod.LinearInterpolation;
                }
               

                toAdapter.indices = fromAdapter.targetPoints;
                toAdapter.reducer = reducer;
                toAdapter.configuration.Add(new PointsReducer.MapItem()
                {
                    sourceComponent = ComponentIndex.X,
                    targetComponent = ComponentIndex.X,
                    targetController = fromAdapter.controller
                });
                toAdapter.configuration.Add(new PointsReducer.MapItem()
                {
                    sourceComponent = ComponentIndex.Y,
                    targetComponent = ComponentIndex.Y,
                    targetController = fromAdapter.controller
                });
                toAdapter.configuration.Add(new PointsReducer.MapItem()
                {
                    sourceComponent = ComponentIndex.Z,
                    targetComponent = ComponentIndex.Z,
                    targetController = fromAdapter.controller
                });
                
                EditorUtility.SetDirty(toAdapter);
                
                Object.DestroyImmediate(fromAdapter);
            }
        }
    }
}