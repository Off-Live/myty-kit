using System.Collections.Generic;
using MYTYKit.ThirdParty.KalidoKit;
using UnityEngine;

namespace MYTYKit.ThirdParty.MeFaMo
{
    public class MeFaMoSolver
    {
        public Dictionary<MeFaMoConfig.FaceBlendShape, float> blendShape = new();
        public Vector2 leftPupil = Vector2.zero;
        public Vector2 rightPupil = Vector2.zero;
        
        public Vector3[] m_landmarks;
        List<Vector3[]> m_history = new();
        
        public void Solve(Vector3[] landmarks, int frameWidth, int frameHeight)
        {
            if (landmarks == null || landmarks.Length < 468)
            {
                Debug.LogWarning("MeFaMoSolver : face landmark size should be equal or greater than 468");
                return;
            }

            m_landmarks = new Vector3[landmarks.Length];
            for (int i = 0; i <landmarks.Length; i++)
            {
                m_landmarks[i] = landmarks[i];
            }
            SmoothMesh();
            if (m_landmarks.Length > 468) //for iris landmarks
            {
                CalculatePupil();
            }
            SolvePose(frameWidth, frameHeight);
            
            CalculateMouthLandmark();
            CalculateEyeLandmark();
            
            
        }
    
        void SolvePose(int width, int height)
        {
            var frameWidth = width;
            var frameHeight = height;
            var focalLength = width;
            var pcf = new PerspectiveCameraFrustum(frameWidth, frameHeight, focalLength);
            var faceGeo = new FaceGeometry(m_landmarks, pcf);
            var metricLandmarks = faceGeo.GetMetricLandmarks();
            for (var i = 0; i < metricLandmarks.Length; i++)
            {
                m_landmarks[i] = metricLandmarks[i];
            }
        }

        void SmoothMesh()
        {
            var weight = new []
            {
                1.0f
            };
            m_history.Add(m_landmarks);
            if (m_history.Count < weight.Length) return;
            if (m_history.Count > weight.Length)
            {
                m_history.RemoveAt(0);
            }

            var newLandmarks = new Vector3[m_landmarks.Length];

        
            for (var i = 0; i < newLandmarks.Length; i++)
            {
                Vector3 tmp = Vector3.zero;
                for (var j = 0; j < weight.Length; j++)
                {
                    tmp += m_history[j][i] * weight[j];
                }
                newLandmarks[i] = tmp;
            }
            m_landmarks = newLandmarks;

        }
        
        float Remap(MeFaMoConfig.FaceBlendShape blendShape, float value)
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
            var upper_lip = m_landmarks[MeFaMoConfig.upper_lip];
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
            var jaw_open = Remap(MeFaMoConfig.FaceBlendShape.JawOpen, jaw_open_ratio);
        
        
        
            var mouth_open = Remap(MeFaMoConfig.FaceBlendShape.MouthClose, mouth_open_dist/mouth_center_nose_dist);
            blendShape[MeFaMoConfig.FaceBlendShape.MouthClose] = 0.0f;
            blendShape[MeFaMoConfig.FaceBlendShape.JawOpen] = mouth_open;
        
            var eye_right_center = (m_landmarks[MeFaMoConfig.eye_right[0]] + m_landmarks[MeFaMoConfig.eye_right[1]]) / 2;
            var eye_left_center = (m_landmarks[MeFaMoConfig.eye_left[0]] + m_landmarks[MeFaMoConfig.eye_left[1]]) / 2;
            var eye_distance = (eye_right_center - eye_left_center).magnitude;
            var mouth_left_half = Mathf.Abs(nose_tip.x - mouth_corner_left.x) / eye_distance;
            var mouth_right_half = Mathf.Abs(nose_tip.x - mouth_corner_right.x) / eye_distance;

            var mouth_width_ratio = mouth_width / eye_distance;
            var mouth_gradient_left = (mouth_corner_left.y - mouth_center.y) / (mouth_corner_left.x - mouth_center.x);
            var mouth_gradient_right = - (mouth_corner_right.y - mouth_center.y) / (mouth_corner_right.x - mouth_center.x);
        
            var mouth_skew = (mouth_center.x - nose_tip.x)/eye_distance;

        
            blendShape[MeFaMoConfig.FaceBlendShape.MouthSmileLeft] = 0;
            blendShape[MeFaMoConfig.FaceBlendShape.MouthStretchLeft] = 0;
            blendShape[MeFaMoConfig.FaceBlendShape.MouthSmileRight] = 0;
            blendShape[MeFaMoConfig.FaceBlendShape.MouthStretchRight] = 0;
            if (mouth_left_half > 0.4f)
            {
                var value = Remap(mouth_left_half, 0.4f, 0.5f);
                if (mouth_gradient_left > 0.0f)
                {
                    var smile = 0.5f+0.5f*Remap(mouth_gradient_left, 0.0f, 0.2f);
                    var strecth = 0.5f * (1 - Remap(mouth_gradient_left, 0.0f, 0.2f));
                    blendShape[MeFaMoConfig.FaceBlendShape.MouthSmileLeft] = smile*value;
                    blendShape[MeFaMoConfig.FaceBlendShape.MouthStretchLeft] = strecth*value;
                
                }
                else
                {
                    var smile = 0.5f*Remap(mouth_gradient_left, -0.3f, 0.0f);
                    var strecth = 0.5f+ 0.5f * (1 - Remap(mouth_gradient_left, -0.3f, 0.0f));
                    blendShape[MeFaMoConfig.FaceBlendShape.MouthSmileLeft] = smile*value;
                    blendShape[MeFaMoConfig.FaceBlendShape.MouthStretchLeft] = strecth*value;
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
                    blendShape[MeFaMoConfig.FaceBlendShape.MouthSmileRight] = smile * value;
                    blendShape[MeFaMoConfig.FaceBlendShape.MouthStretchRight] = strecth * value;

                }
                else
                {
                    var smile = 0.5f*Remap(mouth_gradient_right, -0.3f, 0.0f);
                    var strecth = 0.5f+ 0.5f * (1 - Remap(mouth_gradient_right, -0.3f, 0.0f));
                    blendShape[MeFaMoConfig.FaceBlendShape.MouthSmileRight] = smile*value;
                    blendShape[MeFaMoConfig.FaceBlendShape.MouthStretchRight] = strecth*value;
                }
            }

