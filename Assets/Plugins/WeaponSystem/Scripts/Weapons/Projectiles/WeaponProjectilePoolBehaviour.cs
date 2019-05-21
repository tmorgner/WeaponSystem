using System;
using System.Collections.Generic;
using System.Diagnostics;
using NaughtyAttributes;
using RabbitStewdio.Unity.UnityTools;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Projectiles
{
    /// <summary>
    ///   A weapon projectile pool. This class manages a set of preallocated
    ///   weapon projectiles to reduce pressure on the garbage collector.
    /// </summary>
    /// <remarks>
    ///   You should never need to instantiate this class manually, as it is
    ///   intended to only ever be used within a WeaponProjectilePool service.
    /// </remarks>
    [ExcludeFromObjectFactoryAttribute, ExcludeFromPresetAttribute]
    public class WeaponProjectilePoolBehaviour : MonoBehaviour
    {
        /// <summary>
        ///   A Game object wrapper that avoids callbacks into Unity's native core
        ///   when comparing objects for referential equality.
        /// </summary>
        struct GameObjectCarrier : IEquatable<GameObjectCarrier>
        {
            public readonly WeaponProjectile GameObject;
            readonly int id;

            public GameObjectCarrier(WeaponProjectile gameObject) : this()
            {
                this.GameObject = gameObject;
                this.id = gameObject.GetInstanceID();
            }

            public bool Equals(GameObjectCarrier other)
            {
                return id == other.id;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is GameObjectCarrier other && Equals(other);
            }

            public override int GetHashCode()
            {
                return id;
            }

            public static bool operator ==(GameObjectCarrier left, GameObjectCarrier right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(GameObjectCarrier left, GameObjectCarrier right)
            {
                return !left.Equals(right);
            }
        }

        readonly HashSet<GameObjectCarrier> activeObjects;
        readonly List<WeaponProjectile> pooledObjects;
        
        [ReadOnly]
        [Tooltip("[Auto] A weapon projectile prefab that is used in the pool. This property is set by the service object.")]
        [SerializeField] WeaponProjectile prefab;
        [ReadOnly]
        [Tooltip("[Auto] The maximum number of projectiles stored in the pool. This property is set by the service object.")]
        [SerializeField] int poolSizeLimit;
        [ReadOnly]
        [Tooltip("[Auto] Indicates whether the pool will generate more projectiles even if when at full capacity.")]
        [SerializeField] bool strictLimit;
        int counter;
        int preallocationRequest;
        Stopwatch preallocationTimer;
        
        /// <summary>
        /// Indicates whether the pool will generate more projectiles even if when at full capacity.
        /// </summary>
        public bool StrictLimit
        {
            get => strictLimit;
            set => strictLimit = value;
        }

        /// <summary>
        /// A weapon projectile prefab that is used in the pool. This property is set by the service object.
        /// </summary>
        public WeaponProjectile Prefab
        {
            get => prefab;
            set => prefab = value;
        }

        /// <summary>
        /// The maximum number of projectiles stored in the pool. This property is set by the service object.
        /// </summary>
        public int PoolSizeLimit
        {
            get => poolSizeLimit;
            set => poolSizeLimit = value;
        }

        public WeaponProjectilePoolBehaviour()
        {
            pooledObjects = new List<WeaponProjectile>();
            activeObjects = new HashSet<GameObjectCarrier>();
        }

        /// <summary>
        ///  Attempts to reserve a new weapon projectile instance from the pool.
        /// </summary>
        /// <param name="ray">The projectile that is reserved, or null if the pool cannot provide an instance.</param>
        /// <returns>true if a projectile is available, false otherwise. </returns>
        public bool TryGet(out WeaponProjectile ray)
        {
            if (prefab == null)
            {
                Debug.LogError("The pool instance " + gameObject.GetPath() + " has no valid projectile prefab.");
                ray = null;
                return false;
            }

            if (pooledObjects.Count > 0)
            {
                ray = pooledObjects[pooledObjects.Count - 1];
                pooledObjects.RemoveAt(pooledObjects.Count - 1);
                activeObjects.Add(new GameObjectCarrier(ray));
                ray.gameObject.SetActive(true);
                return true;
            }

            if (strictLimit)
            {
                if (activeObjects.Count >= poolSizeLimit)
                {
                    ray = null;
                    return false;
                }
            }

            ray = AllocateObject();
            activeObjects.Add(new GameObjectCarrier(ray));
            ray.gameObject.SetActive(true);
            return true;
        }

        WeaponProjectile AllocateObject()
        {
            counter += 1;
            var ray = Instantiate(prefab, transform, true);
            ray.Pool = this;
            var rayGameObject = ray.gameObject;
            rayGameObject.name = $"{prefab.name}#{rayGameObject.GetInstanceID():X} ({counter}";
            ApplyHideFlags(rayGameObject, gameObject.hideFlags);
            return ray;
        }

        /// <summary>
        ///   Reclaims the given weapon projectile for the pool.
        /// </summary>
        /// <param name="ray">The projectile that should be reclaimed.</param>
        public void Release(WeaponProjectile ray)
        {
            ray.gameObject.SetActive(false);

            if (!activeObjects.Remove(new GameObjectCarrier(ray)))
            {
                Debug.LogWarning("Attempting to release non active object: " + ray);
                pooledObjects.Remove(ray);
                ray.Pool = null;
                Destroy(ray.gameObject);
                return;
            }

            if (pooledObjects.Count < poolSizeLimit)
            {
                // Debug.Log("Recovered pooled object " + ray);
                pooledObjects.Add(ray);
                return;
            }


            ray.Pool = null;
            Destroy(ray.gameObject);
        }

        static void ApplyHideFlags(GameObject go, HideFlags flags)
        {
            go.hideFlags = HideFlags.DontSave;
            var transform = go.transform;
            var count = transform.childCount;
            for (int i = 0; i < count; i += 1)
            {
                ApplyHideFlags(transform.GetChild(i).gameObject, flags);
            }
        }

        /// <summary>
        ///   Releases all projectiles from the pool. This destroys all projectiles in the process.
        /// </summary>
        public void ClearPool()
        {
            foreach (var activeObject in activeObjects)
            {
                Destroy(activeObject.GameObject.gameObject);
            }

            foreach (var pooledObject in pooledObjects)
            {
                Destroy(pooledObject.gameObject);
            }
        }

        /// <summary>
        ///   Pre-allocates the given number of projectiles in this pool.
        /// </summary>
        /// <param name="count"></param>
        public void PreAllocate(int count)
        {
            preallocationRequest = Math.Max(0, Math.Min(count, poolSizeLimit));
            if (preallocationRequest < pooledObjects.Count)
            {
                preallocationTimer = new Stopwatch();
                enabled = true;
            }
        }

        void PreallocateObjects()
        {
            preallocationTimer.Restart();
            
            for(var c = pooledObjects.Count; c < preallocationRequest; c +=1 )
            {
                if (pooledObjects.Count >= poolSizeLimit)
                {
                    preallocationTimer = null;
                    preallocationRequest = 0;
                    return;
                }

                var ray = AllocateObject();
                ray.gameObject.SetActive(false);
                pooledObjects.Add(ray);

                if (preallocationTimer.Elapsed.TotalMilliseconds > 0.25)
                {
                    // ran out of time budget ..
                    return;
                }
            }

        }

        void Update()
        {
            if (preallocationRequest == 0)
            {
                return;
            }

            PreallocateObjects();
            if (pooledObjects.Count >= preallocationRequest)
            {
                enabled = false;
            }            
        }

        /// <summary>
        ///   Ensures that the prefab is inactive. This has the side effect of
        ///   making the prefab-asset inactive in the editor, however it is
        ///   the only way we can instantiate objects without triggering
        ///   'OnEnable' calls.
        /// </summary>
        public void Activate()
        {
            prefab.gameObject.SetActive(false);
            
        }
    }
}