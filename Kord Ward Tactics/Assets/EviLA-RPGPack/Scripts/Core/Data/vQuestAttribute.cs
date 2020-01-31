using System.Collections.Generic;

namespace EviLA.AddOns.RPGPack
{
    [System.Serializable]
    public class vQuestAttribute
    {
		public vQuestAttributes name = 0;     
		   
        public int value;
        public bool isBool;
		public vQuestAttribute(vQuestAttributes name, int value)
        {
            this.name = name;
            this.value = value;
        }
    }

    public static class vQuestAttributeHelper
    {
		public static bool Contains(this List<vQuestAttribute> attributes, vQuestAttributes name)
        {
            var attribute = attributes.Find(at => at.name == name);
            return attribute != null;
        }
		public static vQuestAttribute GetAttributeByType(this List<vQuestAttribute> attributes, vQuestAttributes name)
        {
            var attribute = attributes.Find(at => at.name == name);
            return attribute;
        }
		public static bool Equals(this vQuestAttribute attributeA, vQuestAttribute attributeB)
        {
            return attributeA.name == attributeB.name;
        }

		public static List<vQuestAttribute> CopyAsNew(this List<vQuestAttribute> copy)
        {
			var target = new List<vQuestAttribute>();

            if (copy != null)
            {
                for (int i = 0; i < copy.Count; i++)
                {
					vQuestAttribute attribute = new vQuestAttribute(copy[i].name, copy[i].value);                  
                    target.Add(attribute);
                }
            }
            return target;
        }
    }

}
