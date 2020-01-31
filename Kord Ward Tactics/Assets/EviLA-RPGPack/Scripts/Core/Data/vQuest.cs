using UnityEngine;
using System;
using System.Collections.Generic;
using Invector.vItemManager;

namespace EviLA.AddOns.RPGPack
{
    public interface IQuestTarget
    {
        bool isTarget();
        bool isSeller();
        bool isProvider();
        string getTargetName();
    }

    [System.Serializable]
    public class vQuest : ScriptableObject
    {
        #region SerializedProperties in customEditor
        [HideInInspector]
        public int id;
        [HideInInspector]
        public string objective = "Quest Objective";
        [HideInInspector]
        public string description = "Quest Description";
        [HideInInspector]
        public vQuestType type;
        [HideInInspector]
        public vQuestState state;
        [HideInInspector]
        public Sprite icon;
        [HideInInspector]
        public List<vQuestAttribute> attributes = new List<vQuestAttribute>();
        [HideInInspector]
        public vQuestProvider provider;
        [HideInInspector]
        public bool isInEquipArea;
        [HideInInspector]
        public bool isAccepted = false;
        [HideInInspector]
        public bool isInitiallyActive = false;
        [HideInInspector]
        public vQuest parent = null;
        [HideInInspector]
        public vItem gatherItem;
        [HideInInspector]
        public vQuestTargetType targetType;
        [HideInInspector]
        public GameObject Target;
        [SerializeField]
        private string _targetName;
        [HideInInspector]
        private IQuestTarget _target;
        [HideInInspector]
        public vItem killWithThis;
        [HideInInspector]
        public string tagObjectTag;
        [HideInInspector]
        public Transform reloadLocationOnDecline;

        public IQuestTarget QuestTarget
        {
            get
            {
                if (_target == null)
                {
                    var type = Activator.CreateInstance(typeof(vQuest).Assembly.FullName, "EviLA.AddOns.RPGPack." + targetType.ToString()).Unwrap().GetType();
                    var targets = FindObjectsOfType(type);
                    foreach (var target in targets)
                    {
                        IQuestTarget _t = (IQuestTarget)target;
                        if (_t.getTargetName().Equals(_targetName))
                        {
                            _target = _t;
                            break;
                        }
                    }
                }
                return _target;
            }
            set
            {
                if (value == null)
                {
                    _target = null;
                    _targetName = "";
                    return;
                }

                if (!value.getTargetName().Equals(""))
                {
                    _target = value;
                    _targetName = value.getTargetName();
                }
            }
        }

        //        public string QuestTargetName{
        //        	get { 
        //        		return _targetName;
        //        	}
        //        }


        #endregion

        #region Secondary Quest List Data

        [Header("---Secondary Quest Settings---")]

        [Header("Secondary Quest List")]
        public vQuestListData secondaryQuestListData;
        [HideInInspector]
        public List<QuestReference> secondaryQuestReferenceList;


        [HideInInspector]
        public List<vQuest> filteredQuests;
        [Header("Secondary Quest Type Filter")]
        public List<vQuestType> secondaryQuestTypeFilter = new List<vQuestType>() { 0 };

        [HideInInspector]
        public List<vQuest> SecondaryQuests
        {
            get
            {
                CleanBlankReferences();

                var secondaryQuestList = new List<vQuest>();

                secondaryQuestReferenceList.ForEach(q =>
                {
                    var secondary = secondaryQuestListData.quests.Find(q2 => q2.id == q.id);
                    if (!secondaryQuestList.Contains(secondary))
                        secondaryQuestList.Add(secondary);
                });
                secondaryQuestList.ForEach(q =>
                {
                    q.parent = this;
                });

                return secondaryQuestList;
            }
        }

        #endregion

        #region Dependency Quest List Data

        [Header("---Dependency Quest Settings---")]

        [Header("Dependency Quest List")]
        public vQuestListData dependencyQuestListData;
        [HideInInspector]
        public List<QuestReference> dependencyQuestReferenceList;


        [HideInInspector]
        public List<vQuest> filteredDependencyQuests;
        [Header("Dependency Quest Type Filter")]
        public List<vQuestType> dependencyQuestFilter = new List<vQuestType>() { 0 };

