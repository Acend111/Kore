using UnityEngine;
using System.Collections.Generic;

namespace EviLA.AddOns.RPGPack
{
    public class vQuestListData : ScriptableObject
    {
        public List<vQuest> quests = new List<vQuest>();       
       
        public bool inEdition;
       
        public bool itemsHidden = true;
        
    }

}
