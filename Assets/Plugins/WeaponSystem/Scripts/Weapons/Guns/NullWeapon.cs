namespace RabbitStewdio.Unity.WeaponSystem.Weapons.Guns
{
    public class NullWeapon : ShipWeapon
    {
        public override float Range => 0;

        public override bool TryGetWeaponDefinition(out WeaponDefinition w)
        {
            w = default;
            return false;
        }
    }
}