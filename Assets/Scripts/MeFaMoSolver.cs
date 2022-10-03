using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

using FaceBlendShape = MeFaMoConfig.FaceBlendShape;
internal class NormalsPerVtx
{
    public List<Vector3> normals= new ();
}
public class MeFaMoSolver : MonoBehaviour
{

    [SerializeField] PointsTemplate faceMesh;
    public Dictionary<FaceBlendShape, float> blendShape = new();
    public bool isNormalize = false;
    public SkinnedMeshRenderer blendShapeRenderer;
    Action uiUpdater;

    Vector3[] m_landmarks = new Vector3[478];
    Vector2[] m_uv;
    int[] m_triangle;
    NormalsPerVtx[] m_normalsPerVertices;
    Vector3[] m_normals;
    
    Mesh m_mesh;
    Vector3[] m_vertices = new Vector3[468];
    void Start()
    {
        m_uv = new Vector2[FaceMeshTopology.UV.Length/2];
        for (var i = 0; i < m_uv.Length; i++)
        {
            m_uv[i] = new Vector2((float)FaceMeshTopology.UV[i * 2], (float)FaceMeshTopology.UV[i * 2 + 1]);
        }

        m_triangle = new int[FaceMeshTopology.TriangleIndices.Length];
        
      
        for (var i = 0; i < m_triangle.Length / 3; i++)
        {
        
            m_triangle[i * 3] = FaceMeshTopology.TriangleIndices[i * 3 +1];
            m_triangle[i * 3 + 1] = FaceMeshTopology.TriangleIndices[i * 3];
            m_triangle[i * 3 + 2] = FaceMeshTopology.TriangleIndices[i * 3 + 2];
        }
        
        m_normalsPerVertices = new NormalsPerVtx[m_vertices.Length];

        for (var i = 0; i < m_vertices.Length; i++)
        {
            m_normalsPerVertices[i] = new NormalsPerVtx();
            
        }
        
        m_normals = new Vector3[m_vertices.Length];
        
        m_mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = m_mesh;

        var m = Matrix<double>.Build.Random(3, 4);
        
    }
    void Update()
    {
        if (faceMesh.points == null || faceMesh.points.Length < 468) return;

        for (int i = 0; i < m_landmarks.Length; i++)
        {
            m_landmarks[i] = faceMesh.points[i];
        }
        
        //if(isNormalize) NormalizeHeadPose();
        
        SolvePose();
        UpdateMesh();
        CalculateMouthLandmark2();
        CalculateEyeLandmark2();
        if (uiUpdater != null)
        {
            uiUpdater();
        }

        foreach (var pair in blendShape)
        {
            string bsName = pair.Key + "";
            bsName = char.ToLower(bsName.First())+bsName.Substring(1);
            var index = blendShapeRenderer.sharedMesh.GetBlendShapeIndex(bsName);
            if (index >= 0)
            {
                blendShapeRenderer.SetBlendShapeWeight(index,pair.Value*100);
            }
        }
    }
    
    public void SetInspectorCallback(Action action)
    {
        uiUpdater = action;
    }

    void SolvePose()
    {
        var frameWidth = 640;
        var frameHeight = 480;
        var focalLength = 640;
        var pcf = new PerspectiveCameraFrustum(frameWidth, frameHeight, focalLength);
        var faceGeo = new FaceGeometry(m_landmarks, pcf);
        m_landmarks = faceGeo.GetMetricLandmarks();
    }
    void NormalizeHeadPose()
    {
        var y_axis = (m_landmarks[10] - m_landmarks[152]).normalized;
        var x_axis = (m_landmarks[454] - m_landmarks[234]).normalized;
        var z_axis = Vector3.Cross(x_axis, y_axis);
        var headRot = Quaternion.LookRotation(z_axis, y_axis);
        var invHeadRot = Quaternion.Inverse(headRot);
        for (int i = 0; i < m_landmarks.Length; i++)
        {
            m_landmarks[i] = invHeadRot * m_landmarks[i];
        }

        var scale = (m_landmarks[454] - m_landmarks[234]).magnitude*3.0f;
        
        
        Vector3 avg = Vector3.zero; 
        for (int i = 0; i < m_landmarks.Length; i++)
        {
            avg += m_landmarks[i];
        }

        avg /= m_landmarks.Length;

        for (int i = 0; i < m_landmarks.Length; i++)
        {
            m_landmarks[i] -= avg;
            m_landmarks[i] /= scale;
        }
    }

