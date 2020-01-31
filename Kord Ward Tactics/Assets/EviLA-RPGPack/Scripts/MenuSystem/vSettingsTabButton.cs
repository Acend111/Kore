using UnityEngine;
using UnityEngine.UI;
using Invector;

namespace EviLA.AddOns.RPGPack.MenuSystem {

	public class vSettingsTabButton : vMonoBehaviour {

		[HideInInspector]
		public Button button;
		public Image background;

		public void Start()
		{
			button = GetComponent<Button> ();
		}

	}

}
