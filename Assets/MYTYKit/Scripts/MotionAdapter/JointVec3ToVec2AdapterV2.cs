using System;
using MYTYKit.Controllers;
using UnityEngine;

using MYTYKit.MotionTemplates;

namespace MYTYKit.MotionAdapters
{
    [Serializable]
    public enum JointVector
    {
        LookAt,
        Up
    }

    [Serializable]
    public enum ProjectionPlane
    {
        XY,
        YZ,
        XZ
    }


    public class JointVec3ToVec2AdapterV2 : DampingAndStabilizingVec3Adapter, ITemplateObserver
    {
        public AnchorTemplate joint;
        public JointVector from;
        public ProjectionPlane plane;
        public bool flip = false;
        public MYTYController controller;
        
        public bool negate1st = false;
        public bool negate2nd = false;

        public float min1st = -1.0f;
        public float max1st = 1.0f;

        public float min2nd = -1.0f;
        public float max2nd = 1.0f;

        protected override void Start()
        {
            base.Start();
            ListenToMotionTemplate();
            SetNumInterpolationSlot(1);
        }

        public void ListenToMotionTemplate()
        {
            joint.SetUpdateCallback(TemplateUpdated);
        }
        public void TemplateUpdated()
        {
            Vector3 vec3 = Vector3.zero;

            var scale1st = negate1st ? -1 : 1;
            var scale2nd = negate2nd ? -1 : 1;

            switch (from)
            {
                case JointVector.LookAt:
                    vec3 = joint.lookAt;
                    break;
                case JointVector.Up:
                    vec3 = joint.up;
                    break;
            }

            var inputVal = Vector2.zero;

            switch (plane)
            {
                case ProjectionPlane.XY:
                    if (flip) inputVal = new Vector2(vec3.y, vec3.x);
                    else inputVal = new Vector2(vec3.x, vec3.y);
                    break;
                case ProjectionPlane.YZ:
                    if (flip) inputVal = new Vector2(vec3.z, vec3.y);
                    else inputVal = new Vector2(vec3.y, vec3.z);
                    break;
                case ProjectionPlane.XZ:
                    if (flip) inputVal = new Vector2(vec3.z, vec3.x);
                    else inputVal = new Vector2(vec3.x, vec3.z);
                    break;
            }

            var x = Mathf.Clamp(inputVal.x * scale1st, min1st, max1st);
            var y = Mathf.Clamp(inputVal.y * scale2nd, min2nd, max2nd);
            
            AddToHistory(new Vector3(x,y));
        }
        void Update()
        {
            if (joint == null) return;
            var input = controller as IVec2Input;
            if (input == null) return;
            input.SetInput(GetResult());
        }
    }
}