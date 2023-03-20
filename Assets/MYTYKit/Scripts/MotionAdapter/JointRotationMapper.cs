using System;
using System.Collections.Generic;
using System.Linq;
using MYTYKit.Controllers;
using UnityEngine;
using MYTYKit.MotionTemplates;
using Newtonsoft.Json.Linq;

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


    public class JointRotationMapper : DampingAndStabilizingVec3Adapter, ITemplateObserver, ISerializableAdapter
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

        public new void SerializeIntoNewObject(GameObject target, Dictionary<GameObject, GameObject> prefabMapping)
        {
            var newAdapter = target.AddComponent<JointRotationMapper>();
            var mtGo = joint.gameObject;
            var prefabGo = prefabMapping[mtGo];
            newAdapter.joint = prefabGo.GetComponent<AnchorTemplate>();
            newAdapter.from = from;
            newAdapter.configuration = new();
            for (var i = 0; i < configuration.Count; i++)
            {
                var newItem = new MapItem();
                newItem.isInverted = configuration[i].isInverted;
                newItem.max = configuration[i].max;
                newItem.min = configuration[i].min;
                newItem.sourceComponent = configuration[i].sourceComponent;
                newItem.targetComponent = configuration[i].targetComponent;

                var conGo = configuration[i].targetController.gameObject;
                var prefabConGo = prefabMapping[conGo];
                newItem.targetController = prefabConGo.GetComponent<MYTYController>();
                newAdapter.configuration.Add(newItem);
            }
        }
        

        public new void Deserialize(Dictionary<GameObject, GameObject> prefabMapping)
        {
            joint = prefabMapping[joint.gameObject].GetComponent<AnchorTemplate>();
            foreach(var item in configuration)
            {
                item.targetController = prefabMapping[item.targetController.gameObject].GetComponent<MYTYController>();
            }
            
            
        }

        public new JObject SerializeToJObject(Dictionary<Transform, int> transformMap)
        {
            var mapper = FindObjectOfType<MotionTemplateMapper>();
            if (mapper == null)
            {
                throw new MYTYException("MotionTemplateMapper cannot be found.");
            }

            var jointName = mapper.GetName(joint);
            Debug.Assert(jointName!=null);
            var baseJo = base.SerializeToJObject(transformMap);
            var thisJo = JObject.FromObject(new
            {
                jointName,
                type = "JointRotationMapper",
                from = from.ToString(),
                configuration = configuration.Select(item => JObject.FromObject(new
                {
                    item.min,
                    item.max,
                    item.isInverted,
                    sourceComponent = (int)item.sourceComponent,
                    targetComponent = (int)item.targetComponent,
                    targetController = transformMap[item.targetController.transform]
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
            joint = motionTemplateMapper.GetTemplate((string)jObject["jointName"]) as AnchorTemplate;
            from = (JointVector)Enum.Parse(typeof(JointVector), (string)jObject["from"]);
            configuration = jObject["configuration"].ToArray().Select(token => new MapItem()
            {
                min = (float) token["min"],
                max = (float) token["max"],
                isInverted = (bool) token["isInverted"],
                sourceComponent = (ComponentIndex) (int)token["sourceComponent"],
                targetComponent = (ComponentIndex) (int)token["targetComponent"],
                targetController = idTransformMap[(int)token["targetController"]].GetComponent<MYTYController>()
            }).ToList();

        }
    }
}