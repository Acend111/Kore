namespace EviLA.AddOns.RPGPack.Experience
{
    public class ShooterWeaponDamageTrend : StatTrend
    {
        public ShooterWeaponStatTypes monitoredStat;

        public void Awake()
        {
            type = BaseStatTypes.ShooterWeapon;
        }
    }
}
