using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionAdapters.Reduce
{
    public class ComponentProduct : ReduceOperator
    {
        public List<float> exponents;
        public Vector3 scale = Vector3.one;
        public override Vector3 Reduce(List<Vector3> items)
        {
            if (items.Count != exponents.Count)
            {
                Debug.LogError("Item count mismatch");
                return Vector3.zero;
            }

            var prod = Vector3.one;

            for (var i = 0; i < items.Count; i++)
            {
                var powedItem = new Vector3(
                    Mathf.Pow(items[i].x, exponents[i]),
                    Mathf.Pow(items[i].y, exponents[i]),
                    Mathf.Pow(items[i].z, exponents[i])
                );


                prod.Scale(powedItem);
            }
            
            prod.Scale(scale);
            return prod;
        }
    }
}