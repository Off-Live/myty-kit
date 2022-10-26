using System;
using UnityEngine;

namespace MYTYKit.MotionTemplates
{
    public class AnchorTemplate : MotionTemplate
    {
        public Vector3 up;
        public Vector3 lookAt;
        public Vector3 position;
        public Vector3 scale = Vector3.one;

        void Start()
        {
            scale = Vector3.one;
        }
    }
}
