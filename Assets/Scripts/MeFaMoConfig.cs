using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using UnityEngine;

public class MeFaMoConfig
{
    public static readonly int[] eye_right = { 33, 133, 160, 159, 158, 144, 145, 153 };
    public static readonly int[] eye_left = { 263, 362, 387, 386, 385, 373, 374, 380 };
    public static readonly int[] head = { 10, 152 };
    public static readonly int nose_tip = 1;
    public static readonly int upper_lip = 13;
    public static readonly int lower_lip = 14;
    public static readonly int upper_outer_lip = 12;
    public static readonly int mouth_corner_left = 291;
    public static readonly int mouth_corner_right = 61;
    public static readonly int lowest_chin = 152;
    public static readonly int upper_head = 10;
    public static readonly int mouth_frown_left = 422;
    public static readonly int mouth_frown_right = 202;
    public static readonly int mouth_left_stretch = 287;
    public static readonly int mouth_right_stretch = 57;
    public static readonly int lowest_lip = 17;
    public static readonly int under_lip = 18;
    public static readonly int over_upper_lip = 164;
    public static readonly int[] left_upper_press = { 40, 80 };
    public static readonly int[] left_lower_press = { 88, 91 };
    public static readonly int[] right_upper_press = { 270, 310 };
    public static readonly int[] right_lower_press = { 318, 321 };
    public static readonly int[] squint_left = { 253, 450 };
    public static readonly int[] squint_right = { 23, 230 };
    public static readonly int right_brow = 27;
    public static readonly int[] right_brow_lower = { 53, 52, 65 };
    public static readonly int left_brow = 257;
    public static readonly int[] left_brow_lower = { 283, 282, 295 };
    public static readonly int inner_brow = 9;
    public static readonly int upper_nose = 6;
    public static readonly int[] cheek_squint_left = { 359, 342 };
    public static readonly int[] cheek_squint_right = { 130, 113 };
    
    public enum FaceBlendShape
    {
        EyeBlinkLeft,
        EyeLookDownLeft,
        EyeLookInLeft,
        EyeLookOutLeft,
        EyeLookUpLeft,
        EyeSquintLeft,
        EyeWideLeft,
        EyeBlinkRight,
        EyeLookDownRight ,
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
        MouthLowerDownRight ,
        MouthUpperUpLeft,
        MouthUpperUpRight,
        BrowDownLeft,
        BrowDownRight,
        BrowInnerUp,
        BrowOuterUpLeft,
        BrowOuterUpRight,
        CheekPuff,
        CheekSquintLeft ,
        CheekSquintRight,
        NoseSneerLeft,
        NoseSneerRight,
        TongueOut,
        HeadYaw,
        HeadPitch,
        HeadRoll,
        LeftEyeYaw,
        LeftEyePitch,
        LeftEyeRoll,
        RightEyeYaw ,
        RightEyePitch ,
        RightEyeRoll 
    }

    public static Dictionary<FaceBlendShape, Vector2> remap_config = new()
    {
        { FaceBlendShape.EyeBlinkLeft, new Vector2(0.4f, 0.7f) },
        { FaceBlendShape.EyeSquintLeft, new Vector2(0.37f,0.44f)},
        { FaceBlendShape.EyeWideLeft, new Vector2(0.9f,1.2f)},
        { FaceBlendShape.EyeBlinkRight, new Vector2(0.4f,0.7f)},
        { FaceBlendShape.EyeSquintRight, new Vector2(0.37f,0.44f)},
        { FaceBlendShape.EyeWideRight, new Vector2(0.9f,1.2f)},
        { FaceBlendShape.JawLeft , new Vector2(-0.4f,0.0f)},
        { FaceBlendShape.JawRight, new Vector2(0.0f,0.4f)},
        { FaceBlendShape.JawOpen, new Vector2(0.50f,0.55f)},
        { FaceBlendShape.MouthClose , new Vector2(3.0f,4.5f)},
        { FaceBlendShape.MouthFunnel, new Vector2(4.0f,4.8f)},
        { FaceBlendShape.MouthPucker, new Vector2(3.46f, 4.92f)},
        { FaceBlendShape.MouthLeft, new Vector2(-3.4f, -2.3f)},
        { FaceBlendShape.MouthRight, new Vector2(1.5f, 3.0f)},
        { FaceBlendShape.MouthSmileLeft, new Vector2(-0.25f, 0.0f)},
        { FaceBlendShape.MouthSmileRight, new Vector2(-0.25f, 0.0f)},
        { FaceBlendShape.MouthStretchLeft, new Vector2(-0.4f,0.0f)},
        { FaceBlendShape.MouthStretchRight, new Vector2(-0.4f,0.0f)},
        { FaceBlendShape.MouthRollLower, new Vector2(0.4f,0.7f)},
        { FaceBlendShape.MouthRollUpper, new Vector2(0.31f,0.34f)},
        { FaceBlendShape.MouthShrugLower, new Vector2(1.9f, 2.3f)},
        { FaceBlendShape.MouthShrugUpper, new Vector2(1.4f, 2.4f)},
        { FaceBlendShape.MouthPressLeft, new Vector2(0.4f,0.5f)},
        { FaceBlendShape.MouthPressRight, new Vector2(0.4f,0.5f)},
        { FaceBlendShape.MouthLowerDownLeft, new Vector2(1.7f,2.1f)},
        { FaceBlendShape.MouthLowerDownRight, new Vector2(1.7f,2.1f)},
        { FaceBlendShape.BrowDownLeft, new Vector2(1.0f,1.2f)},
        { FaceBlendShape.BrowDownRight, new Vector2(1.0f,1.2f)},
        { FaceBlendShape.BrowInnerUp, new Vector2(2.2f, 2.6f)},
        { FaceBlendShape.BrowOuterUpLeft, new Vector2(1.25f, 1.5f)},
        { FaceBlendShape.BrowOuterUpRight, new Vector2(1.25f, 1.5f)},
        { FaceBlendShape.CheekSquintLeft, new Vector2(0.55f,0.63f)},
        { FaceBlendShape.CheekSquintRight, new Vector2(0.55f,0.63f)}
    };
}
