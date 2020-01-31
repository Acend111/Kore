using Invector.vItemManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EviLA.AddOns.RPGPack.Experience
{
    public static class ItemManagerExtensions
    {
        public static EquipPoint GetCurrentlyEquippedWeapon(this vItemManager itemManager, vItemType weaponType, string equipPointName = null)
        {
            try
            {
                if (equipPointName == null)
                {
                    var ep = itemManager.equipPoints.Find(e => e.equipmentReference.item != null && e.equipmentReference.item.type == weaponType);
                    return ep;
                }
                else
                {
                    var ep = itemManager.equipPoints.Find(e => e.equipPointName.Equals(equipPointName) &&
                                                                e.equipmentReference.item != null && e.equipmentReference.item.type == weaponType);
                    return ep;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

    }
}
