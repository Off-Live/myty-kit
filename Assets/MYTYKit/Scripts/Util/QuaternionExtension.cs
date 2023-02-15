using UnityEngine;

namespace MYTYKit
{
    public static class QuaternionExtension
    {
        public static Vector3 GetVector(this Quaternion q)
        {
            return new Vector3(q.x, q.y, q.z);
        }

        public static float GetVectorNorm(this Quaternion q)
        {
            return Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z);
        }

        public static float GetNorm(this Quaternion q)
        {
            return Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w*q.w);
        }

        public static float GetScalar(this Quaternion q)
        {
            return q.w;
        }

        public static Quaternion GetConjugate(this Quaternion q)
        {
            return new Quaternion(-q.x, -q.y, -q.z, q.w);
        }

        public static Quaternion GetNegate(this Quaternion q)
        {
            return new Quaternion(-q.x, -q.y, -q.z, -q.w);
        }

        public static float GetAngleDeg(this Quaternion q)
        {
            var normQ = q.normalized;
            return Mathf.Rad2Deg * Mathf.Acos(normQ.w) * 2;
        }

        public static Quaternion SelectContinuousOrientation(this Quaternion q, Quaternion reference)
        {
            return Mathf.Abs(q.w - reference.w) > Mathf.Abs(-q.w - reference.w) ? GetNegate(q) : q;
        }

    }
}