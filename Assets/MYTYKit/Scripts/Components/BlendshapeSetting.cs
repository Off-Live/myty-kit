using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MYTYKit.Components
{
    public class BlendShapeSetting : MonoBehaviour
    {
        [Serializable]
        public enum BlendShape
        {
            EyeBlinkLeft,
            EyeLookDownLeft,
            EyeLookInLeft,
            EyeLookOutLeft,
            EyeLookUpLeft,
            EyeSquintLeft,
            EyeWideLeft,
            EyeBlinkRight,
            EyeLookDownRight,
            EyeLookInRight,
            EyeLookOutRight,
            EyeLookUpRight,
            EyeSquintRight,
            EyeWideRight,
            JawForward,
            JawLeft,
            JawRight,
            JawOpen,
            MouthClose,
            MouthFunnel,
            MouthPucker,
            MouthLeft,
            MouthRight,
            MouthSmileLeft,
            MouthSmileRight,
            MouthFrownLeft,
            MouthFrownRight,
            MouthDimpleLeft,
            MouthDimpleRight,
            MouthStretchLeft,
            MouthStretchRight,
            MouthRollLower,
            MouthRollUpper,
            MouthShrugLower,
            MouthShrugUpper,
            MouthPressLeft,
            MouthPressRight,
            MouthLowerDownLeft,
            MouthLowerDownRight,
            MouthUpperUpLeft,
            MouthUpperUpRight,
            BrowDownLeft,
            BrowDownRight,
            BrowInnerUp,
            BrowOuterUpLeft,
            BrowOuterUpRight,
            CheekPuff,
            CheekSquintLeft,
            CheekSquintRight,
            NoseSneerLeft,
            NoseSneerRight,
            TongueOut
        }

        [Serializable]
        public class BlendShapeItem
        {
            public string blendShape;
            public string nameOnAvatar;
        }

        public SkinnedMeshRenderer mesh;
        public List<BlendShapeItem> blendShapes;
        
        public static List<string> GetAllBlendShapeNames()
        {
            var enumArray = Enum.GetValues(typeof(BlendShape));
            return Enumerable.Range(0, enumArray.Length).Select(idx => enumArray.GetValue(idx) + "")
                .Select(keyString => char.ToLower(keyString.First()) + keyString.Substring(1)).ToList();
        }

        void Start()
        {
            if (blendShapes == null || blendShapes.Count == 0)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            blendShapes = new();
            GetAllBlendShapeNames().ForEach(name => blendShapes.Add(
                new BlendShapeItem(){blendShape = name, nameOnAvatar = name}));
        }

        public string GetMappedBSName(string name)
        {
            return blendShapes.First(item => item.blendShape == name).nameOnAvatar;
        }

    }
}