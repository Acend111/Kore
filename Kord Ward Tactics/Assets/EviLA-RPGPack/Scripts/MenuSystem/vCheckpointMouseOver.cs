using UnityEngine;
using Invector;

namespace EviLA.AddOns.RPGPack.MenuSystem
{

    public class vCheckpointMouseOver : vMonoBehaviour
	{
		[HideInInspector]
		public bool isOver = false;

		public void OnPointerEnter()
		{
			isOver = true;
			var checkpoint = GetComponent<vCheckPointSlot> ();
			var loader = GetComponentInParent<vCheckpointLoader> ();
			loader.ShowThumbnail (checkpoint);
		}

		public void OnPointerExit()
		{
			var loader = GetComponentInParent<vCheckpointLoader> ();
			loader.HideThumbnail ();
		}
	}

}