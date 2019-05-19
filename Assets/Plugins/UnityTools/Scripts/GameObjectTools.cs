using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RabbitStewdio.Unity.UnityTools
{
    public static class GameObjectTools
    {
        /// <summary>
        ///   Implict conversion magic to trick C# into preferring formattables over normal strings.
        ///   <see href="https://stackoverflow.com/a/39035309"/>
        /// </summary>
        public struct FormattableStringPreferenceAdapter
        {
            public string String { get; }

            public FormattableStringPreferenceAdapter(string s)
            {
                String = s;
            }

            public override string ToString()
            {
                return String;
            }

            public static implicit operator FormattableStringPreferenceAdapter(string s)
            {
                return new FormattableStringPreferenceAdapter(s);
            }

            public static implicit operator FormattableStringPreferenceAdapter(FormattableString fs)
            {
                throw new InvalidOperationException(
                    "Missing FormattableString overload of method taking this type as argument");
            }
        }

        static readonly List<Component> componentBuffer;

        static GameObjectTools()
        {
            componentBuffer = new List<Component>(64);
        }

       public static void LogWithName(this Object hive, GameObjectTools.FormattableStringPreferenceAdapter format)
        {
#if DEBUG
            if (Debug.unityLogger.logEnabled && Debug.unityLogger.IsLogTypeAllowed(LogType.Log))
            {
                Debug.Log($"[{hive.name}] {format}", hive);
            }
#endif
        }

       public static void LogWithName(this Object hive, FormattableString format)
        {
#if DEBUG
            if (Debug.unityLogger.logEnabled && Debug.unityLogger.IsLogTypeAllowed(LogType.Log))
            {
                Debug.Log($"[{hive.name}] {format}", hive);
            }
#endif
        }


       public static void LogWithPath(this GameObject hive, GameObjectTools.FormattableStringPreferenceAdapter format)
        {
#if DEBUG
            if (Debug.unityLogger.logEnabled && Debug.unityLogger.IsLogTypeAllowed(LogType.Log))
            {
                Debug.Log($"[{hive.GetPath()}] {format}", hive);
            }
#endif
        }

       public static void LogWithPath(this GameObject hive, FormattableString format)
        {
#if DEBUG
            if (Debug.unityLogger.logEnabled && Debug.unityLogger.IsLogTypeAllowed(LogType.Log))
            {
                Debug.Log($"[{hive.GetPath()}] {format}", hive);
            }
#endif
        }

       public static void Log<T>(this T hive, GameObjectTools.FormattableStringPreferenceAdapter format) where T: Object, ISelectiveLogBehaviour
        {
#if DEBUG
            if (hive.EnableLogging && Debug.unityLogger.logEnabled && Debug.unityLogger.IsLogTypeAllowed(LogType.Log))
            {
                var path = hive.LogPath ? hive.Path : hive.Name;
                Debug.Log($"[{path}] {format}", hive);
            }
#endif
        }

       public static void LogWarning<T>(this T hive, GameObjectTools.FormattableStringPreferenceAdapter format) where T: Object, ISelectiveLogBehaviour
        {
#if DEBUG
            if (hive.EnableLogging && Debug.unityLogger.logEnabled && Debug.unityLogger.IsLogTypeAllowed(LogType.Warning))
            {
                var path = hive.LogPath ? hive.Path : hive.Name;
                Debug.LogWarning($"[{path}] {format}", hive);
            }
#endif
        }

       public static void Log<T>(this T hive, FormattableString format) where T: Object, ISelectiveLogBehaviour
        {
#if DEBUG
            if (hive.EnableLogging && Debug.unityLogger.logEnabled && Debug.unityLogger.IsLogTypeAllowed(LogType.Log))
            {
                var path = hive.LogPath ? hive.Path : hive.Name;
                Debug.Log($"[{path}] {format}", hive);
            }
#endif
        }

       public static void LogWarning<T>(this T hive, FormattableString format) where T: Object, ISelectiveLogBehaviour
        {
#if DEBUG
            if (hive.EnableLogging && Debug.unityLogger.logEnabled && Debug.unityLogger.IsLogTypeAllowed(LogType.Warning))
            {
                var path = hive.LogPath ? hive.Path : hive.Name;
                Debug.LogWarning($"[{path}] {format}", hive);
            }
#endif
        }

       public static void LogBasic<T>(this T hive, GameObjectTools.FormattableStringPreferenceAdapter format) where T: ISelectiveLogBehaviour
        {
#if DEBUG
            if (hive.EnableLogging && Debug.unityLogger.logEnabled && Debug.unityLogger.IsLogTypeAllowed(LogType.Log))
            {
                var path = hive.LogPath ? hive.Path : hive.Name;
                Debug.Log($"[{path}] {format}");
            }
#endif
        }

       public static void LogBasic<T>(this T hive, FormattableString format) where T: ISelectiveLogBehaviour
        {
#if DEBUG
            if (hive.EnableLogging && Debug.unityLogger.logEnabled && Debug.unityLogger.IsLogTypeAllowed(LogType.Log))
            {
                var path = hive.LogPath ? hive.Path : hive.Name;
                Debug.Log($"[{path}] {format}");
            }
#endif
        }


        public static bool HasParent(this GameObject source, GameObject possibleParent)
        {
            while (true)
            {
                if (source == possibleParent)
                {
                    return true;
                }

                var transformParent = source.transform.parent;
                if (transformParent)
                {
                    source = transformParent.gameObject;
                }
                else
                {
                    return false;
                }
            }
        }

        public static void AddChild(this Transform parent, Transform to)
        {
            to.parent = parent;
        }

        public static T GetOrCreate<T>(this Component c) where T: Component
        {
            if (TryGetComponent<T>(c, out var result))
            {
                return result;
            }

            return c.gameObject.AddComponent<T>();
        }

        public static T GetOrCreate<T>(this GameObject c) where T: Component
        {
            if (TryGetComponent<T>(c, out var result))
            {
                return result;
            }

            return c.gameObject.AddComponent<T>();
        }

        public static bool TryGetComponent<T>(this Component c, out T result) where T : class
        {
            return TryGetComponent(c.gameObject, out result);
        }

        public static bool TryGetComponent<T>(this GameObject go, out T result) where T : class
        {
            componentBuffer.Clear();
            go.GetComponents(componentBuffer);
            result = default;

            // cannot use var here, as we need to trick the compiler into accepting the cast next.
            // ReSharper disable once MoreSpecificForeachVariableTypeAvailable
            foreach (object c in componentBuffer)
            {
                result = c as T;
                if (result != null)
                {
                    return true;
                }
            }

            return false;
        }

        public static List<T> GetComponentsNonAlloc<T>(this Component c, List<T> buffer)
        {
            return c.gameObject.GetComponentsNonAlloc<T>(buffer);
        }

        public static List<T> GetComponentsNonAlloc<T>(this GameObject go, List<T> buffer)
        {
            componentBuffer.Clear();
            go.GetComponents(componentBuffer);
            if (buffer != null)
            {
                buffer.Clear();
                buffer.Capacity = Mathf.Max(buffer.Capacity, componentBuffer.Count);
            }
            else
            {
                buffer = new List<T>(componentBuffer.Count);
            }

            foreach (var c in componentBuffer)
            {
                if (c is T t)
                {
                    buffer.Add(t);
                }
            }

            return buffer;
        }

        public static List<T> GetComponentsInChildrenNonAlloc<T>(this Component c, List<T> buffer, bool includeInactiveComponents = false, bool includeInactiveGameObject = false)
        {
            return c.gameObject.GetComponentsInChildrenNonAlloc<T>(buffer, includeInactiveComponents);
        }

        public static List<T> GetComponentsInChildrenNonAlloc<T>(this GameObject go, List<T> buffer, bool includeInactiveComponents = false, bool includeInactiveGameObject = false)
        {
            if (buffer == null)
            {
                buffer = new List<T>();
            }
            else
            {
                buffer.Clear();
            }

            GetComponentsInChildrenInternal(go, buffer, includeInactiveComponents, includeInactiveGameObject);
            return buffer;
        }

        static void GetComponentsInChildrenInternal<T>(GameObject go, List<T> buffer, bool includeInactiveComponents = false, bool includeInactiveGameObject = false)
        {
            ProcessGameObject(go, buffer, includeInactiveComponents);

            var transform = go.transform;
            var childCount = transform.childCount;
            for (var c = 0; c < childCount; c += 1)
            {
                var cgo = transform.GetChild(c).gameObject;
                if (includeInactiveGameObject || cgo.activeSelf)
                {
                    GetComponentsInChildrenInternal(cgo, buffer, includeInactiveComponents, includeInactiveGameObject);
                }
            }
        }

        public static List<T> GetComponentsInParentsNonAlloc<T>(this Component c, List<T> buffer, bool includeInactiveComponents = false, bool includeInactiveGameObject = false)
        {
            return c.gameObject.GetComponentsInParentsNonAlloc<T>(buffer, includeInactiveComponents);
        }

        public static List<T> GetComponentsInParentsNonAlloc<T>(this GameObject go,
                                                                List<T> buffer,
                                                                bool includeInactiveComponents = false,
                                                                bool includeInactiveGameObject = false)
        {
            if (buffer == null)
            {
                buffer = new List<T>();
            }
            else
            {
                buffer.Clear();
            }

            GetComponentsInParentsInternal(go, buffer, includeInactiveComponents, includeInactiveGameObject);
            return buffer;
        }

        public static T GetParentComponent<T>(this Component c) where T : Component
        {
            if (c)
            {
                return c.gameObject.GetParentComponent<T>();
            }

            return null;
        }
        public static T GetParentComponent<T>(this GameObject go) where T: Component
        {
            if (!go)
            {
                return null;
            }

            var transformParent = go.transform.parent;
            if (transformParent)
            {
                return transformParent.GetComponentInParent<T>();
            }

            return null;
        }

        static void GetComponentsInParentsInternal<T>(GameObject go,
                                                      List<T> buffer,
                                                      bool includeInactiveComponents = false,
                                                      bool includeInactiveGameObject = false)
        {
            ProcessGameObject(go, buffer, includeInactiveComponents);

            var parentTransform = go.transform.parent;
            while (parentTransform != null)
            {
                var cgo = parentTransform.gameObject;
                if (includeInactiveGameObject == false && !cgo.activeSelf)
                {
                    return;
                }

                ProcessGameObject(cgo, buffer, includeInactiveComponents);
                parentTransform = cgo.transform.parent;
            }
        }

        public static void ProcessGameObject<T>(GameObject go, List<T> buffer, bool includeInactive)
        {
            componentBuffer.Clear();
            try
            {
                go.GetComponents(componentBuffer);
                foreach (var c in componentBuffer)
                {
                    if (c is T t && ShouldInclude(c, includeInactive))
                    {
                        buffer.Add(t);
                    }
                }
            }
            finally
            {
                componentBuffer.Clear();
            }
        }

        static bool ShouldInclude(Component c, bool includeInactive)
        {
            if (includeInactive)
            {
                return true;
            }

            if (c is Behaviour m)
            {
                return m.enabled;
            }

            return true;
        }



        public static bool IsMatched(this LayerMask m, GameObject g)
        {
            var gl = 1 << g.layer;
            return (m.value & gl) != 0;
        }

        public static bool IsEqual(this RectInt r, RectInt other)
        {
            return r.x == other.x && r.y == other.y && r.width == other.width && r.height == other.height;
        }

        public static string GetPath(this GameObject go)
        {
            return GetPath(go.transform);
        }

        public static string GetPath(this Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }

        public static void DrawCone(Transform transform, float totalFOV = 70, float rayRange = 10)
        {
            DrawCone(transform.position, transform.forward, totalFOV, rayRange);
        }

        public static void DrawCone(Vector3 position, Vector3 forward, float totalFOV = 70, float rayRange = 10)
        {
            var halfFOV = totalFOV / 2.0f;
            var baseRotation = Quaternion.LookRotation(forward, Vector3.up);
            var leftRayRotation = baseRotation * Quaternion.Euler(-halfFOV, 0, 0);
            var rightRayRotation = baseRotation * Quaternion.Euler(halfFOV, 0, 0);
            var topRayRotation = baseRotation * Quaternion.Euler(0, -halfFOV, 0);
            var bottomRayRotation = baseRotation * Quaternion.Euler(0, halfFOV, 0);

            var leftRayDirection = leftRayRotation * Vector3.forward;
            var rightRayDirection = rightRayRotation * Vector3.forward;
            var topRayDirection = topRayRotation * Vector3.forward;
            var bottomRayDirection = bottomRayRotation * Vector3.forward;
            Gizmos.DrawRay(position, leftRayDirection * rayRange);
            Gizmos.DrawRay(position, rightRayDirection * rayRange);
            Gizmos.DrawRay(position, topRayDirection * rayRange);
            Gizmos.DrawRay(position, bottomRayDirection * rayRange);
            Gizmos.DrawRay(position, forward * rayRange);

            var cosHalf = Mathf.Cos(totalFOV * Mathf.Deg2Rad / 2);
            var cos = Mathf.Sin(totalFOV * Mathf.Deg2Rad / 2);

            DrawWireArc(position, baseRotation * Vector3.up, topRayDirection * rayRange, totalFOV, rayRange);
            DrawWireArc(position, baseRotation * Vector3.left, rightRayDirection * rayRange, totalFOV, rayRange);
            DrawWireDisc(position + forward * rayRange * cosHalf, forward, rayRange * cos);
            DrawWireDisc(position + forward * rayRange * cosHalf / 2, forward, rayRange * cos / 2);
        }

        public static void DrawWireArcSimple(Vector3 position, float radius, Vector3 rayA, Vector3 rayB)
        {
            var rayAN = rayA.normalized;
            var rayBN = rayB.normalized;
            var up = Vector3.Cross(rayAN, rayBN);
            var dot = Vector3.Dot(rayAN, rayBN);
            var angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
            DrawWireArc(position, up, rayA, angle, radius);
        }

        public static void DrawWireArc(Vector3 position, Vector3 up, Vector3 from, float angle, float radius)
        {
#if UNITY_EDITOR
            Handles.color = Gizmos.color;
            Handles.DrawWireArc(position, up, @from, angle, radius);
#endif
        }

        public static void DrawWireDisc(Vector3 position, Vector3 normal, float radius)
        {
#if UNITY_EDITOR
            Handles.color = Gizmos.color;
            Handles.DrawWireDisc(position, normal, radius);
#endif
        }

        public static void DrawSolidDisc(Vector3 position, Vector3 normal, float radius)
        {
#if UNITY_EDITOR
            Handles.color = Gizmos.color;
            Handles.DrawSolidDisc(position, normal, radius);
#endif
        }

        public static bool FindValidSpawnPoint(this ISelectiveLogBehaviour logger, 
                                               Vector3 point, LayerMask groundMask, out Vector3 spawnPoint)
        {
            if (Physics.Raycast(point + new Vector3(0, 1000, 0),
                                Vector3.down,
                                out var hit,
                                4000,
                                groundMask))
            {
                spawnPoint = hit.point;
                return true;
            }
              //
            logger.LogBasic("No valid spawn point at " + point + " using ground mask " + groundMask.value);
            logger.LogBasic("Is Matched? " + groundMask.IsMatched(Terrain.activeTerrain.gameObject));
            logger.LogBasic("Anthing there? " + Physics.Raycast(point + new Vector3(0, 1000, 0), Vector3.down, 4000));
            spawnPoint = default;
            return false;
        }
    }
}
