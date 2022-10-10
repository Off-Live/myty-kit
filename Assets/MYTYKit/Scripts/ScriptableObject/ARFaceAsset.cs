using System;
using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit
{
    [Serializable]
    public class ARFaceItem
    {
        public bool isValid = false;
        public Camera renderCam;
        public List<string> traits;
    }

    public class ARFaceAsset : ScriptableObject
    {
        public bool AROnly = false;
        public ARFaceItem[] items;
    }
}
