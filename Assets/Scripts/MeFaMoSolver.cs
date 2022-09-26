using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using FaceBlendShape = MeFaMoConfig.FaceBlendShape;

public class MeFaMoSolver : MonoBehaviour
{

    [SerializeField] PointsTemplate faceMesh;
    public Dictionary<FaceBlendShape, float> blendShape = new();


    void Update()
    {
        CalculateMouthLandmark();
        CalculateEyeLandmark();
    }

    float Remap(FaceBlendShape blendShape, float value)
    {
        var minMax = MeFaMoConfig.remap_config[blendShape];
        var min = minMax[0];
        var max = minMax[1];
        return Remap(value, min, max);
    }

    float Remap(float value, float min, float max)
    {
        var clamped = Mathf.Clamp(value, min, max);
        return (clamped - min) / (max - min);
    }
    
    void CalculateMouthLandmark()
    {
        var landmarks = faceMesh.points;
        if (landmarks == null || landmarks.Length < 468) return;

        var upper_lip = landmarks[MeFaMoConfig.upper_lip];
        var upper_outer_lip = landmarks[MeFaMoConfig.upper_outer_lip];
        var lower_lip = landmarks[MeFaMoConfig.lower_lip];

        var mouth_corner_left = landmarks[MeFaMoConfig.mouth_corner_left];
        var mouth_corner_right = landmarks[MeFaMoConfig.mouth_corner_right];
        var lowest_chin = landmarks[MeFaMoConfig.lowest_chin];
        var nose_tip = landmarks[MeFaMoConfig.nose_tip];
        var upper_head = landmarks[MeFaMoConfig.upper_head];

        var mouth_width = (mouth_corner_left - mouth_corner_right).magnitude;
        var mouth_center = (upper_lip + lower_lip) / 2;
        var mouth_open_dist = (upper_lip - lower_lip).magnitude;
        var mouth_center_nose_dist = (mouth_center - nose_tip).magnitude;

        var jaw_nose_dist = (lowest_chin - nose_tip).magnitude;
        var head_height = (upper_head - nose_tip).magnitude;
        var jaw_open_ratio = jaw_nose_dist / head_height;

        var jaw_open = Remap(FaceBlendShape.JawOpen, jaw_open_ratio);
        blendShape[FaceBlendShape.JawOpen] = jaw_open;

        var mouth_open = Remap(FaceBlendShape.MouthClose, mouth_center_nose_dist - mouth_open_dist);
        blendShape[FaceBlendShape.MouthClose] = mouth_open;

        var smile_left = upper_lip.x - mouth_corner_left.x;
        var smile_right = upper_lip.x - mouth_corner_right.x;

        var mouth_smile_left = 1 - Remap(FaceBlendShape.MouthSmileLeft, smile_left);
        var mouth_smile_right = 1 - Remap(FaceBlendShape.MouthSmileRight, smile_right);

        blendShape[FaceBlendShape.MouthSmileLeft] = mouth_smile_left;
        blendShape[FaceBlendShape.MouthSmileRight] = mouth_smile_right;
        blendShape[FaceBlendShape.MouthDimpleLeft] = mouth_smile_left / 2;
        blendShape[FaceBlendShape.MouthDimpleRight] = mouth_smile_right / 2;

        var mouth_frown_left = (mouth_corner_left - landmarks[MeFaMoConfig.mouth_frown_left]).x;
        var mouth_frown_right = (mouth_corner_right - landmarks[MeFaMoConfig.mouth_frown_right]).x;

        blendShape[FaceBlendShape.MouthFrownLeft] = 1 - Remap(FaceBlendShape.MouthFrownLeft, mouth_frown_left);
        blendShape[FaceBlendShape.MouthFrownRight] = 1 - Remap(FaceBlendShape.MouthFrownRight, mouth_frown_right);

        var mouth_left_stretch_point = landmarks[MeFaMoConfig.mouth_left_stretch];
        var mouth_right_stretch_point = landmarks[MeFaMoConfig.mouth_right_stretch];

        var mouth_left_stretch = mouth_corner_left.x - mouth_left_stretch_point.x;
        var mouth_right_stretch = mouth_right_stretch_point.x - mouth_corner_right.x;
        var mouth_center_left_stretch = mouth_center.x - mouth_left_stretch_point.x;
        var mouth_center_right_stretch = mouth_center.x - mouth_right_stretch_point.x;

        var mouth_left = Remap(FaceBlendShape.MouthLeft, mouth_center_left_stretch);
        var mouth_right = 1 - Remap(FaceBlendShape.MouthRight, mouth_center_right_stretch);

        blendShape[FaceBlendShape.MouthLeft] = mouth_left;
        blendShape[FaceBlendShape.MouthRight] = mouth_right;

        var stretch_normal_left = -0.7f + (0.42f * mouth_smile_left) + (0.36f * mouth_left);
        var stretch_max_left = -0.45f + (0.45f * mouth_smile_left) + (0.36f * mouth_left);
        
        var stretch_normal_right = -0.7f + (0.42f * mouth_smile_right) + (0.36f * mouth_right); // Fixed. different from original formula
        var stretch_max_right = -0.45f + (0.45f * mouth_smile_right) + (0.36f * mouth_right);

        blendShape[FaceBlendShape.MouthStretchLeft] = Remap(mouth_left_stretch, stretch_normal_left, stretch_max_left);
        blendShape[FaceBlendShape.MouthStretchRight] = Remap(mouth_right_stretch, stretch_normal_right, stretch_max_right);


        var uppest_lip = landmarks[0];
        var jaw_right_left = nose_tip.x - lowest_chin.x;

        blendShape[FaceBlendShape.JawLeft] = 1 - Remap(FaceBlendShape.JawLeft, jaw_right_left);
        blendShape[FaceBlendShape.JawRight] = Remap(FaceBlendShape.JawRight, jaw_right_left);

        var lowest_lip = landmarks[MeFaMoConfig.lowest_lip];
        var under_lip = landmarks[MeFaMoConfig.under_lip];

        var outer_lip_dist = (lower_lip - lowest_lip).magnitude;
        var upper_lip_dist = (upper_lip - upper_outer_lip).magnitude;

        var mouth_pucker = Remap(FaceBlendShape.MouthPucker, mouth_width);

        blendShape[FaceBlendShape.MouthPucker] = 1 - mouth_pucker;
        blendShape[FaceBlendShape.MouthRollLower] = 1 - Remap(FaceBlendShape.MouthRollLower, outer_lip_dist);
        blendShape[FaceBlendShape.MouthRollUpper] = 1 - Remap(FaceBlendShape.MouthRollUpper, upper_lip_dist);

        var upper_lip_nose_dist = nose_tip.y - uppest_lip.y;
        blendShape[FaceBlendShape.MouthShrugUpper] = 1 - Remap(FaceBlendShape.MouthShrugUpper, upper_lip_nose_dist);

        var over_upper_lip = landmarks[MeFaMoConfig.over_upper_lip];
        var mouth_shrug_lower = (lowest_lip - over_upper_lip).magnitude;

        blendShape[FaceBlendShape.MouthShrugLower] = 1 - Remap(FaceBlendShape.MouthShrugLower, mouth_shrug_lower);

        var lower_down_left = (landmarks[424] - landmarks[319]).magnitude + mouth_open_dist * 0.5f;
        var lower_down_right = (landmarks[204] - landmarks[89]).magnitude + mouth_open_dist * 0.5f;

        blendShape[FaceBlendShape.MouthLowerDownLeft] = 1 - Remap(FaceBlendShape.MouthLowerDownLeft, lower_down_left);
        blendShape[FaceBlendShape.MouthLowerDownRight] = 1 - Remap(FaceBlendShape.MouthLowerDownRight, lower_down_right);

        if (blendShape[FaceBlendShape.MouthPucker] < 0.5f)
        {
            blendShape[FaceBlendShape.MouthFunnel] = 1 - Remap(FaceBlendShape.MouthFunnel, mouth_width);
        }
        else
        {
            blendShape[FaceBlendShape.MouthFunnel] = 0;
        }

        var left_upper_press =
            (landmarks[MeFaMoConfig.left_upper_press[0]] - landmarks[MeFaMoConfig.left_upper_press[1]]).magnitude;
        var left_lower_press =
            (landmarks[MeFaMoConfig.left_lower_press[0]] - landmarks[MeFaMoConfig.left_lower_press[1]]).magnitude;

        var mouth_press_left = (left_upper_press + left_lower_press) / 2;

        var right_upper_press =
            (landmarks[MeFaMoConfig.right_upper_press[0]] - landmarks[MeFaMoConfig.right_upper_press[1]]).magnitude;
        var right_lower_press =
            (landmarks[MeFaMoConfig.right_lower_press[0]] - landmarks[MeFaMoConfig.right_lower_press[1]]).magnitude;

        var mouth_press_right = (right_upper_press + right_lower_press) / 2;

        blendShape[FaceBlendShape.MouthPressLeft] = 1 - Remap(FaceBlendShape.MouthPressLeft, mouth_press_left);
        blendShape[FaceBlendShape.MouthPressRight] = 1 - Remap(FaceBlendShape.MouthPressRight, mouth_press_right);
    }

