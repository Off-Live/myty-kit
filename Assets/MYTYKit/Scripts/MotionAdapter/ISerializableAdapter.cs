

using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionAdapters
{
    public interface ISerializableAdapter
    {
        public GameObject GetSerializedClone(Dictionary<GameObject, GameObject> prefabMapping);
        public void Deserialize(Dictionary<GameObject, GameObject> prefabMapping);
    }
}