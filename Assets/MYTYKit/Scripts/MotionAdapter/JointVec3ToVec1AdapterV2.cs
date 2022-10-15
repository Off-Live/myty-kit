using System;
using MYTYKit.Controllers;
using UnityEngine;

using MYTYKit.MotionTemplates;

namespace MYTYKit.MotionAdapters
{
    [Serializable]
    public enum ComponentIndex
    {
        X,
        Y,
        Z
    }

    public class JointVec3ToVec1AdapterV2 : DampingAndStabilizingVec3Adapter, ITemplateObserver
    {
        public AnchorTemplate joint;
        public JointVector from;
        public ComponentIndex component;
        public bool negate = false;
        public MYTYController controller;

        void Start()
        {
            ListenToMotionTemplate();
        }

        public void ListenToMotionTemplate()
        {
            joint.SetUpdateCallback(TemplateUpdated);
        }
        public void TemplateUpdated()
        {
            Vector3 vec3 = Vector3.zero;
            var scale = negate ? -1 : 1;

            switch (from)
            {
                case JointVector.LookAt:
                    vec3 = joint.lookAt;
                    break;
                case JointVector.Up:
                    vec3 = joint.up;
                    break;
            }

            float val = 0;

            switch (component)
            {
                case ComponentIndex.X:
                    val = scale * vec3.x;
                    break;
                case ComponentIndex.Y:
                    val = scale * vec3.y;
                    break;
                case ComponentIndex.Z:
                    val = scale * vec3.z;
                    break;
            }

            AddToHistory(new Vector3(val,0,0));
        }
        void Update()
        {
            if (joint == null) return;
            var input = controller as IFloatInput;
            if (input == null) return;

            input.SetInput(GetResult().x);
        }
    }
}