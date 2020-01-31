using UnityEngine;
using Invector;

namespace EviLA.AddOns.RPGPack.MenuSystem {

	public class vSettingsTab : vMonoBehaviour {

		public vSettingsTabButton button;
		public GameObject panel;
		public Settings setting;

		private bool isEnabled;
		private bool childTab = false;

		public void Start()
		{
			var tab = gameObject.transform.parent.GetComponentInParent<vSettingsTab> ();
			if (tab != null)
				childTab = true;
		}

		public void SelectThis()
		{
			button.background.gameObject.SetActive (true);
			button.background.enabled = true;
			panel.SetActive (true);
			isEnabled = true;
		}

		public void DeSelectThis()
		{
			button.background.gameObject.SetActive (false);
			panel.SetActive (false);
			isEnabled = false;
		}

		public bool Selected
		{
			get {
				return isEnabled;
			}
		}

		public bool IsChildTab
		{
			get
			{ 
				return childTab;
			}
		}

	}

}