using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionAdapters.Reduce
{
    public class LinearCombination : ReduceOperator
    {
        public List<float> weights = new();
        public Vector3 offset = Vector3.zero;
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

            sum += offset;

            return sum;
        }
    }
}