        [HideInInspector]
        public List<vQuest> DependentQuests
        {
            get
            {
                CleanBlankReferences();

                var dependencyQuestList = new List<vQuest>();

                dependencyQuestReferenceList.ForEach(q =>
                {
                    var dependency = dependencyQuestListData.quests.Find(q2 => q2.id == q.id);
                    if (!dependencyQuestList.Contains(dependency))
                        dependencyQuestList.Add(dependency);
                });

                return dependencyQuestList;
            }
        }

        #endregion

        #region Reward Settings Properties
        [Header("---Reward Settings---")]

        [Header("Reward Item List")]
        public vItemListData rewardItemListData;
        [HideInInspector]
        public List<vItem> filteredRewardItems;
        [Header("Reward Item Type Filter")]
        public List<vItemType> rewardFilter = new List<vItemType>() { 0 };
        #endregion

        [HideInInspector]
        public List<ItemReference> rewardItemReferenceList;

        [HideInInspector, SerializeField]
        public List<ItemReference> Rewards
        {
            get
            {
                CleanBlankReferences();
                return rewardItemReferenceList;
            }
        }

        [HideInInspector]
        public vItemListData gatherItemListData;
        [HideInInspector]
        public vItemListData killWithItemListData;

        public vQuest() : base()
        {
            //secondaryQuestList = new List<vQuest> ();
            isAccepted = false;
        }

        public void UpdateQuestStatus(vQuestState state, bool updateChildren = true)
        {

            this.state = state;

            if (state == vQuestState.Failed)
                this.isAccepted = false;

            if (secondaryQuestListData)
            {
                secondaryQuestReferenceList.ForEach(
                    q =>
                    {
                        var s = secondaryQuestListData.quests.Find(q2 => q2.id == q.id);
                        if (s == null)
                            return;
                        s.isAccepted = this.isAccepted;
                        if (updateChildren)
                            s.UpdateQuestStatus(state);
                    });
            }
        }

        public void CleanBlankReferences()
        {

            if ((secondaryQuestListData == null || (secondaryQuestListData != null && secondaryQuestListData.quests != null)))
            {
                if (secondaryQuestListData != null)
                {
                    secondaryQuestListData.quests.RemoveAll(q => q == null);
                    secondaryQuestReferenceList.RemoveAll(q => secondaryQuestListData.quests.Find(qu => qu.id == q.id) == null);
                }
                else
                {
                    if (secondaryQuestReferenceList != null)
                        secondaryQuestReferenceList.Clear();
                }
            }
            if (rewardItemListData == null || (rewardItemListData != null && rewardItemListData.items != null))
            {
                if (rewardItemListData != null)
                {
                    rewardItemListData.items.RemoveAll(i => i == null);
                    rewardItemReferenceList.RemoveAll(i => rewardItemListData.items.Find(it => it.id == i.id) == null);
                }
                else
                {
                    if (rewardItemReferenceList != null)
                        rewardItemReferenceList.Clear();
                }
            }
            if (dependencyQuestListData == null || (dependencyQuestListData != null && dependencyQuestListData.quests != null))
            {
                if (dependencyQuestListData != null)
                {
                    dependencyQuestListData.quests.RemoveAll(q => q == null);
                    dependencyQuestReferenceList.RemoveAll(q => dependencyQuestListData.quests.Find(qu => qu.id == q.id) == null);
                }
                else
                {
                    if (dependencyQuestReferenceList != null)
                        dependencyQuestReferenceList.Clear();
                }
            }
        }
        /// <summary>
        /// Convert Sprite icon to texture
        /// </summary>
        public Texture2D iconTexture
        {
            get
            {
                if (!icon) return null;
                try
                {
                    if (icon.rect.width != icon.texture.width || icon.rect.height != icon.texture.height)
                    {
                        Texture2D newText = new Texture2D((int)icon.textureRect.width, (int)icon.textureRect.height);
                        newText.name = icon.name;
                        Color[] newColors = icon.texture.GetPixels((int)icon.textureRect.x, (int)icon.textureRect.y, (int)icon.textureRect.width, (int)icon.textureRect.height);
                        newText.SetPixels(newColors);
                        newText.Apply();

                        return newText;
                    }
                    else
                        return icon.texture;
                }
                catch
                {
                    return icon.texture;
                }
            }
        }

        public vQuestAttribute GetQuestAttribute(string name)
        {
            if (attributes != null)
            {
                var attr = attributes.Find(attribute => attribute.name.Equals(name));
                return attr;
            }
            return null;
        }
    }
}

