using MYTYKit.MotionAdapters;
using MYTYKit.MotionTemplates;
using UnityEditor;
using UnityEngine;

namespace MYTYKit
{
    public static class WeightedSum2DProcessor
    {
        public static void MigrateWeightedSum2DAdapter(this Migration migration)
        {
            var adapters = Object.FindObjectsOfType<WeightedSum2DAdapter>();
            var mtMapper = Object.FindObjectOfType<MotionTemplateMapper>();
            foreach (var target in adapters)
            {
                var go = target.gameObject;
                var fromAdapter = go.GetComponent<WeightedSum2DAdapter>();
                var toAdapter = go.AddComponent<WeightedSum1DAdapterV2>();

                toAdapter.stabilizeWindow = fromAdapter.stabilizeWindow;
                toAdapter.smoothWindow = fromAdapter.smoothWindow;
                
                toAdapter.template = mtMapper.GetTemplate("SimpleFaceParam") as ParametricTemplate;
                toAdapter.controller = fromAdapter.controller;
                toAdapter.weights = fromAdapter.weights;
                toAdapter.paramNames = fromAdapter.fields;
                
                toAdapter.controller = fromAdapter.controller;
                toAdapter.stabilizeTime = fromAdapter.stabilizeTime;
                EditorUtility.SetDirty(toAdapter);
                
                Object.DestroyImmediate(fromAdapter);
            }
        }
    }
}