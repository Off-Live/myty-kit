using System;
using System.Collections.Generic;
using MYTYKit.Controllers;
using MYTYKit.MotionAdapters.Reduce;
using UnityEngine;
using MYTYKit.MotionTemplates;

namespace MYTYKit.MotionAdapters
{
    public class PointsReducer : DampingAndStabilizingVec3Adapter, ITemplateObserver
    {
        [Serializable]
        public class MapItem
        {
            public ComponentIndex sourceComponent;
            public MYTYController targetController;
            public ComponentIndex targetComponent;
        }
        
        public PointsTemplate template;
        public List<int> indices;
        public ReduceOperator reducer;
        
        public List<MapItem> configuration = new();

        
        protected override void Start()
        {
            base.Start();
            ListenToMotionTemplate();
            SetNumInterpolationSlot(1);
        }
        public void TemplateUpdated()
        {
            List<Vector3> reducerInput = new();
            if (indices == null || indices.Count == 0)
            {
                foreach (var point in template.points)
                {
                    reducerInput.Add(point);
                }
            }
            else
            {
                foreach (var index in indices)
                {
                    reducerInput.Add(template.points[index]);
                }
            }
            
            AddToHistory(reducer.Reduce(reducerInput));
        }

        public void ListenToMotionTemplate()
        {
            template.SetUpdateCallback(TemplateUpdated);
        }

        void Update()
        {
            if (template == null) return;

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
                input.SetComponent(sourceValue, (int) mapItem.targetComponent);
            }
        }
    }
}