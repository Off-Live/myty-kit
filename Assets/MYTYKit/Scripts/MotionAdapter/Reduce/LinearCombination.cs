using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionAdapters.Reduce
{
    public class LinearCombination : ReduceOperator
    {
        public List<float> weights = new();
        public Vector3 offset = Vector3.zero;
        public Vector3 scale = Vector3.one;
        public override Vector3 Reduce(List<Vector3> items)
        {
            if (items.Count != weights.Count)
            {
                Debug.LogError("Item count mismatch");
                return Vector3.zero;
            }

            var sum = Vector3.zero;
            
            for (var i = 0; i < items.Count; i++)
            {
                sum += weights[i] * items[i];
            }
            
            sum.Scale(scale);
            sum += offset;

            return sum;
        }

        public override ReduceOperator SerializeIntoNewObject(GameObject target, Dictionary<GameObject, GameObject> prefabMapping)
        {
            var newOperator = target.AddComponent<LinearCombination>();
            newOperator.offset = offset;
            newOperator.scale = scale;
            newOperator.weights = new List<float>(weights);
            return newOperator;
        }
    }
}