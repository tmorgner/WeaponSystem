using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RabbitStewdio.Unity.UnityTools
{
    public class SpatialIndex<T> where T : MonoBehaviour
    {
        struct Point : IEquatable<Point>
        {
            readonly int x;
            readonly int y;

            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public Point(Vector3 p) : this()
            {
                this.x = (int)Mathf.Floor(p.x);
                this.y = (int)Mathf.Floor(p.y);
            }

            public bool Equals(Point other)
            {
                return x == other.x
                       && y == other.y;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                return obj is Point && Equals((Point)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (x * 397) ^ y;
                }
            }

            public static bool operator ==(Point left, Point right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Point left, Point right)
            {
                return !left.Equals(right);
            }
        }

        struct RefEqualsWrapper : IEquatable<RefEqualsWrapper>
        {
            public readonly T Target;

            public RefEqualsWrapper(T target)
            {
                if (target == null)
                {
                    throw new ArgumentNullException(nameof(target));
                }

                Target = target;
            }

            public bool Equals(RefEqualsWrapper other)
            {
                return ReferenceEquals(Target, other.Target);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                return obj is RefEqualsWrapper && Equals((RefEqualsWrapper)obj);
            }

            public override int GetHashCode()
            {
                return RuntimeHelpers.GetHashCode(Target);
            }

            public static bool operator ==(RefEqualsWrapper left, RefEqualsWrapper right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(RefEqualsWrapper left, RefEqualsWrapper right)
            {
                return !left.Equals(right);
            }
        }

        readonly Dictionary<Point, HashSet<RefEqualsWrapper>> index;
        readonly Dictionary<RefEqualsWrapper, Point> reverseIndex;

        public SpatialIndex()
        {
            index = new Dictionary<Point, HashSet<RefEqualsWrapper>>();
            reverseIndex = new Dictionary<RefEqualsWrapper, Point>();
        }

        public List<T> Query(Vector3 v, List<T> data = null)
        {
            if (data == null)
            {
                data = new List<T>();
            }
            else
            {
                data.Clear();
            }

            var point = new Point(v);
            HashSet<RefEqualsWrapper> hs;
            if (index.TryGetValue(point, out hs))
            {
                foreach (var wrapper in hs)
                {
                    data.Add(wrapper.Target);
                }
            }

            return data;
        }

        public void Add(T t)
        {
            var point = new Point(t.transform.position);
            var wrapper = new RefEqualsWrapper(t);
            Point existing;
            if (reverseIndex.TryGetValue(wrapper, out existing))
            {
                if (existing == point)
                {
                    return;
                }

                Remove(t);
            }

            HashSet<RefEqualsWrapper> hs;
            if (index.TryGetValue(point, out hs))
            {
                hs.Add(wrapper);
            }
            else
            {
                hs = new HashSet<RefEqualsWrapper> { wrapper };
                index[point] = hs;
            }

            reverseIndex[wrapper] = point;
        }

        public void Moved(T t)
        {
            Remove(t);
            Add(t);
        }

        public void Remove(T t)
        {
            Point point;
            var wrapper = new RefEqualsWrapper(t);
            if (reverseIndex.TryGetValue(wrapper, out point))
            {
                HashSet<RefEqualsWrapper> hs;
                if (index.TryGetValue(point, out hs))
                {
                    hs.Remove(wrapper);
                }
                reverseIndex.Remove(wrapper);
            }

        }
    }
}
