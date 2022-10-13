/**
 * This code is C# (Unity) version of https://github.com/yeemachine/kalidokit 
 *   
 * Original License
 * 
 * MIT License
 * Copyright (c) 2021 yeemachine
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 *  
 */

using UnityEngine;

namespace MYTYKit.ThirdParty.KalidoKit
{
    public class FaceSolver
    {
        /**
 * Landmark points labeled for eye, brow, and pupils
 */

        private static int[] RIGHT_EYE = { 33, 133, 160, 159, 158, 144, 145, 153 };
        private static int[] LEFT_EYE = { 263, 362, 387, 386, 385, 373, 374, 380 };

        private static int[] RIGHT_EYEBROW = { 35, 244, 63, 105, 66, 229, 230, 231 };
        private static int[] LEFT_EYEBROW = { 265, 464, 293, 334, 296, 449, 450, 451 };

        private static int[] RIGHT_PUPIL = { 468, 469, 470, 471, 472 };
        private static int[] LEFT_PUPIL = { 473, 474, 475, 476, 477 };

        private static int[] END_POINTS = {234,454, 10,152 };


        private static float GetEyeLidRatio(
            Vector3 eyeOuterCorner,
            Vector3 eyeInnerCorner,
            Vector3 eyeOuterUpperLid,
            Vector3 eyeMidUpperLid,
            Vector3 eyeInnerUpperLid,
            Vector3 eyeOuterLowerLid,
            Vector3 eyeMidLowerLid,
            Vector3 eyeInnerLowerLid,
            Vector3 faceLeftMost,
            Vector3 faceRightMost,
            Vector3 faceTopMost, 
            Vector3 faceBottomMost)
        {
            float faceHeight = (faceTopMost - faceBottomMost).magnitude;
            float faceWidth = (faceLeftMost - faceRightMost).magnitude;
            float eyeWidth = (eyeOuterCorner - eyeInnerCorner).magnitude/faceWidth;
            float eyeOuterLidDistance = (eyeOuterUpperLid - eyeOuterLowerLid).magnitude;
            float eyeMidLidDistance = (eyeMidUpperLid - eyeMidLowerLid).magnitude;
            float eyeInnerLidDistance = (eyeInnerUpperLid - eyeInnerLowerLid).magnitude;
            float eyeLidAvg = (eyeOuterLidDistance + eyeMidLidDistance + eyeInnerLidDistance) / 3 /faceHeight;
            return  eyeLidAvg / eyeWidth;
        }

        public static void GetEyeOpen(out float normRatio, out float rawRatio, bool left, Vector3[] faceLM, float high=0.16f, float low = 0.005f)
        {
            int [] indices;
            if (left) indices = LEFT_EYE;
            else indices = RIGHT_EYE;
            float eyeDistance = GetEyeLidRatio(
                faceLM[indices[0]],
                faceLM[indices[1]],
                faceLM[indices[2]],
                faceLM[indices[3]],
                faceLM[indices[4]],
                faceLM[indices[5]],
                faceLM[indices[6]],
                faceLM[indices[7]],
                faceLM[END_POINTS[0]],
                faceLM[END_POINTS[1]],
                faceLM[END_POINTS[2]],
                faceLM[END_POINTS[3]]
            );

            //float maxRatio = 0.285f;
            //float ratio = Mathf.Clamp(eyeDistance / maxRatio, 0, 2);
            //float eyeOpenRatio = (Mathf.Clamp(ratio, low, high) - low) / (high - low);
        
            float eyeOpenRatio = (Mathf.Clamp(eyeDistance, low, high) - low) / (high - low);

            normRatio = eyeOpenRatio;
            rawRatio = normRatio;

        
        }

        /**
 * Calculate pupil position [-1,1]
 * @param {Results} lm : array of results from tfjs or mediapipe
 * @param {"left"| "right"} side : "left" or "right"
 */
        public static Vector2 GetPupilPosition(Vector3[] faceLM, bool left) {
            int[] eyeIndices;
            int[] pupilIndices;
            if (left) {
                eyeIndices = LEFT_EYE;
                pupilIndices = LEFT_PUPIL;
            }else {
                eyeIndices = RIGHT_EYE;
                pupilIndices = RIGHT_PUPIL;
            }
            Vector3 eyeOuterCorner = faceLM[eyeIndices[0]];
            Vector3 eyeInnerCorner = faceLM[eyeIndices[1]];
            Vector3 eyeMidUpper = faceLM[eyeIndices[3]];
            Vector3 eyeMidLower = faceLM[eyeIndices[6]];
            
            float eyeWidth = (eyeOuterCorner - eyeInnerCorner).magnitude;
            Vector3 midPoint = (eyeOuterCorner + eyeInnerCorner)*0.5f;
            Vector3 pupil = faceLM[pupilIndices[0]];

            Vector3 pos = pupil - midPoint;
            Vector3 principalAxis = (eyeOuterCorner - eyeInnerCorner).normalized;
            Vector3 verticalAxis = (eyeMidUpper - eyeMidLower).normalized;
            Vector3 lookAt = Vector3.Cross(verticalAxis, principalAxis).normalized;
            verticalAxis = - Vector3.Cross(principalAxis,lookAt).normalized;// the capture raw landmarks is invertied in y-axis
            

            Vector3 xComp = Vector3.Dot(pos, principalAxis) * principalAxis;
            Vector3 yComp = Vector3.Dot(pos, verticalAxis) * verticalAxis;

            //float ySign = Vector3.Dot(yComp, Vector3.up) / Mathf.Abs(Vector3.Dot(yComp, Vector3.up));
            float xSign = left ? -1.0f : 1.0f;
            
            return new Vector2(Vector3.Dot(xComp,principalAxis)/eyeWidth*xSign *2.0f, Vector3.Dot(yComp,verticalAxis)/eyeWidth*2.0f);

        }

