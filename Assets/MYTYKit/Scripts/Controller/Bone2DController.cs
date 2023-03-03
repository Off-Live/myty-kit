using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace MYTYKit.Controllers
{
    public class Bone2DController : BoneController, IVec2Input, IComponentWiseInput
    {
        public Vector2 controlPosition;
        public float xScale = 1.0f;
        public float yScale = 1.0f;

        public List<RiggingEntity> xminRig;
        public List<RiggingEntity> xmaxRig;
        public List<RiggingEntity> yminRig;
        public List<RiggingEntity> ymaxRig;

        List<RiggingEntity> diffBuffer;

        void Update()
        {
            var interpList = CalcInterpolate();
            if (interpList == null) return;
            diffBuffer = CalcDiff(orgRig, interpList);

        }
   

        public override void ApplyDiff()
        {
            if (diffBuffer == null|| diffBuffer.Count==0) return;
            AccumulatePose(diffBuffer);
        }

    
        protected override List<RiggingEntity> CalcInterpolate()
        {

            if (orgRig == null || orgRig.Count == 0) return null;

            List<RiggingEntity> xmin = xminRig;
            List<RiggingEntity> xmax = xmaxRig;
            List<RiggingEntity> ymin = yminRig;
            List<RiggingEntity> ymax = ymaxRig;

            if (xmin == null || xmin.Count==0) xmin = orgRig;
            if (xmax == null || xmax.Count==0) xmax = orgRig;
            if (ymin == null || ymin.Count==0) ymin = orgRig;
            if (ymax == null || ymax.Count==0) ymax = orgRig;
        

            List<RiggingEntity> interpList;
            if (controlPosition.x >=0 && controlPosition.y >= 0)
            {
                interpList = BilinearInterp(orgRig, xmax ,ymax);   
            }else if (controlPosition.x < 0 && controlPosition.y >= 0)
            {
                interpList = BilinearInterp(orgRig, xmin, ymax);
            }else if (controlPosition.x < 0 && controlPosition.y < 0)
            {
                interpList = BilinearInterp(orgRig, xmin, ymin);
            }
            else
            {
                interpList = BilinearInterp(orgRig, xmax, ymin);
            }


            return interpList;

        }


        private List<RiggingEntity> BilinearInterp(List<RiggingEntity> originList, List<RiggingEntity> xMaxList, List<RiggingEntity> yMaxList)
        {
            var interpList = new List<RiggingEntity>();
            for(int i = 0; i < originList.Count; i++)
            {
                var origin = originList[i];
                var xMax = xMaxList[i];
                var yMax = yMaxList[i];
                RiggingEntity xyMax = new RiggingEntity();
                RiggingEntity interp = new RiggingEntity();
            
                xyMax.position = (xMax.position - origin.position) + (yMax.position - origin.position) + origin.position;
                xyMax.rotation = xMax.rotation * Quaternion.Inverse(origin.rotation) * yMax.rotation;
                xyMax.scale = (xMax.scale - origin.scale) + (yMax.scale - origin.scale) + origin.scale;

                if (xScale == 0) xScale = 1.0f;
                if (yScale == 0) yScale = 1.0f;

                var u = Math.Abs(controlPosition.x)/xScale;
                var v = Math.Abs(controlPosition.y)/yScale;

                interp.position = (1 - u) * (1 - v) * origin.position + u * (1 - v) * xMax.position + (1 - u) * v * yMax.position + u * v * xyMax.position;
                interp.scale = (1 - u) * (1 - v) * origin.scale + u * (1 - v) * xMax.scale + (1 - u) * v * yMax.scale + u * v * xyMax.scale;
                interp.rotation = Quaternion.Slerp(Quaternion.Slerp(origin.rotation, xMax.rotation, u), Quaternion.Slerp(yMax.rotation, xyMax.rotation, u), v);
                interpList.Add(interp);
            }
            return interpList;
        }

        public void SetInput(Vector2 val)
        {
            controlPosition = val;
        }

        public void FlipY()
        {
            if (Application.isPlaying) return;
#if UNITY_EDITOR
            var so = new SerializedObject(this);
            var yminProp = so.FindProperty("yminRig");
            var ymaxProp = so.FindProperty("ymaxRig");

            for (int i = 0; i < yminProp.arraySize; i++)
            {
                var tmp = new RiggingEntity()
                {
                    position = yminProp.GetArrayElementAtIndex(i).FindPropertyRelative("position").vector3Value,
                    rotation = yminProp.GetArrayElementAtIndex(i).FindPropertyRelative("rotation").quaternionValue,
                    scale = yminProp.GetArrayElementAtIndex(i).FindPropertyRelative("scale").vector3Value
                };

                yminProp.GetArrayElementAtIndex(i).FindPropertyRelative("position").vector3Value =
                    ymaxProp.GetArrayElementAtIndex(i).FindPropertyRelative("position").vector3Value;
                yminProp.GetArrayElementAtIndex(i).FindPropertyRelative("rotation").quaternionValue =
                    ymaxProp.GetArrayElementAtIndex(i).FindPropertyRelative("rotation").quaternionValue;
                yminProp.GetArrayElementAtIndex(i).FindPropertyRelative("scale").vector3Value =
                    ymaxProp.GetArrayElementAtIndex(i).FindPropertyRelative("scale").vector3Value;

                ymaxProp.GetArrayElementAtIndex(i).FindPropertyRelative("position").vector3Value =
                    tmp.position;
                ymaxProp.GetArrayElementAtIndex(i).FindPropertyRelative("rotation").quaternionValue =
                    tmp.rotation;
                ymaxProp.GetArrayElementAtIndex(i).FindPropertyRelative("scale").vector3Value =
                    tmp.scale;
            }

            so.ApplyModifiedProperties();
#endif
        }

        public void FlipX()
        {
            if (Application.isPlaying) return;
#if UNITY_EDITOR
            var so = new SerializedObject(this);
            var xminProp = so.FindProperty("xminRig");
            var xmaxProp = so.FindProperty("xmaxRig");

            for (int i = 0; i < xminProp.arraySize; i++)
            {
                var tmp = new RiggingEntity()
                {
                    position = xminProp.GetArrayElementAtIndex(i).FindPropertyRelative("position").vector3Value,
                    rotation = xminProp.GetArrayElementAtIndex(i).FindPropertyRelative("rotation").quaternionValue,
                    scale = xminProp.GetArrayElementAtIndex(i).FindPropertyRelative("scale").vector3Value
                };

                xminProp.GetArrayElementAtIndex(i).FindPropertyRelative("position").vector3Value =
                    xmaxProp.GetArrayElementAtIndex(i).FindPropertyRelative("position").vector3Value;
                xminProp.GetArrayElementAtIndex(i).FindPropertyRelative("rotation").quaternionValue =
                    xmaxProp.GetArrayElementAtIndex(i).FindPropertyRelative("rotation").quaternionValue;
                xminProp.GetArrayElementAtIndex(i).FindPropertyRelative("scale").vector3Value =
                    xmaxProp.GetArrayElementAtIndex(i).FindPropertyRelative("scale").vector3Value;

                xmaxProp.GetArrayElementAtIndex(i).FindPropertyRelative("position").vector3Value =
                    tmp.position;
                xmaxProp.GetArrayElementAtIndex(i).FindPropertyRelative("rotation").quaternionValue =
                    tmp.rotation;
                xmaxProp.GetArrayElementAtIndex(i).FindPropertyRelative("scale").vector3Value =
                    tmp.scale;
            }

            so.ApplyModifiedProperties();
#endif


        }
        public void SetComponent(float value, int componentIdx)
        {
            if (componentIdx >= 2 || componentIdx < 0) return;
            controlPosition[componentIdx] = value;
        }
        
        public override JObject SerializeToJObject(Dictionary<Transform, int> tfMap)
        {
            var baseJo =  base.SerializeToJObject(tfMap);
            var jo = JObject.FromObject(new
            {
                xScale,
                yScale,
                xmaxRig = xmaxRig.Select(item => item.SerializeToJObject()).ToArray(),
                xminRig = xminRig.Select(item => item.SerializeToJObject()).ToArray(),
                ymaxRig = ymaxRig.Select(item => item.SerializeToJObject()).ToArray(),
                yminRig = yminRig.Select(item => item.SerializeToJObject()).ToArray()

            });
            baseJo.Merge(jo);
            return baseJo;
        }
    }
}
