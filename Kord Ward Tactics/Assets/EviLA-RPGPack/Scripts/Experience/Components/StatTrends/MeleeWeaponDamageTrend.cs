namespace EviLA.AddOns.RPGPack.Experience
{
    public class MeleeWeaponDamageTrend : StatTrend
    {
        public MeleeWeaponStatTypes monitoredStat;

        public void Awake()
        {
            type = BaseStatTypes.MeleeWeapon;
        }
    }
}
