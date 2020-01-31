using UnityEngine;
using EviLA.AddOns.RPGPack.Persistence;
using Invector;

namespace EviLA.AddOns.RPGPack.MenuSystem
{

    public class vSaveSlot : vMonoBehaviour
    {

        [HideInInspector]
        public int gameID;

        public void UpdateSlot()
        {
            vQuestSystemLevelLoader.instance.UpdateGameID(gameID);
        }
    }
}