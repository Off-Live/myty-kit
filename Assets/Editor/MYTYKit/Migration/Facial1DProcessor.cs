using MYTYKit.MotionAdapters;
using MYTYKit.MotionTemplates;
using UnityEditor;
using UnityEngine;
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
                var toAdapter = go.AddComponent<Parametric1DAdapter>();

                toAdapter.template = mtMapper.GetTemplate("SimpleFaceParam") as ParametricTemplate;
                toAdapter.paramName = fromAdapter.fieldName;
                toAdapter.con = fromAdapter.con;
                
                EditorUtility.SetDirty(toAdapter);
                Object.DestroyImmediate(fromAdapter);
            }
        }
    }
}