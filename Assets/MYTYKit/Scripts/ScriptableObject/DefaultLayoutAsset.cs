using System;
using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit
{
    [Serializable]
    public class TransformProperty
    {
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;
    }

    public class DefaultLayoutAsset : ScriptableObject
    {
        public Camera camera;
        public List<TransformProperty> templateTransforms;
    }
}
