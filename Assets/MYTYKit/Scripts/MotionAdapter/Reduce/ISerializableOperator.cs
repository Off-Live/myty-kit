using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit.MotionAdapters.Reduce
{
    public interface ISerializableOperator
    {
        public ReduceOperator SerializeIntoNewObject(GameObject target, Dictionary<GameObject, GameObject> prefabMapping);

        public JObject SerializeToJObject();
        public void DeserializeFromJObject(JObject jObject);
    }
}