    float GetEyeLidDistance(int[] eyePoints)
    {
        var landmarks = faceMesh.points;
        if (landmarks == null || landmarks.Length < 468) return Single.NaN;

        var eye_width = (landmarks[eyePoints[0]] - landmarks[eyePoints[1]]).magnitude;
        var eye_outer_lid = (landmarks[eyePoints[2]] - landmarks[eyePoints[5]]).magnitude;
        var eye_mid_lid = (landmarks[eyePoints[3]] - landmarks[eyePoints[6]]).magnitude;
        var eye_inner_lid = (landmarks[eyePoints[4]] - landmarks[eyePoints[7]]).magnitude;
        var eye_lid_avg = (eye_outer_lid + eye_mid_lid + eye_inner_lid) / 3;
        var ratio = eye_lid_avg / eye_width;
        return ratio;
    }

    float GetEyeOpenRatio(int[] points)
    {
        var eye_distance = GetEyeLidDistance(points);
        var max_ratio = 0.285f;
        return Mathf.Clamp(eye_distance / max_ratio, 0, 2);
    }

    void CalculateEyeLandmark()
    {
        var landmarks = faceMesh.points;
        if (landmarks == null || landmarks.Length < 468) return;

        var eye_open_ratio_left = GetEyeOpenRatio(MeFaMoConfig.eye_left);
        var eye_open_ratio_right = GetEyeOpenRatio(MeFaMoConfig.eye_right);

        var blink_left = 1 - Remap(FaceBlendShape.EyeBlinkLeft, eye_open_ratio_left);
        var blink_right = 1 - Remap(FaceBlendShape.EyeBlinkRight, eye_open_ratio_right);

        blendShape[FaceBlendShape.EyeBlinkLeft] = blink_left;
        blendShape[FaceBlendShape.EyeBlinkRight] = blink_right;

        blendShape[FaceBlendShape.EyeWideLeft] = Remap(FaceBlendShape.EyeWideLeft, eye_open_ratio_left);
        blendShape[FaceBlendShape.EyeWideRight] = Remap(FaceBlendShape.EyeWideRight, eye_open_ratio_right);

        var squint_left = (landmarks[MeFaMoConfig.squint_left[0]] - landmarks[MeFaMoConfig.squint_left[1]]).magnitude;
        blendShape[FaceBlendShape.EyeSquintLeft] = 1 - Remap(FaceBlendShape.EyeSquintLeft, squint_left);
        
        var squint_right = (landmarks[MeFaMoConfig.squint_right[0]] - landmarks[MeFaMoConfig.squint_right[1]]).magnitude;
        blendShape[FaceBlendShape.EyeSquintRight] = 1 - Remap(FaceBlendShape.EyeSquintRight, squint_right);

        var right_brow_lower =
            (landmarks[MeFaMoConfig.right_brow_lower[0]] 
             + landmarks[MeFaMoConfig.right_brow_lower[1]] 
             + landmarks[MeFaMoConfig.right_brow_lower[2]]) / 3;
        var right_brow_dist = (landmarks[MeFaMoConfig.right_brow] - right_brow_lower).magnitude;
        
        var left_brow_lower =
            (landmarks[MeFaMoConfig.left_brow_lower[0]] 
             + landmarks[MeFaMoConfig.left_brow_lower[1]] 
             + landmarks[MeFaMoConfig.left_brow_lower[2]]) / 3;
        var left_brow_dist = (landmarks[MeFaMoConfig.left_brow] - left_brow_lower).magnitude;

        blendShape[FaceBlendShape.BrowDownLeft] = 1 - Remap(FaceBlendShape.BrowDownLeft, left_brow_dist);
        blendShape[FaceBlendShape.BrowOuterUpLeft] = Remap(FaceBlendShape.BrowOuterUpLeft, left_brow_dist);
        
        blendShape[FaceBlendShape.BrowDownRight] = 1 - Remap(FaceBlendShape.BrowDownRight, right_brow_dist);
        blendShape[FaceBlendShape.BrowOuterUpRight] = Remap(FaceBlendShape.BrowOuterUpRight, right_brow_dist);

        var inner_brow = landmarks[MeFaMoConfig.inner_brow];
        var upper_nose = landmarks[MeFaMoConfig.upper_nose];
        var inner_brow_dist = (upper_nose - inner_brow).magnitude;

        blendShape[FaceBlendShape.BrowInnerUp] = Remap(FaceBlendShape.BrowInnerUp, inner_brow_dist);

        var cheek_squint_left = (landmarks[MeFaMoConfig.cheek_squint_left[0]]
                                 - landmarks[MeFaMoConfig.cheek_squint_left[1]]).magnitude;
        var cheek_squint_right = (landmarks[MeFaMoConfig.cheek_squint_right[0]]
                                  - landmarks[MeFaMoConfig.cheek_squint_right[1]]).magnitude;
        blendShape[FaceBlendShape.CheekSquintLeft] = 1 - Remap(FaceBlendShape.CheekSquintLeft, cheek_squint_left);
        blendShape[FaceBlendShape.CheekSquintRight] = 1 - Remap(FaceBlendShape.CheekSquintRight, cheek_squint_right);

        blendShape[FaceBlendShape.NoseSneerLeft] = blendShape[FaceBlendShape.CheekSquintLeft];
        blendShape[FaceBlendShape.NoseSneerRight] = blendShape[FaceBlendShape.CheekSquintRight];

    }
}

