using MYTYKit.MotionAdapters;
using MYTYKit.MotionTemplates;
using UnityEditor;
using UnityEngine;

namespace MYTYKit
{
    public static class Facial2DProcessor
    {
        public static void MigrateFacial2DAdapter(this Migration migration)
        {
            var adapters = Object.FindObjectsOfType<Facial2DAdapter>();
            var mtMapper = Object.FindObjectOfType<MotionTemplateMapper>();
            foreach (var target in adapters)
            {
                var go = target.gameObject;
                var fromAdapter = go.GetComponent<Facial2DAdapter>();
                var toAdapter = go.AddComponent<Parametric2DAdapter>();
                
                toAdapter.template = mtMapper.GetTemplate("SimpleFaceParam") as ParametricTemplate;
                toAdapter.xParamName = fromAdapter.fieldName+"X";
                toAdapter.yParamName = fromAdapter.fieldName+"Y";
                toAdapter.con = fromAdapter.con;
                
                EditorUtility.SetDirty(toAdapter);
                Object.DestroyImmediate(fromAdapter);
            }
        }
    }
}