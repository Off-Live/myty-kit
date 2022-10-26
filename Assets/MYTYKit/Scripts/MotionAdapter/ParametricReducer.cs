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


        public GameObject GetSerializedClone(Dictionary<GameObject, GameObject> prefabMapping)
        {
            var motionAdapterClone = Instantiate(gameObject);
            var mtGo = template.gameObject;
            var prefabGo = prefabMapping[mtGo];
            var clonedAdapter = motionAdapterClone.GetComponent<ParametricReducer>();
            motionAdapterClone.name = gameObject.name;
            clonedAdapter.template = prefabGo.GetComponent<ParametricTemplate>();

            for (var i = 0; i < configuration.Count; i++)
            {
                var conGo = configuration[i].controller.gameObject;
                var prefabConGo = prefabMapping[conGo];
                clonedAdapter.configuration[i].controller = prefabConGo.GetComponent<MYTYController>();
            }

            return motionAdapterClone;
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