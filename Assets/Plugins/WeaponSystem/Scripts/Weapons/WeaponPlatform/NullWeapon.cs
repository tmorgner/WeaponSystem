namespace RabbitStewdio.Unity.WeaponSystem.Weapons.WeaponPlatform
{
    /// <summary>
    ///   A no-op weapon. Use this to simulate a state where the
    ///   player wants to be peaceful and has put his weapons away.
    /// </summary>
    public class NullWeapon : PlayerWeapon
    {
        /// <summary>
        ///   NullWeapons have no range.
        /// </summary>
        public override float Range => 0;

        /// <summary>
        ///   NullWeapons have no weapon definition.
        /// </summary>
        public override bool TryGetWeaponDefinition(out WeaponDefinition w)
        {
            w = default;
            return false;
        }
    }
}