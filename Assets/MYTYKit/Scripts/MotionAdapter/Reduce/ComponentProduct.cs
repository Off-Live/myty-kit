using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit.MotionAdapters.Reduce
{
    public class ComponentProduct : ReduceOperator, ISerializableOperator
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


        public ReduceOperator SerializeIntoNewObject(GameObject target, Dictionary<GameObject, GameObject> prefabMapping)
        {
            var newOperator = target.AddComponent<ComponentProduct>();
            newOperator.scale = scale;
            newOperator.exponents = new List<float>(exponents);
            return newOperator;
        }
        
        public JObject SerializeToJObject()
        {
            return JObject.FromObject(new
            {
                type = "ComponentProduct",
                scale = new
                {
                    scale.x,
                    scale.y,
                    scale.z
                },
                exponents
            });
        }

        public void DeserializeFromJObject(JObject jObject)
        {
            scale = jObject["scale"].ToObject<Vector3>();
            exponents = jObject["exponents"].ToObject<List<float>>();
        }
    }
}