using MYTYKit.MotionAdapters;
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
                var toAdapter = go.AddComponent<AveragePosFromPointsAdapterV2>();

                if (fromAdapter.pointsModel != null)
                {   var pointsName = fromAdapter.pointsModel.name;
                    switch (pointsName)
                    {
                        case "HeadPoints":
                            toAdapter.pointsModel = mtMapper.GetTemplate("FacePoints") as PointsTemplate;
                            break;
                        
                        default:
                            var template = mtMapper.GetTemplate(pointsName) as PointsTemplate;
                            if (template != null) toAdapter.pointsModel = template;
                            break;
                    }
                }

                toAdapter.targetPoints = fromAdapter.targetPoints;
                toAdapter.controller = fromAdapter.controller;
                
                EditorUtility.SetDirty(toAdapter);
                
                Object.DestroyImmediate(fromAdapter);
            }
        }
    }
}