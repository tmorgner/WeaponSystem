using System;
using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    /// <summary>
    /// <para>
    ///   A basic state machine that encodes the state cycle of projectile weapons.
    ///   If a weapon has no target, the weapon is in the <see cref="WeaponState.Idle"/>
    ///   state. Once a target is found, the weapon will enter the <see cref="WeaponState.Charging"/>
    ///   state that allows the gun to control the rate of fire and play some effects along the way.
    ///   Once the charge time is up, the projectile enters the <see cref="WeaponState.WillFire"/>
    ///   state and will fire during the next update. It then enters the <see cref="WeaponState.Fired"/>
    ///   state that simulates a cool down before going back into the Idle state.
    ///</para>
    /// <para>
    ///   A weapon will change state once per update call. It therefore takes at least 2 frames
    ///   (if there is no charging and cooldown time) to fire a gun.
    /// </para>
    /// </summary>
    public class WeaponStateTracker
    {
        /// <summary>
        ///   A empty dummy object to avoid null checks.
        /// </summary>
        class NullWeaponBehaviour : IWeaponBehaviour
        {
            public void BeginIdle()
            {
            }

            public float BeginCharging()
            {
                return 0;
            }

            public void WhileCharging(float percentComplete)
            {
            }

            public float BeginFire(ref Vector3 target)
            {
                return 0;
            }

            public void WhileCoolDown(float percentComplete)
            {
            }
        }

        static readonly NullWeaponBehaviour nullBehaviour = new NullWeaponBehaviour();
        float chargeCompleteTime;
        float chargeStartedTime;
        IWeaponBehaviour weaponBehaviour;

        /// <summary>
        ///   The current weapon behaviour.
        /// </summary>
        public IWeaponBehaviour WeaponBehaviour
        {
            get => ReferenceEquals(weaponBehaviour, nullBehaviour) ? null : weaponBehaviour;
            set
            {
                weaponBehaviour = value ?? nullBehaviour;
            }
        }


        /// <summary>
        ///  Represents the current weapon state.
        /// </summary>
        public WeaponState State
        {
            get;
            private set;
        }

        /// <summary>
        ///   Updates the current state for the given target. Use <see cref="MarkIdle"/> if you have no target. 
        /// </summary>
        /// <param name="target">The target to fire at</param>
        public void Update(Vector3 target)
        {

            switch (State)
            {
                case WeaponState.Idle:
                {
                    var chargeTime = weaponBehaviour.BeginCharging();
                    if (chargeTime > 0)
                    {
                        chargeStartedTime = Time.time;
                        chargeCompleteTime = chargeStartedTime + chargeTime;
                        State = WeaponState.Charging;
                    }
                    else
                    {
                        State = WeaponState.WillFire;
                    }

                    break;
                }
                case WeaponState.Charging:
                {
                    if (Time.time < chargeCompleteTime)
                    {
                        var chargeTime = chargeCompleteTime - chargeStartedTime;
                        var chargeTimePassed = Time.time - chargeStartedTime;
                        var percentComplete = Mathf.Clamp01(chargeTimePassed / chargeTime);
                        weaponBehaviour.WhileCharging(percentComplete);
                    }
                    else
                    {
                        State = WeaponState.WillFire;
                    }

                    break;
                }
                case WeaponState.WillFire:
                {
                    var cooldown = weaponBehaviour.BeginFire(ref target);
                    if (cooldown > 0)
                    {
                        chargeStartedTime = Time.time;
                        chargeCompleteTime = chargeStartedTime + cooldown;
                        State = WeaponState.Fired;
                    }
                    else
                    {
                        MarkIdle();
                    }
                    break;
                }
                case WeaponState.Fired:
                {
                    HandleCoolDown();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void HandleCoolDown()
        {
            if (Time.time < chargeCompleteTime)
            {
                var chargeTime = chargeCompleteTime - chargeStartedTime;
                var chargeTimePassed = Time.time - chargeStartedTime;
                var percentComplete = Mathf.Clamp01(chargeTimePassed / chargeTime);
                weaponBehaviour.WhileCoolDown(percentComplete);
            }
            else
            {
                State = WeaponState.Idle;
            }
        }

        /// <summary>
        ///   Updates the state of the state machine when there is no target.
        /// </summary>
        public void MarkIdle()
        {
            if (State == WeaponState.Fired)
            {
                HandleCoolDown();
                return;
            }

            if (State != WeaponState.Idle)
            {
                weaponBehaviour.BeginIdle();
            }
            State = WeaponState.Idle;
        }

    }
}