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
                var toAdapter = go.AddComponent<ParametricReducer>();
                var reducer = go.AddComponent<LinearCombination>();
                
                toAdapter.template = mtMapper.GetTemplate("SimpleFaceParam") as ParametricTemplate;
                if (fromAdapter.controller != null &&
                    (fromAdapter.controller.GetType().IsSubclassOf(typeof(SpriteController)) ||
                     fromAdapter.controller.GetType().IsSubclassOf(typeof(MSRSpriteController))))
                {
                    toAdapter.stabilizeMethod = InterpolationMethod.LinearInterpolation;
                }

                reducer.weights = fromAdapter.weights;
                
                toAdapter.configuration.Add(new ParametricReducer.ReduceItem()
                {
                    reducer = reducer,
                    paramNames = fromAdapter.fields,
                    controller = fromAdapter.controller,
                    component = ComponentIndex.X
                });
                
                EditorUtility.SetDirty(toAdapter);
                
                Object.DestroyImmediate(fromAdapter);
            }
        }
    }
}