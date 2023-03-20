using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MYTYKit
{
    public static class TransformExtension
    {

        public static JObject Serialize(this Transform tf)
        {
            var position = tf.localPosition;
            var rotation = tf.localRotation;
            var scale = tf.localScale;
            return JObject.FromObject(new
            {
                position = new
                {
                    position.x,
                    position.y,
                    position.z,
                },
                rotation = new
                {
                    rotation.x,
                    rotation.y,
                    rotation.z,
                    rotation.w,
                },
                scale = new
                {
                    scale.x,
                    scale.y,
                    scale.z,
                }
            });
        }

        public static void Deserialize(this Transform tf, JObject tfJson)
        {
            var position = tfJson["position"] as JObject;
            var rotation = tfJson["rotation"] as JObject;
            var scale = tfJson["scale"] as JObject;

            tf.localPosition = new Vector3(
                (float)position["x"],
                (float)position["y"],
                (float)position["z"]);
            tf.localRotation = new Quaternion(
                (float)rotation["x"],
                (float)rotation["y"],
                (float)rotation["z"],
                (float)rotation["w"]
            );
            tf.localScale = new Vector3(
                (float)scale["x"],
                (float)scale["y"],
                (float)scale["z"]);
        }
        public static List<Transform> GetChildrenList(this Transform tf)
        {
            return Enumerable.Range(0, tf.childCount).Select(idx => tf.GetChild(idx)).ToList();
        }

    }
}