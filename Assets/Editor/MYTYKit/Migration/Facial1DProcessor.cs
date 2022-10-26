using System.Collections.Generic;
using MYTYKit.Controllers;
using MYTYKit.MotionAdapters;
using MYTYKit.MotionAdapters.Interpolation;
using MYTYKit.MotionTemplates;
using UnityEditor;
using UnityEngine;
using MYTYKit.MotionAdapters.Reduce;
namespace MYTYKit
{
    public static class Facial1DProcessor
    {
        public static void MigrateFacial1DAdapter(this Migration migration)
        {
            var adapters = Object.FindObjectsOfType<Facial1DAdapter>();
            var mtMapper = Object.FindObjectOfType<MotionTemplateMapper>();
            foreach (var target in adapters)
            {
                var go = target.gameObject;
                var fromAdapter = go.GetComponent<Facial1DAdapter>();
                var toAdapter = go.AddComponent<ParametricReducer>();
                var reducer = go.AddComponent<LinearCombination>();

                if (fromAdapter.con != null &&
                    (fromAdapter.con.GetType().IsSubclassOf(typeof(SpriteController)) ||
                     fromAdapter.con.GetType().IsSubclassOf(typeof(MSRSpriteController))))
                {
                    toAdapter.stabilizeMethod = InterpolationMethod.LinearInterpolation;
                }
                toAdapter.template = mtMapper.GetTemplate("SimpleFaceParam") as ParametricTemplate;
                toAdapter.configuration.Add(new ParametricReducer.ReduceItem()
                {
                    reducer = reducer,
                    paramNames = new List<string>() { fromAdapter.fieldName },
                    controller = fromAdapter.con,
                    component = ComponentIndex.X
                });

                reducer.weights = new List<float>() { 1.0f };
                
                EditorUtility.SetDirty(toAdapter);
                Object.DestroyImmediate(fromAdapter);
            }
        }
    }
}