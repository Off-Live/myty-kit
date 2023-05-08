using System.Collections.Generic;
using MYTYKit.Controllers;
using MYTYKit.MotionTemplates;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit.MotionAdapters
{
    public class BoneBlendshapeAdapter : NativeAdapter, ITemplateObserver, ISerializableAdapter
    {
        public ParametricTemplate template;
        public BoneBlendShapeController controller;

        void Start()
        {
            ListenToMotionTemplate();
        }
        
        public void TemplateUpdated()
        {
            foreach (var basis in controller.blendShapes)
            {
                var key = basis.name[0].ToString().ToLower() + basis.name.Substring(1);   
                var weight = template.GetValue(key);
                basis.weight = weight;
            }
        }

        public void ListenToMotionTemplate()
        {
            template.SetUpdateCallback(TemplateUpdated);
        }

        public void Deserialize(Dictionary<GameObject, GameObject> prefabMapping)
        {
            template = prefabMapping[template.gameObject].GetComponent<ParametricTemplate>();
            controller = prefabMapping[controller.gameObject].GetComponent<BoneBlendShapeController>();
        }

        public void SerializeIntoNewObject(GameObject target, Dictionary<GameObject, GameObject> prefabMapping)
        {
            var newAdapter = target.AddComponent<BoneBlendshapeAdapter>();
            var mtGo = template.gameObject;
            var prefabGo = prefabMapping[mtGo];
            var prefabConGo = prefabMapping[controller.gameObject];
            newAdapter.template = prefabGo.GetComponent<ParametricTemplate>();
            newAdapter.controller = prefabConGo.GetComponent<BoneBlendShapeController>();
        }

        public JObject SerializeToJObject(Dictionary<Transform, int> transformMap)
        {
            var mapper = FindObjectOfType<MotionTemplateMapper>();
            if (mapper == null)
            {
                throw new MYTYException("MotionTemplateMapper cannot be found.");
            }

            var templateName = mapper.GetName(template); 
            Debug.Assert(templateName!=null);
            
            return JObject.FromObject(new
            {
                type = "BoneBlendshapeAdapter",
                templateName,
                controllerId = transformMap[controller.transform]
            });
        }

        public void DeserializeFromJObject(JObject jObject, Dictionary<int, Transform> idTransformMap)
        {
            if (motionTemplateMapper == null)
            {
                throw new MYTYException("MotionTemplateMapper should be set up first.");
            }
            
            template = motionTemplateMapper.GetTemplate((string)jObject["templateName"]) as ParametricTemplate;
            controller = idTransformMap[(int)jObject["controllerId"]].GetComponent<BoneBlendShapeController>();
        }
    }
}