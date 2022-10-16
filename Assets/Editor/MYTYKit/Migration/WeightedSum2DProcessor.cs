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

                var xNameList = new List<string>();
                var yNameList = new List<string>();
                foreach (var name in fromAdapter.fields)
                {
                    xNameList.Add(name+"X");
                    yNameList.Add(name+"Y");
                }
                
                toAdapter.configuration.Add(new ParametricReducer.ReduceItem()
                {
                    reducer = reducer,
                    paramNames = xNameList,
                    controller = fromAdapter.controller,
                    component = ComponentIndex.X
                });
                
                toAdapter.configuration.Add(new ParametricReducer.ReduceItem()
                {
                    reducer = reducer,
                    paramNames = yNameList,
                    controller = fromAdapter.controller,
                    component = ComponentIndex.Y
                });
                
                EditorUtility.SetDirty(toAdapter);
                
                Object.DestroyImmediate(fromAdapter);
            }
        }
    }
}