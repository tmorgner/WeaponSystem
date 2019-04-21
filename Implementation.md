# Implementation Notes

The weapon system contained in this project is an extensible
system that allows you to create a wide range of projectile
weapons without having to write complex scripts.

The projectile weapons created with this system require a 
``WeaponDefinition``. The weapon definition holds all properties
a weapon needs to function. At bare minimum you'll need to
provide a ``WeaponProjectile`` prefab and fill in some sensible
values for the velocity, target range and tracking parameters.

Internally the weapon definition will instantiate a ProjectilePool
service object to handle the pooling of spawned projectiles. 

## Service objects

A service object is a scriptable-object that controls a 
MonoBehaviour to connect with the Unity runtime events and other
game objects in the scene.

Service objects are not singletons, and thus share none of their
awful drawbacks (like unclean dependency structures). As a
scriptable object, service objects are defined in the Unity 
asset database and can be assigned/injected into prefabs and 
scene objects. As the handle for the service lives outside of
the scene, prefabs can be fully configured at the asset level
and can share references to the same service. 

Being a scriptable object asset, service objects do not limit 
how many of these assets can be created. You can have as many
service object definitions as needed, including specialised
instances for Unity and integration testing.

As service objects are injected via the inspector, there are 
no hidden references buried deep in the code. Your prefabs 
inspector always show exactly which instance is used at runtime.

## Projectiles

Weapon projectiles are standard game objects. As these objects
are pooled, you should avoid any operation that destroys these
objects outside of the pool's own management functions.

Projectiles should follow a basic component / game object structure:

    + Projectile Main
        |
        +-- Muzzle-Effect
        |
        +-- Body-Effect
        |
        +-- Hit-Effect
        
The projectile main object should contain the ``WeaponProjectile``
script. This script is implemented either as ``ArtificialWeaponProjectile``
for kinematic movement or ``PhysicalWeaponProjectile`` for 
physics based movement using forces.

The weapon projectile script manages the whole lifecycle of the
object. The three effect objects, when provided, should have
behaviours attached that play on awake and that disable, but not
destroy, the effect object. The scripts must be able to be restarted
by disabling and re-enabling the effect game object.

Projectile prefabs should be saved as disabled by default so that
pooling does not activate them before they are fired by a gun.

(As a conveniece method both the hit and muzzle effect game object will
automatically receive a ``DisableWhenEffectsFinished`` MonoBehaviour
to track the state of any animation, particle effect or sound currently
playing on the game object. This script will disable the game object
when all effect systems stopped playing. This obviously means that 
you should not add any 'looping' behaviour to your effects if you want
this automatic management to work.)

When a projectile is fired the system will test whether the projectile
would immediately (or within a single frame) hit a target. If so, it 
will not activate the body effect game-object as a simple optimization 
and will immediately inform the target that is has been hit.

Hit testing happens via forward looking raycasts. Each frame the 
projectile shoots a short ray forward to see whether a collision
occurs. Bullets are fast-moving small objects and using the standard
discrete step physics system projectiles might teleport through walls
or other thinner objects. By using a raycast the projectile augments
the exiting physics checks at relatively low cost.

## Guns

A gun is a projectile launcher system. In most circumstances you
should be able to simply use the ``AutomaticGun`` script for all
your needs.

Guns use an internal state machine to keep track of its current 
state. A gun can be in the following states:

* Idle - there is no target or the gun is not allowed to fire
* Charging - a target has been found, prepares to fire
* WillFire - actually launches the projectile
* Fired - a cool-down mode. 

Using the current implementation, a gun can fire a projectile every
two frames (by skipping the charging and fired stages). Assuming a 
framerate of 60 FPS the maximum achievable rate of fire is 1800 bullets
spawned per minute. However, the library is designed for more sane
rates of fire and the fire rate becomes frame rate independent as 
soon as the weapon has either a charging time or a cool-down time
defined in its ``WeaponDefintion`` object.

Guns use a WeaponTargetTracker to acquire targets. The currently 
implemented tracker uses a trigger collider in front of the gun to
sweep the tracking area for potential targets. 

If the WeaponDefinition has a target set defined, each possible 
target detected by the colliders will also be checked against the
target set defined in the weapon definition. The target set is a 
``SmartRigidbodySet``, a ScriptableObject that maintains an up to
date list of active rigidbodies in the scene. Set membership for 
objects is managed via the ``SmartRuntimeSetMemberBehaviour``
script. Unlike Unity's tag or layer system, a game object can be 
member in as many sets as needed.

The SmartSet components of UnityTools are based on ideas expressed 
in [Ryan Hipple's Unite Austin 2017 talk](https://github.com/roboryantron/Unite2017)
without the over engineering found in the 
[SmartData Project](https://github.com/sigtrapgames/SmartData).

You can influence the target selection by providing a 
``TargetSelectionStrategy`` instance. Default instances are provided
in the library, and you can easily implement your own strategies
by subclassing the ``TargetSelectionStrategy`` scriptable object.

Based on the aiming mode used for the target tracker, the tracker
will then take the weighted preferences expressed by the ``TargetSelectionStrategy``
and will select the actual target to shoot at. If the aiming
mode is assisted, the target tracker will prefer any target that
is closest to the flight path of the bullet to maintain the 
illusion that the player can actually shoot well, while an automatic
aiming mode will select the target closest to the gun.

## Player weapons and Weapon platforms 

Using a pure AutomaticGun script is usually sufficient for 
statically mounted guns. However, if you want to build more 
complex weapon systems that combine multiple, independently
movable weapons (like tanks, or human players in a first-person
shooter) you will need the WeaponPlatform package.

The WeaponPlatform package contains additional scripts that make
it possible to implement weapons that are controlled by a human
player. 

PlayerWeaponTargetTracker:
   This class modifies the standard target tracker to always 
   report a target. Players love to shoot into thin air (a 
   most dangerous foe) and thus this class simulates a fallback
   target so that guns can always fire.
   
IPointerDirectionService:
   provides an interface to a pointing device (light guns, 
   VR hand controls or headsets) that can be used to modify the
   targetting or weapon rotation independent of the game object
   that carries the weapon.
   
PlayerWeaponDirectionTracker:
   Uses a PointerDirectionService to align the gun with the
   player's view or pointer direction.
   
WeaponControl:
   is a facade into the player controller. It provides means for
   the weapon system to query the currently active weapon and
   the player's intent to fire (usually a button press).
   
PlayerWeapon:
   A controller script that bridges WeaponControl and Guns to
   keep both sides of the implementation decoupled.
   