using System;
using System.Collections.Generic;
using MYTYKit.Controllers;
using MYTYKit.MotionAdapters.Reduce;
using UnityEngine;
using MYTYKit.MotionTemplates;

namespace MYTYKit.MotionAdapters
{
    public class ParametricReducer : DampingAndStabilizingVec3Adapter, ITemplateObserver, ISerializableAdapter
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
        public List<ReduceItem> configuration = new();
        


        protected override void Start()
        {
            base.Start();
            ListenToMotionTemplate();
            SetNumInterpolationSlot(configuration.Count);
            foreach (var item in configuration)
            {
                if (item.reducer.gameObject != gameObject)
                {
                    Debug.LogWarning("The reducer is not from the same gameobject. it can be exported abnormally");
                }
            }
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
                AddToHistory(result,slotIdx);
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

        public void SerializeIntoNewObject(GameObject target, Dictionary<GameObject, GameObject> prefabMapping)
        {
            var newAdapter = target.AddComponent<ParametricReducer>();
            var mtGo = template.gameObject;
            var prefabGo = prefabMapping[mtGo];
            newAdapter.template = prefabGo.GetComponent<ParametricTemplate>();
            newAdapter.configuration = new();
            for (var i = 0; i < configuration.Count; i++)
            {
                var newItem = new ReduceItem();
                newItem.component = configuration[i].component;
                var conGo = configuration[i].controller.gameObject;
                var prefabConGo = prefabMapping[conGo];
                newItem.controller = prefabConGo.GetComponent<MYTYController>();
                newItem.reducer = configuration[i].reducer.SerializeIntoNewObject(target, prefabMapping);
                newItem.paramNames = new List<string>(configuration[i].paramNames);
                
                newAdapter.configuration.Add(newItem);
            }
        }
        
        public void Deserialize(Dictionary<GameObject, GameObject> prefabMapping)
        {
            template = prefabMapping[template.gameObject].GetComponent<ParametricTemplate>();
            foreach(var item in configuration)
            {
                item.controller = prefabMapping[item.controller.gameObject].GetComponent<MYTYController>();
            }


        }
    }
}