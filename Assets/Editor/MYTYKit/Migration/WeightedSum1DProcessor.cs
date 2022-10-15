using MYTYKit.MotionAdapters;
using MYTYKit.MotionTemplates;
using UnityEditor;
using UnityEngine;

namespace MYTYKit
{
    public static class WeightedSum1DProcessor
    {
        public static void MigrateWeightedSum1DAdapter(this Migration migration)
        {
            var adapters = Object.FindObjectsOfType<WeightedSum1DAdapter>();
            var mtMapper = Object.FindObjectOfType<MotionTemplateMapper>();
            foreach (var target in adapters)
            {
                var go = target.gameObject;
                var fromAdapter = go.GetComponent<WeightedSum1DAdapter>();
                var toAdapter = go.AddComponent<WeightedSum1DAdapterV2>();
                
                toAdapter.template = mtMapper.GetTemplate("SimpleFaceParam") as ParametricTemplate;
                toAdapter.controller = fromAdapter.controller;
                toAdapter.weights = fromAdapter.weights;
                toAdapter.paramNames = fromAdapter.fields;
                
                toAdapter.controller = fromAdapter.controller;
                EditorUtility.SetDirty(toAdapter);
                
                Object.DestroyImmediate(fromAdapter);
            }
        }
    }
}