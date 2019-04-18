# Weapon System

This module contains the necessary code to build projectile weapon systems. 
Although I use the term 'gun', this system works equally well for bows,
mortars and catapults.

[![Demo](weapon-system-demo.png)](https://youtu.be/Kr6Dx-Wtqs0)]

The weapon systems created with this library consist of three major parts:

* Weapon Definition (Scriptable Object)

  This object defines the core properties of a weapon, which projectile to 
  use, how it behaves when it is launched and how the projectiles are pooled.
  
* Guns

  Guns are the things that launch projectiles. The gun system has support for
  three firing modes:
  
  * ``manual firing``: blindly fires whereever the gun is pointed
  * ``assisted aiming`` that gently corrects the player's input to make firing
    more accurate without making it obvious to the player what happens
  * ``automatic firing`` which automatically selects targets and fires at them.
  
  Guns are paired with an target tracker which is responsible for finding 
  targets to shoot at. This library comes with three implementations by default:
  
  * ``ManualWeaponTargetTracker`` does not track any target at all. Use that if
    your gun does not need tracking at all. A gun equipped with this tracker will
    always fire directly ahead.
  * ``DefaultWeaponTargetTracker`` implements a suitable target tracking behaviour
    for computer controlled automatic weapons. It works either as fully automated
    tracker that freely chooses any target in range, or it acts as aiming assistant
    and therefore chooses a target that is as close as possible to the forward
    vector of the gun.
  * ``PlayerWeaponTargetTracker`` combines both previous trackers into one. 
    If there is a target, it can automatically aim at it and if there is no target
    it will happily shoot the air in front of the gun. Human players want to 
    shoot things regardless whether there are things to shoot at.
  
* Projectiles

  Projectiles are the things that are launched by guns. Projectiles can either
  be artificial (using a manually simulated object without a rigidbody) or 
  physical (uses the standard physics engine to move the object).
  
  Projectiles can be affected by gravity to be proper ballistic objects that 
  fly in an arc towards the targer or they can ignore gravity to behave like
  lasers. The system contains the necessary code to allow you to correctly aim
  the guns to hit with either type of projectiles.
  

  