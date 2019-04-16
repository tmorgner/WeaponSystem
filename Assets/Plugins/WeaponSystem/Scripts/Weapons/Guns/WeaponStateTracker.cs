using System;
using UnityEngine;

namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    public class WeaponStateTracker
    {
        public enum WeaponState
        {
            Idle, Charging, WillFire, Fired
        }

        public IWeaponBehaviour WeaponBehaviour { get; set; }
        float chargeCompleteTime;
        float chargeStartedTime;

        public WeaponState State
        {
            get;
            private set;
        }

        public void Update(Vector3 target)
        {

            switch (State)
            {
                case WeaponStateTracker.WeaponState.Idle:
                {
                    var chargeTime = WeaponBehaviour?.BeginCharging() ?? 0f;
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
                case WeaponStateTracker.WeaponState.Charging:
                {
                    if (Time.time < chargeCompleteTime)
                    {
                        var chargeTime = chargeCompleteTime - chargeStartedTime;
                        var chargeTimePassed = Time.time - chargeStartedTime;
                        var percentComplete = Mathf.Clamp01(chargeTimePassed / chargeTime);
                        WeaponBehaviour?.WhileCharging(percentComplete);
                    }
                    else
                    {
                        State = WeaponStateTracker.WeaponState.WillFire;
                    }

                    break;
                }
                case WeaponStateTracker.WeaponState.WillFire:
                {
                    var cooldown = WeaponBehaviour?.BeginFire(ref target) ?? 0;
                    if (cooldown > 0)
                    {
                        chargeStartedTime = Time.time;
                        chargeCompleteTime = chargeStartedTime + cooldown;
                        State = WeaponStateTracker.WeaponState.Fired;
                    }
                    else
                    {
                        MarkIdle();
                    }
                    break;
                }
                case WeaponStateTracker.WeaponState.Fired:
                {
                    HandleCooldown();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void HandleCooldown()
        {
            if (Time.time < chargeCompleteTime)
            {
                var chargeTime = chargeCompleteTime - chargeStartedTime;
                var chargeTimePassed = Time.time - chargeStartedTime;
                var percentComplete = Mathf.Clamp01(chargeTimePassed / chargeTime);
                WeaponBehaviour?.WhileCoolDown(percentComplete);
            }
            else
            {
                State = WeaponStateTracker.WeaponState.Idle;
            }
        }

        public void MarkIdle()
        {
            if (State == WeaponState.Fired)
            {
                HandleCooldown();
                return;
            }

            if (State != WeaponState.Idle)
            {
                WeaponBehaviour?.BeginIdle();
            }
            State = WeaponState.Idle;
        }

    }
}