            if (mouth_width_ratio < 0.7f)
            {
                blendShape[MeFaMoConfig.FaceBlendShape.MouthPucker] = 1- Remap(mouth_width_ratio, 0.5f, 0.7f);
                blendShape[MeFaMoConfig.FaceBlendShape.MouthFunnel] = mouth_open;
            }
            else
            {
                blendShape[MeFaMoConfig.FaceBlendShape.MouthPucker] = 0.0f;
                blendShape[MeFaMoConfig.FaceBlendShape.MouthFunnel] = 0.0f;
            }

            blendShape[MeFaMoConfig.FaceBlendShape.MouthLeft] = Remap(mouth_skew, 0, 0.2f);
            blendShape[MeFaMoConfig.FaceBlendShape.MouthRight] = 1.0f - Remap(mouth_skew, -0.2f, 0f);

            blendShape[MeFaMoConfig.FaceBlendShape.JawLeft] = blendShape[MeFaMoConfig.FaceBlendShape.MouthLeft];
            blendShape[MeFaMoConfig.FaceBlendShape.JawRight] = blendShape[MeFaMoConfig.FaceBlendShape.MouthRight];

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

        void CalculateEyeLandmark()
        {
            var eye_open_ratio_left = GetEyeOpenRatio(MeFaMoConfig.eye_left);
            var eye_open_ratio_right = GetEyeOpenRatio(MeFaMoConfig.eye_right);
        
            
            var blink_left = 1 - Remap(eye_open_ratio_left,0.25f,0.80f);
            var blink_right = 1 - Remap(eye_open_ratio_right,0.25f,0.80f);
            var left_eye_center = (m_landmarks[MeFaMoConfig.eye_left[0]] + m_landmarks[MeFaMoConfig.eye_left[1]]) / 2;
            var right_eye_center = (m_landmarks[MeFaMoConfig.eye_right[0]] + m_landmarks[MeFaMoConfig.eye_right[1]]) / 2;
            blendShape[MeFaMoConfig.FaceBlendShape.EyeBlinkLeft] = blink_left;
            blendShape[MeFaMoConfig.FaceBlendShape.EyeBlinkRight] = blink_right;
            blendShape[MeFaMoConfig.FaceBlendShape.EyeSquintLeft] = blendShape[MeFaMoConfig.FaceBlendShape.MouthSmileLeft]*0.7f;
            blendShape[MeFaMoConfig.FaceBlendShape.EyeSquintRight] = blendShape[MeFaMoConfig.FaceBlendShape.MouthSmileRight]*0.7f;

            var left_brow_dist = m_landmarks[MeFaMoConfig.left_brow_top].y - left_eye_center.y;
            var right_brow_dist = m_landmarks[MeFaMoConfig.right_brow_top].y - right_eye_center.y;

        
            blendShape[MeFaMoConfig.FaceBlendShape.BrowOuterUpLeft] = Remap(left_brow_dist, 2.8f, 3.2f);
            blendShape[MeFaMoConfig.FaceBlendShape.BrowOuterUpRight] = Remap(right_brow_dist, 2.8f, 3.2f);
            blendShape[MeFaMoConfig.FaceBlendShape.BrowInnerUp] =
                (blendShape[MeFaMoConfig.FaceBlendShape.BrowOuterUpLeft] + blendShape[MeFaMoConfig.FaceBlendShape.BrowOuterUpRight]) / 2;
            blendShape[MeFaMoConfig.FaceBlendShape.BrowDownLeft] = (1- Remap(left_brow_dist, 2.2f, 2.4f))*0.4f;
            blendShape[MeFaMoConfig.FaceBlendShape.BrowDownRight] = (1- Remap(right_brow_dist, 2.2f, 2.4f))*0.4f;

            blendShape[MeFaMoConfig.FaceBlendShape.EyeWideLeft] = blendShape[MeFaMoConfig.FaceBlendShape.BrowOuterUpLeft];
            blendShape[MeFaMoConfig.FaceBlendShape.EyeWideRight] = blendShape[MeFaMoConfig.FaceBlendShape.BrowOuterUpRight];
        }

        void CalculatePupil()
        {
            
            leftPupil = FaceSolver.GetPupilPosition(m_landmarks, true);
            rightPupil = FaceSolver.GetPupilPosition(m_landmarks, false);
            leftPupil = new Vector2(Mathf.Clamp(leftPupil.x *3.0f , -1.0f,1.0f), -Mathf.Clamp(leftPupil.y ,-1.0f,1.0f));
            rightPupil = new Vector2(Mathf.Clamp(rightPupil.x *3.0f,-1.0f,1.0f), -Mathf.Clamp(rightPupil.y ,-1.0f,1.0f));

        }
    }
}

