

using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionAdapters
{
    public interface ISerializableAdapter
    {
        
        public void Deserialize(Dictionary<GameObject, GameObject> prefabMapping);

        public void SerializeIntoNewObject(GameObject target, Dictionary<GameObject, GameObject> prefabMapping);
    }
}