using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionAdapters.Reduce
{
    public abstract class ReduceOperator: MonoBehaviour, ISerializableOperator
    {
        public abstract Vector3 Reduce(List<Vector3> items);

        public abstract ReduceOperator SerializeIntoNewObject(GameObject target, Dictionary<GameObject, GameObject> prefabMapping);

    }
}