        /**
 * Method to stabilize blink speeds to fix inconsistent eye open/close timing
 * @param {Object} eye : object with left and right eye values
 * @param {Number} headY : head y axis rotation in radians
 * @param {Object} options: Options for blink stabilization
 */
        public static void StabilizeBlink(out float left, out float right, float eyeL, float eyeR, float headY, bool enableWink = true, float maxRot = 0.5f)
        {
            eyeR = Mathf.Clamp01(eyeR);
            eyeL = Mathf.Clamp01(eyeL);

            float blinkDiff = Mathf.Abs(eyeL - eyeR);
            //theshold to which difference is considered a wink
            float blinkThresh = enableWink ? 0.8f : 1.2f;
            //detect when both eyes are closing
            bool isClosing = eyeR < 0.3 && eyeL < 0.3;
            bool isOpen = eyeL > 0.6 && eyeR > 0.6;
            // sets obstructed eye to the opposite eye value

            if (headY > maxRot)
            {
                left = eyeR;
                right = eyeR;
                return;
            }
            if (headY < -maxRot)
            {
                left = eyeL;
                right = eyeL;
            }

            left = blinkDiff >= blinkThresh && !isClosing && !isOpen
                ? eyeL
                : eyeR > eyeL
                    ? Mathf.Lerp(eyeR, eyeL, 0.95f)
                    : Mathf.Lerp(eyeR, eyeL, 0.05f);

            right = blinkDiff >= blinkThresh && !isClosing && !isOpen
                ? eyeR
                : eyeR > eyeL
                    ? Mathf.Lerp(eyeR, eyeL, 0.95f)
                    : Mathf.Lerp(eyeR, eyeL, 0.05f);
        }

        public static float  GetBrowRaise(Vector3[] faceLM, bool left)
        {
            int[] browIndices;
            if (left) browIndices = LEFT_EYEBROW;
            else browIndices = RIGHT_EYEBROW;

            float browDistance = GetEyeLidRatio(
                faceLM[browIndices[0]],
                faceLM[browIndices[1]],
                faceLM[browIndices[2]],
                faceLM[browIndices[3]],
                faceLM[browIndices[4]],
                faceLM[browIndices[5]],
                faceLM[browIndices[6]],
                faceLM[browIndices[7]],
                faceLM[END_POINTS[0]],
                faceLM[END_POINTS[1]],
                faceLM[END_POINTS[2]],
                faceLM[END_POINTS[3]]
            );
            //float maxBrowRatio = 1.15f;
            //float browHigh = 0.125f;
            //float browLow = 0.07f;

            //float browRatio = browDistance / maxBrowRatio - 1;
            //return (Mathf.Clamp(browRatio, browLow, browHigh) - browLow) / (browHigh - browLow);

        

            float browHigh = 0.8f;
            float browLow = 0.5f;
            return (Mathf.Clamp(browDistance, browLow, browHigh) - browLow) / (browHigh - browLow);

        }
        private static float remap(float val, float min, float max)
        {
            return (Mathf.Clamp(val, min, max) - min) / (max - min);
        }
        public static void CalcMouth(Vector3[] faceLM,
            out float mouthX,
            out float mouthY)

        {
            Vector3 eyeInnerCornerL = faceLM[133];
            Vector3 eyeInnerCornerR = faceLM[362];
            Vector3 eyeOuterCornerL = faceLM[130];
            Vector3 eyeOuterCornerR = faceLM[263];

            // eye keypoint distances
            float eyeInnerDistance = (eyeInnerCornerL - eyeInnerCornerR).magnitude;
            float eyeOuterDistance = (eyeOuterCornerL - eyeOuterCornerR).magnitude;

            // mouth keypoints
            Vector3 upperInnerLip = faceLM[13];
            Vector3 lowerInnerLip = faceLM[14];
            Vector3 mouthCornerLeft = faceLM[61];
            Vector3 mouthCornerRight = faceLM[291];

            // mouth keypoint distances
            float mouthOpen = (upperInnerLip-lowerInnerLip).magnitude;
            float mouthWidth = (mouthCornerLeft-mouthCornerRight).magnitude;

            float ratioY = mouthOpen / eyeInnerDistance;
            float ratioX = mouthWidth / eyeOuterDistance;

            // normalize and scale mouth open
            ratioY = remap(ratioY, 0.10f, 2.0f);

            //// normalize and scale mouth shape
            ratioX = remap(ratioX, 0.30f, 0.75f);
            ratioX = (ratioX - 0.3f) * 2f;

            mouthX = ratioX;
            mouthY = ratioY; 
        }
    };
}
