namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    /// <summary>
    ///   A weapon state enumeration.
    /// </summary>
    public enum WeaponState
    {
        /// <summary>
        ///  Indicates the weapon is idle.
        /// </summary>
        Idle, 
        /// <summary>
        ///  Indicates the weapon is waiting to be charged.
        /// </summary>
        Charging, 
        /// <summary>
        ///  Indicates the weapon will fire during the next frame.
        /// </summary>
        WillFire, 
        /// <summary>
        ///  Indicates the weapon has fired and is currently cooling down.
        /// </summary>
        Fired
    }
}