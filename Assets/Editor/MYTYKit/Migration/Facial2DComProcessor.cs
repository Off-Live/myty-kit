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
    public static class Facial2DComProcessor
    {
        public static void MigrateFacial2DCompound(this Migration migration)
        {
            var adapters = Object.FindObjectsOfType<Facial2DCompound>();
            var mtMapper = Object.FindObjectOfType<MotionTemplateMapper>();
            foreach (var target in adapters)
            {
                var go = target.gameObject;
                var fromAdapter = go.GetComponent<Facial2DCompound>();
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
                    paramNames = new List<string>() { fromAdapter.xFieldName },
                    controller = fromAdapter.con,
                    component = ComponentIndex.X
                });
                toAdapter.configuration.Add(new ParametricReducer.ReduceItem()
                {
                    reducer = reducer,
                    paramNames = new List<string>() { fromAdapter.yFieldName },
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