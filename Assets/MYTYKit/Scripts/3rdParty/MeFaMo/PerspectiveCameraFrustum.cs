using UnityEngine;

namespace MYTYKit.ThirdParty.MeFaMo
{
    public class PerspectiveCameraFrustum
    {
        public float near;
        public float far;
        public float width;
        public float height;
        public float focalLength;
        public float fovY;
        public float heightAtNear;
        public float widthAtNear;
        public float left;
        public float right;
        public float bottom;
        public float top;
        public PerspectiveCameraFrustum(
            float frameWidth,
            float frameHeight,
            float focalLength,
            float near = 1.0f,
            float far = 1000.0f
        )
        {
            this.near = near;
            this.far = far;
            this.focalLength = focalLength;
            width = frameWidth;
            height = frameHeight;
            fovY = 2 * Mathf.Atan(height / (2.0f * focalLength));
            heightAtNear = height * near / focalLength;
            widthAtNear = width * near / focalLength;

            left = -0.5f * widthAtNear;
            right = 0.5f * widthAtNear;
            bottom = -0.5f * heightAtNear;
            top = 0.5f * heightAtNear;
        }
    }
}