    void UpdateMesh()
    {
        for (int i = 0; i < 468; i++)
        {
            m_vertices[i] = m_landmarks[i];
        }
        m_mesh.Clear();
        m_mesh.vertices = m_vertices;
        m_mesh.uv = m_uv;
        m_mesh.triangles = m_triangle;
        m_mesh.RecalculateNormals();

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

    void CalculateMouthLandmark2()
    {
        var upper_lip = m_landmarks[MeFaMoConfig.upper_lip];
        var upper_outer_lip = m_landmarks[MeFaMoConfig.upper_outer_lip];
        var lower_lip = m_landmarks[MeFaMoConfig.lower_lip];

        var mouth_corner_left = m_landmarks[MeFaMoConfig.mouth_corner_left];
        var mouth_corner_right = m_landmarks[MeFaMoConfig.mouth_corner_right];
        var lowest_chin = m_landmarks[MeFaMoConfig.lowest_chin];
        var nose_tip = m_landmarks[MeFaMoConfig.nose_tip];
        var upper_head = m_landmarks[MeFaMoConfig.upper_head];

        var mouth_width = (mouth_corner_left - mouth_corner_right).magnitude;
        var mouth_center = (upper_lip + lower_lip) / 2;
        var mouth_open_dist = upper_lip.y - lower_lip.y;
        var mouth_center_nose_dist = (mouth_center - nose_tip).magnitude;

        var jaw_nose_dist = (lowest_chin - nose_tip).magnitude;
        var head_height = (upper_head - lowest_chin).magnitude;
        var jaw_open_ratio = jaw_nose_dist / head_height;
        var jaw_open = Remap(FaceBlendShape.JawOpen, jaw_open_ratio);
        
        
        
        var mouth_open = Remap(FaceBlendShape.MouthClose, mouth_open_dist/mouth_center_nose_dist);
        blendShape[FaceBlendShape.MouthClose] = 0;
        blendShape[FaceBlendShape.JawOpen] = mouth_open;
        
        var eye_right_center = (m_landmarks[MeFaMoConfig.eye_right[0]] + m_landmarks[MeFaMoConfig.eye_right[1]]) / 2;
        var eye_left_center = (m_landmarks[MeFaMoConfig.eye_left[0]] + m_landmarks[MeFaMoConfig.eye_left[1]]) / 2;
        var eye_distance = (eye_right_center - eye_left_center).magnitude;
        var mouth_left_half = Mathf.Abs(nose_tip.x - mouth_corner_left.x) / eye_distance;
        var mouth_right_half = Mathf.Abs(nose_tip.x - mouth_corner_right.x) / eye_distance;

        var mouth_width_ratio = mouth_width / eye_distance;
        var mouth_gradient_left = (mouth_corner_left.y - mouth_center.y) / (mouth_corner_left.x - mouth_center.x);
        var mouth_gradient_right = - (mouth_corner_right.y - mouth_center.y) / (mouth_corner_right.x - mouth_center.x);
        
        var mouth_skew = (mouth_center.x - nose_tip.x)/eye_distance;

        
        blendShape[FaceBlendShape.MouthSmileLeft] = 0;
        blendShape[FaceBlendShape.MouthStretchLeft] = 0;
        blendShape[FaceBlendShape.MouthSmileRight] = 0;
        blendShape[FaceBlendShape.MouthStretchRight] = 0;
        if (mouth_left_half > 0.4f)
        {
            var value = Remap(mouth_left_half, 0.4f, 0.5f);
            if (mouth_gradient_left > 0.0f)
            {
                var smile = 0.5f+0.5f*Remap(mouth_gradient_left, 0.0f, 0.2f);
                var strecth = 0.5f * (1 - Remap(mouth_gradient_left, 0.0f, 0.2f));
                blendShape[FaceBlendShape.MouthSmileLeft] = smile*value;
                blendShape[FaceBlendShape.MouthStretchLeft] = strecth*value;
                
            }
            else
            {
                var smile = 0.5f*Remap(mouth_gradient_left, -0.3f, 0.0f);
                var strecth = 0.5f+ 0.5f * (1 - Remap(mouth_gradient_left, -0.3f, 0.0f));
                blendShape[FaceBlendShape.MouthSmileLeft] = smile*value;
                blendShape[FaceBlendShape.MouthStretchLeft] = strecth*value;
            }
        }
        ;
        if (mouth_right_half > 0.4f)
        {
            var value = Remap(mouth_right_half, 0.4f, 0.5f);
            if (mouth_gradient_right > 0.0f)
            {
                var smile = 0.5f+0.5f*Remap(mouth_gradient_right, 0.0f, 0.2f);
                var strecth = 0.5f * (1 - Remap(mouth_gradient_right, 0.0f, 0.2f));
                blendShape[FaceBlendShape.MouthSmileRight] = smile * value;
                blendShape[FaceBlendShape.MouthStretchRight] = strecth * value;

            }
            else
            {
                var smile = 0.5f*Remap(mouth_gradient_right, -0.3f, 0.0f);
                var strecth = 0.5f+ 0.5f * (1 - Remap(mouth_gradient_right, -0.3f, 0.0f));
                blendShape[FaceBlendShape.MouthSmileRight] = smile*value;
                blendShape[FaceBlendShape.MouthStretchRight] = strecth*value;
            }
        }

        if (mouth_width_ratio < 0.7f)
        {
            blendShape[FaceBlendShape.MouthPucker] = 1- Remap(mouth_width_ratio, 0.5f, 0.7f);
            blendShape[FaceBlendShape.MouthFunnel] = mouth_open;
        }
        else
        {
            blendShape[FaceBlendShape.MouthPucker] = 0.0f;
            blendShape[FaceBlendShape.MouthFunnel] = 0.0f;
        }

        blendShape[FaceBlendShape.MouthLeft] = Remap(mouth_skew, 0, 0.2f);
        blendShape[FaceBlendShape.MouthRight] = 1.0f - Remap(mouth_skew, -0.2f, 0f);

        blendShape[FaceBlendShape.JawLeft] = blendShape[FaceBlendShape.MouthLeft];
        blendShape[FaceBlendShape.JawRight] = blendShape[FaceBlendShape.MouthRight];

    }
    void CalculateMouthLandmark()
    {
        var upper_lip = m_landmarks[MeFaMoConfig.upper_lip];
        var upper_outer_lip = m_landmarks[MeFaMoConfig.upper_outer_lip];
        var lower_lip = m_landmarks[MeFaMoConfig.lower_lip];

        var mouth_corner_left = m_landmarks[MeFaMoConfig.mouth_corner_left];
        var mouth_corner_right = m_landmarks[MeFaMoConfig.mouth_corner_right];
        var lowest_chin = m_landmarks[MeFaMoConfig.lowest_chin];
        var nose_tip = m_landmarks[MeFaMoConfig.nose_tip];
        var upper_head = m_landmarks[MeFaMoConfig.upper_head];

        var mouth_width = (mouth_corner_left - mouth_corner_right).magnitude;
        var mouth_center = (upper_lip + lower_lip) / 2;
        var mouth_open_dist = (upper_lip - lower_lip).magnitude;
        var mouth_center_nose_dist = (mouth_center - nose_tip).magnitude;

        var jaw_nose_dist = (lowest_chin - nose_tip).magnitude;
        var head_height = (upper_head - lowest_chin).magnitude;
        var jaw_open_ratio = jaw_nose_dist / head_height;
        var jaw_open = Remap(FaceBlendShape.JawOpen, jaw_open_ratio);
        blendShape[FaceBlendShape.JawOpen] = 1.0f;//jaw_open;
        
        var mouth_open = Remap(FaceBlendShape.MouthClose, mouth_open_dist/mouth_center_nose_dist);
        blendShape[FaceBlendShape.MouthClose] = Remap(1-mouth_open,0,jaw_open);
        
        //Debug.Log("Jaw, mouth "+jaw_open+" "+(mouth_open_dist)/mouth_center_nose_dist);
         var smile_left = upper_lip.y - mouth_corner_left.y;
         var smile_right = upper_lip.y - mouth_corner_right.y;

        //var smile_left = (mouth_center.y - mouth_corner_left.y) / (mouth_center.x - mouth_corner_left.x);
        //var smile_right = (mouth_center.y - mouth_corner_right.y) / (mouth_center.x - mouth_corner_right.x);

        //Debug.Log("smile "+smile_left+" "+smile_right);
//        Debug.Log("test Left " + mouth_corner_left);
        var mouth_smile_left = 1 - Remap(FaceBlendShape.MouthSmileLeft, smile_left);
        var mouth_smile_right = 1 - Remap(FaceBlendShape.MouthSmileRight, smile_right);

        blendShape[FaceBlendShape.MouthSmileLeft] = mouth_smile_left;
        blendShape[FaceBlendShape.MouthSmileRight] = mouth_smile_right;
        blendShape[FaceBlendShape.MouthDimpleLeft] = mouth_smile_left / 2;
        blendShape[FaceBlendShape.MouthDimpleRight] = mouth_smile_right / 2;

        var mouth_frown_left = (mouth_corner_left - m_landmarks[MeFaMoConfig.mouth_frown_left]).y;
        var mouth_frown_right = (mouth_corner_right - m_landmarks[MeFaMoConfig.mouth_frown_right]).y;

        blendShape[FaceBlendShape.MouthFrownLeft] = 1 - Remap(FaceBlendShape.MouthFrownLeft, mouth_frown_left);
        blendShape[FaceBlendShape.MouthFrownRight] = 1 - Remap(FaceBlendShape.MouthFrownRight, mouth_frown_right);

        var mouth_left_stretch_point = m_landmarks[MeFaMoConfig.mouth_left_stretch];
        var mouth_right_stretch_point = m_landmarks[MeFaMoConfig.mouth_right_stretch];

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


        var uppest_lip = m_landmarks[0];
        var jaw_right_left = nose_tip.x - lowest_chin.x;

        blendShape[FaceBlendShape.JawLeft] = 1 - Remap(FaceBlendShape.JawLeft, jaw_right_left);
        blendShape[FaceBlendShape.JawRight] = Remap(FaceBlendShape.JawRight, jaw_right_left);

        var lowest_lip = m_landmarks[MeFaMoConfig.lowest_lip];
        var under_lip = m_landmarks[MeFaMoConfig.under_lip];

        var outer_lip_dist = (lower_lip - lowest_lip).magnitude;
        var upper_lip_dist = (upper_lip - upper_outer_lip).magnitude;

        var mouth_pucker = Remap(FaceBlendShape.MouthPucker, mouth_width);

        blendShape[FaceBlendShape.MouthPucker] = 1 - mouth_pucker;
        blendShape[FaceBlendShape.MouthRollLower] = 1 - Remap(FaceBlendShape.MouthRollLower, outer_lip_dist);
        blendShape[FaceBlendShape.MouthRollUpper] = 1 - Remap(FaceBlendShape.MouthRollUpper, upper_lip_dist);

        var upper_lip_nose_dist = nose_tip.y - uppest_lip.y;
        blendShape[FaceBlendShape.MouthShrugUpper] = 1 - Remap(FaceBlendShape.MouthShrugUpper, upper_lip_nose_dist);

        var over_upper_lip = m_landmarks[MeFaMoConfig.over_upper_lip];
        var mouth_shrug_lower = (lowest_lip - over_upper_lip).magnitude;

        blendShape[FaceBlendShape.MouthShrugLower] = 1 - Remap(FaceBlendShape.MouthShrugLower, mouth_shrug_lower);

        var lower_down_left = (m_landmarks[424] - m_landmarks[319]).magnitude + mouth_open_dist * 0.5f;
        var lower_down_right = (m_landmarks[204] - m_landmarks[89]).magnitude + mouth_open_dist * 0.5f;

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
            (m_landmarks[MeFaMoConfig.left_upper_press[0]] - m_landmarks[MeFaMoConfig.left_upper_press[1]]).magnitude;
        var left_lower_press =
            (m_landmarks[MeFaMoConfig.left_lower_press[0]] - m_landmarks[MeFaMoConfig.left_lower_press[1]]).magnitude;

        var mouth_press_left = (left_upper_press + left_lower_press) / 2;

        var right_upper_press =
            (m_landmarks[MeFaMoConfig.right_upper_press[0]] - m_landmarks[MeFaMoConfig.right_upper_press[1]]).magnitude;
        var right_lower_press =
            (m_landmarks[MeFaMoConfig.right_lower_press[0]] - m_landmarks[MeFaMoConfig.right_lower_press[1]]).magnitude;

        var mouth_press_right = (right_upper_press + right_lower_press) / 2;

        blendShape[FaceBlendShape.MouthPressLeft] = 1 - Remap(FaceBlendShape.MouthPressLeft, mouth_press_left);
        blendShape[FaceBlendShape.MouthPressRight] = 1 - Remap(FaceBlendShape.MouthPressRight, mouth_press_right);
    }

    float GetEyeLidDistance(int[] eyePoints)
    {
        var eye_width = (m_landmarks[eyePoints[0]] - m_landmarks[eyePoints[1]]).magnitude;
        var eye_outer_lid = (m_landmarks[eyePoints[2]] - m_landmarks[eyePoints[5]]).magnitude;
        var eye_mid_lid = (m_landmarks[eyePoints[3]] - m_landmarks[eyePoints[6]]).magnitude;
        var eye_inner_lid = (m_landmarks[eyePoints[4]] - m_landmarks[eyePoints[7]]).magnitude;
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

    void CalculateEyeLandmark2()
    {
        var eye_open_ratio_left = GetEyeOpenRatio(MeFaMoConfig.eye_left);
        var eye_open_ratio_right = GetEyeOpenRatio(MeFaMoConfig.eye_right);
        
        var blink_left = 1 - Remap(eye_open_ratio_left,0.25f,0.80f);
        var blink_right = 1 - Remap(eye_open_ratio_right,0.25f,0.80f);
        var left_eye_center = (m_landmarks[MeFaMoConfig.eye_left[0]] + m_landmarks[MeFaMoConfig.eye_left[1]]) / 2;
        var right_eye_center = (m_landmarks[MeFaMoConfig.eye_right[0]] + m_landmarks[MeFaMoConfig.eye_right[1]]) / 2;
        blendShape[FaceBlendShape.EyeBlinkLeft] = blink_left;
        blendShape[FaceBlendShape.EyeBlinkRight] = blink_right;
        blendShape[FaceBlendShape.EyeSquintLeft] = blendShape[FaceBlendShape.MouthSmileLeft]*0.7f;
        blendShape[FaceBlendShape.EyeSquintRight] = blendShape[FaceBlendShape.MouthSmileRight]*0.7f;

        var left_brow_dist = m_landmarks[MeFaMoConfig.left_brow_top].y - left_eye_center.y;
        var right_brow_dist = m_landmarks[MeFaMoConfig.right_brow_top].y - right_eye_center.y;

        Debug.Log(left_brow_dist + " " + right_brow_dist);

        blendShape[FaceBlendShape.BrowOuterUpLeft] = Remap(left_brow_dist, 2.9f, 3.4f);
        blendShape[FaceBlendShape.BrowOuterUpRight] = Remap(right_brow_dist, 2.9f, 3.4f);
        blendShape[FaceBlendShape.BrowInnerUp] =
            (blendShape[FaceBlendShape.BrowOuterUpLeft] + blendShape[FaceBlendShape.BrowOuterUpRight]) / 2;
        blendShape[FaceBlendShape.BrowDownLeft] = 1- Remap(left_brow_dist, 2.35f, 2.5f);
        blendShape[FaceBlendShape.BrowDownRight] = 1- Remap(right_brow_dist, 2.35f, 2.5f);

        blendShape[FaceBlendShape.EyeWideLeft] = blendShape[FaceBlendShape.BrowOuterUpLeft];
        blendShape[FaceBlendShape.EyeWideRight] = blendShape[FaceBlendShape.BrowOuterUpRight];
    }
    void CalculateEyeLandmark()
    {
        var eye_open_ratio_left = GetEyeOpenRatio(MeFaMoConfig.eye_left);
        var eye_open_ratio_right = GetEyeOpenRatio(MeFaMoConfig.eye_right);

        var blink_left = 1 - Remap(FaceBlendShape.EyeBlinkLeft, eye_open_ratio_left);
        var blink_right = 1 - Remap(FaceBlendShape.EyeBlinkRight, eye_open_ratio_right);

        blendShape[FaceBlendShape.EyeBlinkLeft] = blink_left;
        blendShape[FaceBlendShape.EyeBlinkRight] = blink_right;

        blendShape[FaceBlendShape.EyeWideLeft] = Remap(FaceBlendShape.EyeWideLeft, eye_open_ratio_left);
        blendShape[FaceBlendShape.EyeWideRight] = Remap(FaceBlendShape.EyeWideRight, eye_open_ratio_right);

        var squint_left = (m_landmarks[MeFaMoConfig.squint_left[0]] - m_landmarks[MeFaMoConfig.squint_left[1]]).magnitude;
        blendShape[FaceBlendShape.EyeSquintLeft] = 1 - Remap(FaceBlendShape.EyeSquintLeft, squint_left);
        
        var squint_right = (m_landmarks[MeFaMoConfig.squint_right[0]] - m_landmarks[MeFaMoConfig.squint_right[1]]).magnitude;
        blendShape[FaceBlendShape.EyeSquintRight] = 1 - Remap(FaceBlendShape.EyeSquintRight, squint_right);

        var right_brow_lower =
            (m_landmarks[MeFaMoConfig.right_brow_lower[0]] 
             + m_landmarks[MeFaMoConfig.right_brow_lower[1]] 
             + m_landmarks[MeFaMoConfig.right_brow_lower[2]]) / 3;
        var right_brow_dist = (m_landmarks[MeFaMoConfig.right_brow] - right_brow_lower).magnitude;
        
        var left_brow_lower =
            (m_landmarks[MeFaMoConfig.left_brow_lower[0]] 
             + m_landmarks[MeFaMoConfig.left_brow_lower[1]] 
             + m_landmarks[MeFaMoConfig.left_brow_lower[2]]) / 3;
        var left_brow_dist = (m_landmarks[MeFaMoConfig.left_brow] - left_brow_lower).magnitude;

        blendShape[FaceBlendShape.BrowDownLeft] = 1 - Remap(FaceBlendShape.BrowDownLeft, left_brow_dist);
        blendShape[FaceBlendShape.BrowOuterUpLeft] = Remap(FaceBlendShape.BrowOuterUpLeft, left_brow_dist);
        
        blendShape[FaceBlendShape.BrowDownRight] = 1 - Remap(FaceBlendShape.BrowDownRight, right_brow_dist);
        blendShape[FaceBlendShape.BrowOuterUpRight] = Remap(FaceBlendShape.BrowOuterUpRight, right_brow_dist);

        var inner_brow = m_landmarks[MeFaMoConfig.inner_brow];
        var upper_nose = m_landmarks[MeFaMoConfig.upper_nose];
        var inner_brow_dist = (upper_nose - inner_brow).magnitude;

        blendShape[FaceBlendShape.BrowInnerUp] = Remap(FaceBlendShape.BrowInnerUp, inner_brow_dist);

        var cheek_squint_left = (m_landmarks[MeFaMoConfig.cheek_squint_left[0]]
                                 - m_landmarks[MeFaMoConfig.cheek_squint_left[1]]).magnitude;
        var cheek_squint_right = (m_landmarks[MeFaMoConfig.cheek_squint_right[0]]
                                  - m_landmarks[MeFaMoConfig.cheek_squint_right[1]]).magnitude;
        blendShape[FaceBlendShape.CheekSquintLeft] = 1 - Remap(FaceBlendShape.CheekSquintLeft, cheek_squint_left);
        blendShape[FaceBlendShape.CheekSquintRight] = 1 - Remap(FaceBlendShape.CheekSquintRight, cheek_squint_right);

        blendShape[FaceBlendShape.NoseSneerLeft] = blendShape[FaceBlendShape.CheekSquintLeft];
        blendShape[FaceBlendShape.NoseSneerRight] = blendShape[FaceBlendShape.CheekSquintRight];

    }
}

