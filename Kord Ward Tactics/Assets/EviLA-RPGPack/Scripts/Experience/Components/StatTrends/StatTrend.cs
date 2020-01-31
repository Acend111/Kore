using Invector;
using UnityEngine;

namespace EviLA.AddOns.RPGPack.Experience
{ 
	[vClassHeader("Stat Trend", "Stat Trend Editor", iconName = "icon_v2")]
    public class StatTrend : vMonoBehaviour
    {
		[vEditorToolbar("Trend Details")]
        public string trendID;
        public BaseStatTypes type;
        public bool isPercentage;
        public bool isNumeric;
        public bool isBool;
        public string value;
        public bool applyOnLevelUp;
		[vEditorToolbar("Trend Curve")]
        public AnimationCurve trend;
    }
}
