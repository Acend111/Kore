using UnityEngine;
using EviLA.AddOns.RPGPack.Persistence;
using Invector;

namespace EviLA.AddOns.RPGPack.MenuSystem
{

    public class vCheckPointSlot : vMonoBehaviour {

		[HideInInspector]
		public int checkpointID;

		public void UpdateSlot()
		{
			vQuestSystemLevelLoader.instance.UpdateCheckPointID (checkpointID);
		}
	}

}