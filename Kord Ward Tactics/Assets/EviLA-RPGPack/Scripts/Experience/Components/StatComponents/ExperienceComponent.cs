using Invector.vCharacterController;
using UnityEngine;

namespace EviLA.AddOns.RPGPack.Experience
{
    public class ExperienceComponent : MonoBehaviour
    {
        public int experienceCost;
        public bool destroyAfterUse;
        private bool used;

        public void AddExperienceFromComponent()
        {
            if (!used)
            {
                var manager = vThirdPersonController.instance.GetComponent<vLevelManager>();
                manager.AddExperience(this.experienceCost);
                used = true;
            }
        }
    }
}