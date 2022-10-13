using MYTYKit.MotionAdapters;
using MYTYKit.MotionTemplates;
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
                var toAdapter = go.AddComponent<Parametric2DAdapter>();

                toAdapter.stabilizeWindow = fromAdapter.stabilizeWindow;
                toAdapter.smoothWindow = fromAdapter.smoothWindow;
                toAdapter.template = mtMapper.GetTemplate("SimpleFaceParam") as ParametricTemplate;
                toAdapter.xParamName = fromAdapter.xFieldName;
                toAdapter.yParamName = fromAdapter.yFieldName;
                toAdapter.con = fromAdapter.con;
                toAdapter.stabilizeTime = fromAdapter.stabilizeTime;
                
                Object.DestroyImmediate(fromAdapter);
            }
        }
    }
}