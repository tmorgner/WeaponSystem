using RabbitStewdio.Unity.WeaponSystem.Weapons;
using RabbitStewdio.Unity.WeaponSystem.Weapons.Guns;
using UnityEngine;
using UnityEngine.TestTools;

public class ValidateHitTest : MonoBehaviour, IMonoBehaviourTest
{
    float time;

    public GameObject target;
    public GunBase gun;
    public ValidateHitDetector hitDetector;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start Called");
        time = Time.time;

        target = GameObject.Find("Target");
        hitDetector = target.AddComponent<ValidateHitDetector>();

        gun = FindObjectOfType<GunBase>();
        gun.Armed = true;
    }

    public bool IsTestFinished
    {
        get
        {
            // This method is called before the Start method has been invoked.
            if (hitDetector == null)
            {
                return false;
            }

            return hitDetector.done || Time.time - time > 15;
        }
    }

    public class ValidateHitDetector: MonoBehaviour, IHitReceiver
    {
        public bool done;
        public bool hit;
        public Vector3 point;
        public IHitDamageSource projectile;
        public GameObject source;

        void OnCollisionEnter(Collision other)
        {
            Debug.Log("OnCollision Hit");
            hit = true;
            done = true;
        }

        public void OnReceivedHit(GameObject source, IHitDamageSource projectile, Vector3 point)
        {
            Debug.Log("OnReceived Hit");
            this.source = source;
            this.projectile = projectile;
            this.point = point;
            done = true;
        }
    }
}

