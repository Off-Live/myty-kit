using System;
using System.Collections.Generic;
using System.Linq;
using MYTYKit.Controllers;
using MYTYKit.MotionAdapters.Reduce;
using UnityEngine;
using MYTYKit.MotionTemplates;
using Newtonsoft.Json.Linq;

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

        public new void SerializeIntoNewObject(GameObject target, Dictionary<GameObject, GameObject> prefabMapping)
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

                var serializableReducer = configuration[i].reducer as ISerializableOperator;
                if(serializableReducer!=null) newItem.reducer = serializableReducer.SerializeIntoNewObject(target, prefabMapping);
                newItem.paramNames = new List<string>(configuration[i].paramNames);
                
                newAdapter.configuration.Add(newItem);
            }
        }
        
        public new void Deserialize(Dictionary<GameObject, GameObject> prefabMapping)
        {
            template = prefabMapping[template.gameObject].GetComponent<ParametricTemplate>();
            foreach(var item in configuration)
            {
                item.controller = prefabMapping[item.controller.gameObject].GetComponent<MYTYController>();
            }
        }

        public new JObject SerializeToJObject(Dictionary<Transform, int> transformMap)
        {
            var mapper = FindObjectOfType<MotionTemplateMapper>();
            if (mapper == null)
            {
                throw new MYTYException("MotionTemplateMapper cannot be found.");
            }

            var templateName = mapper.GetName(template); 
            Debug.Assert(templateName!=null);
            var baseJo = base.SerializeToJObject(transformMap);
            var thisJo = JObject.FromObject(new
            {
                type = "ParametricReducer",
                templateName,
                configuration = configuration.Select(item => JObject.FromObject(new
                {
                    item.paramNames,
                    reducer = (item.reducer as ISerializableOperator).SerializeToJObject(),
                    controller = transformMap[item.controller.transform],
                    component = (int)item.component
                }))
            });
            
            baseJo.Merge(thisJo);

            return baseJo;
        }

        
        public new void DeserializeFromJObject(JObject jObject, Dictionary<int, Transform> idTransformMap)
        {
            if (motionTemplateMapper == null)
            {
                throw new MYTYException("MotionTemplateMapper should be set up first.");
            }
            base.DeserializeFromJObject(jObject, idTransformMap);
            template = motionTemplateMapper.GetTemplate((string)jObject["templateName"]) as ParametricTemplate;
            configuration = jObject["configuration"].ToArray().Select(token =>
            {
                var reducerJo = token["reducer"] as JObject;
                var typeName = typeof(ISerializableOperator).Namespace + "." + (string)reducerJo["type"] + ", "
                               + typeof(ISerializableOperator).Assembly.GetName().Name;
                var reducerComponent = (ReduceOperator) gameObject.AddComponent(Type.GetType(typeName));
                ((ISerializableOperator)reducerComponent).DeserializeFromJObject(reducerJo);
                
                return new ReduceItem()
                {
                    paramNames = token["paramNames"].ToObject<List<string>>(),
                    reducer = reducerComponent,
                    component = (ComponentIndex)(int)token["component"],
                    controller = idTransformMap[(int)token["controller"]].GetComponent<MYTYController>()
                };
            }).ToList();
            
        }
    }
}