using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionAdapters.Reduce
{
    public interface ISerializableOperator
    {
        public ReduceOperator SerializeIntoNewObject(GameObject target, Dictionary<GameObject, GameObject> prefabMapping);
    }
}