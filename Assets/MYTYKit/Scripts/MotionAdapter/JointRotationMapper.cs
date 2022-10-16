using System;
using System.Collections.Generic;
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
    
    
    [Serializable]
    public enum JointVector
    {
        LookAt,Up
    }


    public class JointRotationMapper : DampingAndStabilizingVec3Adapter, ITemplateObserver
    {
        [Serializable]
        public class MapItem
        {
            public ComponentIndex sourceComponent;
            public MYTYController targetController;
            public ComponentIndex targetComponent;
            public bool isInverted = false;
            public float min = -1.0f;
            public float max = 1.0f;
        }

        public AnchorTemplate joint;
        public JointVector from;

        public List<MapItem> configuration = new();
        
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

            switch (from)
            {
                case JointVector.LookAt:
                    vec3 = joint.lookAt;
                    break;
                case JointVector.Up:
                    vec3 = joint.up;
                    break;
            }
            AddToHistory(vec3);
        }
        void Update()
        {
            if (joint == null) return;
            Vector3 sourceVector = GetResult();

            foreach (var mapItem in configuration)
            {
                var input = mapItem.targetController as IComponentWiseInput;
                if (input == null)
                {
                    Debug.LogWarning(mapItem.targetController.name + " is not component-wise input");
                    continue;
                }

                var sourceValue = sourceVector[(int)mapItem.sourceComponent];
                if (mapItem.isInverted) sourceValue = -sourceValue;
                sourceValue = Mathf.Clamp(sourceValue, mapItem.min, mapItem.max);
                
                input.SetComponent(sourceValue, (int) mapItem.targetComponent);
            }
            
        }
    }
}