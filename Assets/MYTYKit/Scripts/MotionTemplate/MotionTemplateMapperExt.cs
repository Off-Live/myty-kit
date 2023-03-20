using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit.MotionTemplates
{
    public static class MotionTemplateMapperExt
    {
        public static JObject SerializeToJObject(this MotionTemplateMapper mapper)
        {
            var names = mapper.GetNames();
            return JObject.FromObject(new
            {
                templates = names.Select(name => (name, type: mapper.GetTemplate(name).GetType().Name))
            });
        }

        public static void DeserializeFromJObject(this MotionTemplateMapper mapper, JObject jObject)
        {
            mapper.Clear();
            var templates = jObject["templates"].ToObject<List<(string, string)>>();
            templates.ForEach(templatePair =>
            {
                var name = templatePair.Item1;
                var templateType = templatePair.Item2;
                var templateGo = new GameObject
                {
                    name = name,
                    transform =
                    {
                        parent = mapper.transform
                    }
                };

                var typeName = typeof(MotionTemplate).Namespace + "." + templateType + ", "
                               + typeof(MotionTemplate).Assembly.GetName().Name;

                var templateComponent = (MotionTemplate)templateGo.AddComponent(Type.GetType(typeName));
                mapper.SetTemplate(name, templateComponent);
            });
        }
    }
}