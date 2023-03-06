

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit.MotionAdapters
{
    public interface ISerializableAdapter
    {
        
        public void Deserialize(Dictionary<GameObject, GameObject> prefabMapping);

        public void SerializeIntoNewObject(GameObject target, Dictionary<GameObject, GameObject> prefabMapping);
        public JObject SerializeToJObject(Dictionary<Transform, int> transformMap);
    }
}