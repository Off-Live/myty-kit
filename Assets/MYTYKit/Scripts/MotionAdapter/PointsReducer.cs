using System;
using System.Collections.Generic;
using MYTYKit.Controllers;
using MYTYKit.MotionAdapters.Reduce;
using UnityEngine;
using MYTYKit.MotionTemplates;

namespace MYTYKit.MotionAdapters
{
    public class PointsReducer : DampingAndStabilizingVec3Adapter, ITemplateObserver, ISerializableAdapter
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
            
            if (reducer.gameObject != gameObject)
            {
                Debug.LogWarning("The reducer is not from the same gameobject. it can be exported abnormally");
            }
        
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

        public void SerializeIntoNewObject(GameObject target, Dictionary<GameObject, GameObject> prefabMapping)
        {
            var newAdapter = target.AddComponent<PointsReducer>();
            var mtGo = template.gameObject;
            var prefabGo = prefabMapping[mtGo];
            newAdapter.template = prefabGo.GetComponent<PointsTemplate>();
            newAdapter.indices = new List<int>(indices);

            var serializableReducer = reducer as ISerializableOperator;
            if(serializableReducer!=null) newAdapter.reducer = serializableReducer.SerializeIntoNewObject(target, prefabMapping);
            newAdapter.configuration = new();
            for (var i = 0; i < configuration.Count; i++)
            {
                var newItem = new MapItem();
                newItem.sourceComponent = configuration[i].sourceComponent;
                newItem.targetComponent = configuration[i].targetComponent;
                var conGo = configuration[i].targetController.gameObject;
                var prefabConGo = prefabMapping[conGo];
                newItem.targetController = prefabConGo.GetComponent<MYTYController>();
                newAdapter.configuration.Add(newItem);
            }
        }
        

        public void Deserialize(Dictionary<GameObject, GameObject> prefabMapping)
        {
            template = prefabMapping[template.gameObject].GetComponent<PointsTemplate>();
            foreach(var item in configuration)
            {
                item.targetController = prefabMapping[item.targetController.gameObject].GetComponent<MYTYController>();
            }
        }
    }
}