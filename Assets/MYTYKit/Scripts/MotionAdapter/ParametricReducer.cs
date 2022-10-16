using System;
using System.Collections.Generic;
using MYTYKit.Controllers;
using MYTYKit.MotionAdapters.Reduce;
using UnityEngine;
using MYTYKit.MotionTemplates;

namespace MYTYKit.MotionAdapters
{
    public class ParametricReducer : DampingAndStabilizingVec3Adapter, ITemplateObserver
    {
        [Serializable]
        public class ReduceItem
        {
            public List<string> paramNames;
            public ReduceOperator reducer;
            public MYTYController controller;
            public ComponentIndex component;
        }
        
        public ParametricTemplate template;
        public List<ReduceItem> configuration;

        
        protected override void Start()
        {
            base.Start();
            ListenToMotionTemplate();
            SetNumInterpolationSlot(configuration.Count);
        }
        public void TemplateUpdated()
        {
            int slotIdx = 0;
            foreach (var reduceItem in configuration)
            {
                List<Vector3> paramList = new();
                foreach (var name in reduceItem.paramNames)
                {
                    paramList.Add(new Vector3(template.GetValue(name),0,0));
                }

                var result = reduceItem.reducer.Reduce(paramList);
                AddToHistory(result);
                slotIdx++;
            }
        }

        public void ListenToMotionTemplate()
        {
            template.SetUpdateCallback(TemplateUpdated);
        }

        void Update()
        {
            if (template == null) return;

            for (var i = 0; i < configuration.Count; i++)
            {
                var result = GetResult(i);
                var input = configuration[i].controller as IComponentWiseInput;
                if (input == null)
                {
                    Debug.LogWarning(configuration[i].controller + " is not component-wise input");
                    continue;
                }
                
                input.SetComponent(result.x, (int)configuration[i].component);

            }
        }
        
        
    }
}