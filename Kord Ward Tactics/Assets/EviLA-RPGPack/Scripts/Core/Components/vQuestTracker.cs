using UnityEngine;

namespace EviLA.AddOns.RPGPack
{
    public class vQuestTracker : MonoBehaviour
    {
        public int questID;
        private bool originalActiveState;

        public void Start()
        {
            originalActiveState = this.gameObject.activeSelf;
        }

        public void RestoreState()
        {
            this.gameObject.SetActive(originalActiveState);
        }
    }
}
