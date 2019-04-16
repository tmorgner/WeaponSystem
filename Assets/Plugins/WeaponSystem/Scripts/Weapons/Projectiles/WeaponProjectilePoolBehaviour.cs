using System;
using System.Collections.Generic;
using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Projectiles
{
    public class WeaponProjectilePoolBehaviour : MonoBehaviour
    {
        public struct GameObjectCarrier : IEquatable<GameObjectCarrier>
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
        [SerializeField] WeaponProjectile prefab;
        [SerializeField] int poolSizeLimit;
        [SerializeField] bool strictLimit;
        int counter;

        public bool StrictLimit
        {
            get => strictLimit;
            set => strictLimit = value;
        }

        public WeaponProjectile Prefab
        {
            get => prefab;
            set => prefab = value;
        }

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

        public bool TryGet(out WeaponProjectile ray)
        {
            if (prefab == null)
            {
                Debug.Log("No prefab");
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
                    // Debug.Log("Size Limit reached");
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

        public void PreAllocate(int count)
        {
            for(var c = 0; c < count; c +=1 )
            {
                if (pooledObjects.Count < poolSizeLimit)
                {
                    // Debug.Log("Recovered pooled object " + ray);
                    var ray = AllocateObject();
                    ray.gameObject.SetActive(false);
                    pooledObjects.Add(ray);
                }
                else
                {
                    return;
                }
            }
        }

        public void Activate()
        {
            prefab.gameObject.SetActive(false);
        }
    }
}