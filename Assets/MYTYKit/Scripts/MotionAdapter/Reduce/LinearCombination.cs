using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit.MotionAdapters.Reduce
{
    public class LinearCombination : ReduceOperator, ISerializableOperator
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

        public ReduceOperator SerializeIntoNewObject(GameObject target, Dictionary<GameObject, GameObject> prefabMapping)
        {
            var newOperator = target.AddComponent<LinearCombination>();
            newOperator.offset = offset;
            newOperator.scale = scale;
            newOperator.weights = new List<float>(weights);
            return newOperator;
        }

        public JObject SerializeToJObject()
        {
            return JObject.FromObject(new
            {
                type = "LinearCombination",
                scale = new
                {
                    scale.x,
                    scale.y,
                    scale.z
                },
                offset = new
                {
                    offset.x,
                    offset.y,
                    offset.z
                },
                weights
            });
        }

        public void DeserializeFromJObject(JObject jObject)
        {
            scale = jObject["scale"].ToObject<Vector3>();
            offset = jObject["offset"].ToObject<Vector3>();
            weights = jObject["weights"].ToObject<List<float>>();
        }
    }

}