using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Projectiles
{
    public class LineRendererWeaponProjectileTracer : MonoBehaviour
    {
        ArtificialWeaponProjectile projectile;
        LineRenderer lineRenderer;
        bool hasProjectile;
        bool hasLineRenderer;

        void Awake()
        {
            projectile = GetComponentInParent<ArtificialWeaponProjectile>();
            if (projectile != null)
            {
                hasProjectile = true;
            }

            lineRenderer = GetComponentInChildren<LineRenderer>();
            if (lineRenderer != null)
            {
                hasLineRenderer = true;
            }
        }

        void OnEnable()
        {
            if (hasLineRenderer && hasProjectile)
            {
                lineRenderer.SetPosition(0, Vector3.zero);
                lineRenderer.SetPosition(1, Vector3.forward * (projectile.RayLength));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}