using System;
using UnityEngine;

namespace UnityTools
{
    public static class CommonUtils
    {
        public static Vector3 Cleanse(this Vector3 v)
        {
            var precision = 1000f;
            return new Vector3(Mathf.Round(v.x * precision) / precision,
                               Mathf.Round(v.y * precision) / precision,
                               Mathf.Round(v.z * precision) / precision);
        }

        public static Matrix4x4 SRT(Vector3 translation, Quaternion rotation, Vector3 scale)
        {
            return Matrix4x4.Scale(scale) * Matrix4x4.Rotate(rotation) * Matrix4x4.Translate(translation);
        }

        public static bool IsEqual(this float f, float t, float delta = 0.005f)
        {
            return Mathf.Abs(f - t) < delta;
        }

        public static float Round(this float v, float precision)
        {
            return Mathf.Round(v / precision) * precision;
        }

        public static Vector3 NormalizeAngle(this Vector3 qe)
        {
            return new Vector3(Mathf.DeltaAngle(qe.x, 0), Mathf.DeltaAngle(qe.y, 0), Mathf.DeltaAngle(qe.z, 0));
        }

        public static Vector3 Squared(this Vector3 v)
        {
            return new Vector3(v.x * v.x, v.y * v.y, v.z * v.z);
        }

        public static string Print(this Vector3 input)
        {
            return $"({input.x:0.000}, {input.y:0.000}, {input.z:0.000},|{input.magnitude}|)";
        }

        public static Vector4 ToTransformVector(this Vector3 t)
        {
            return new Vector4(t.x, t.y, t.z, 1);
        }


        static Vector3 Pendicular(Vector3 v)
        {
            v.Normalize();
            var rotateA = Quaternion.Euler(90, 0, 0) * v;
            if ((rotateA - v).magnitude > 1)
            {
                return rotateA;
            }
            var rotateB = Quaternion.Euler(0, 90, 0) * v;
            if ((rotateB - v).magnitude > 1)
            {
                return rotateB;
            }
            return Quaternion.Euler(0, 0, 90) * v;
        }

        /// <summary>
        ///  Similar to Quaternion.Angle, but preserving the directionality of the rotation.
        ///  This requires a reference vector that signifies the plane in which to rotate.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Angle(Quaternion a, Quaternion b, Vector3 reference)
        {
            var refVector = Pendicular(reference);

            Vector3 sourceForward = a * refVector;
            Vector3 targetForward = b * refVector;
            // Debug.Log("ForwardPos: A=" + sourceForward.Print() + "  - " + targetForward.Print());
            var angle = Mathf.Acos(Vector3.Dot(sourceForward, targetForward)) * Mathf.Rad2Deg;

            var direction = Vector3.Dot(reference, Vector3.Cross(sourceForward, targetForward));
            return Mathf.Sign(direction) * angle;
        }

        public static RectInt Intersect(this RectInt r1, RectInt r2)
        {
            int x = Math.Max(r1.x, r2.x);
            int y = Math.Max(r1.y, r2.y);
            int maxx = Math.Min(r1.xMax, r2.xMax);
            int maxy = Math.Min(r1.yMax, r2.yMax);
            return new RectInt(x, y, maxx - x, maxy - y);
        }

        public static T Read<T>(this T[,] array, int x, int y)
        {
            var maxX = array.GetUpperBound(0);
            var maxY = array.GetUpperBound(1);
            var minX = array.GetLowerBound(0);
            var minY = array.GetLowerBound(1);
            var rx = Mathf.Clamp(x, minX, maxX);
            var ry = Mathf.Clamp(y, minY, maxY);
            return array[rx, ry];
        }

        public static void Write<T>(this T[,] array, int x, int y, T value)
        {
            var maxX = array.GetUpperBound(0);
            var maxY = array.GetUpperBound(1);
            var minX = array.GetLowerBound(0);
            var minY = array.GetLowerBound(1);
            if (x < minX || x > maxX)
            {
                return;
            }

            if (y < minY || y > maxY)
            {
                return;
            }

            array[x, y] = value;
        }

        public static void Fill<T>(this T[,] array, T v)
        {
            var width = array.GetUpperBound(0);
            var height = array.GetUpperBound(1);

            for (var y = array.GetLowerBound(1); y <= height; y += 1)
            {
                for (var x = array.GetLowerBound(0); x <= width; x += 1)
                {
                    array[x, y] = v;
                }
            }
        }

        public static Vector2 Multiply(this Vector2 a, Vector2 b)
        {
            a.Scale(b);
            return a;
        }

        public static Vector2 ToVector2(this Vector3 a)
        {
            return new Vector2(a.x, a.y);
        }

        public static bool IsValid(this Vector3 a)
        {
            if (float.IsNaN(a.x) || float.IsNaN(a.y) || float.IsNaN(a.z))
            {
                return false;
            }

            return true;
        }
    }
}