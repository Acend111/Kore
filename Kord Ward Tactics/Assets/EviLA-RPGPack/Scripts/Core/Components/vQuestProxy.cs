using UnityEngine;
using System.Collections.Generic;
using System;

using Invector;
using Invector.vCharacterController;
using Invector.vItemManager;
using EviLA.AddOns.RPGPack.Spawners;

namespace EviLA.AddOns.RPGPack
{
    [Serializable]
    public partial class vQuestProxy
    {
        [SerializeField]
        private int id;
        [SerializeField]
        private string name;
        [SerializeField]
        private int parent = -1;
        [SerializeField]
        private vQuestType type;
        [SerializeField]
        private vQuestState state;
        [SerializeField]
        private bool isAccepted;
        [SerializeField]
        private string providerName;
        [SerializeField]
        private string targetName;
        [SerializeField]
        private string tagObjectTag;
        [SerializeField]
        private bool spawnOnlyIfQuestIsInProgress;
        [SerializeField]
        private bool destroySpawnedOnQuestFailure;
        [SerializeField]
        private int gatherItemId = -1;
        [SerializeField]
        private int killWithItemId = -1;
        [SerializeField]
        private string description;
        [SerializeField]
        private string objective;
        [SerializeField]
        private string completeText;
        [SerializeField]
        private string failureText;
        [SerializeField]
        private float currentDuration;
        [SerializeField]
        private bool isInitiallyActive;
        //[SerializeField]
        //private bool rewarded = false;
        [SerializeField]
        private int currentActionCount;
        [NonSerialized]
        private List<vQuestTracker> trackers;


        [SerializeField]
        private List<vQuestAttribute> attributes = new List<vQuestAttribute>();
        [SerializeField]
        private List<ItemReference> rewards = new List<ItemReference>();
        [SerializeField]
        private List<int> secondaryQuests = new List<int>();
        [SerializeField]
        private Dictionary<vQuestAttribute, int> currentAttributeStat = new Dictionary<vQuestAttribute, int>();

        [NonSerialized]
        private vQuestSystemSpawner spawner;


        public vQuestProxy(int id, vQuestType type, string providerName, vQuestState state = vQuestState.NotStarted, bool isAccepted = false, bool isInitiallyActive = false, int parent = -1)
        {

            this.id = id;
            this.type = type;
            this.state = state;
            this.isAccepted = isAccepted;
            this.providerName = providerName;
            this.parent = parent;
            this.isInitiallyActive = isInitiallyActive;
            this.currentActionCount = 0;

            var trackers = Resources.FindObjectsOfTypeAll<vQuestTracker>().vToList().FindAll(tracker => tracker.questID == id);
            this.trackers = trackers;

        }

        public int Id
        {
            get
            {
                return id;
            }
        }

        public vQuestState State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;

                if (state == vQuestState.Completed)
                {
                    AwardExperience();
                    Reward();
                    DropAllQuestItems();
                }

                if (state == vQuestState.Failed)
                {
                    if (spawner != null && spawner.destroySpawnedOnQuestFailure)
                        spawner.ResetSpawner();
                    DropAllQuestItems();
                }

                if (state == vQuestState.InProgress)
                {

                    if (!ScriptedCountDownEnabled)
                    {
                        if (Duration > 0f)
                        {
                            ResetDurationCountdown();
                            vQuestSystemManager.Instance.QuestManager.StartCountDown(id);
                        }
                    }
                }

                else if (spawner != null)
                {
                    if (state == vQuestState.InProgress)
                    {

                        if (spawnOnlyIfQuestIsInProgress)
                        {
                            spawner.enabled = true;
                            spawner.Respawn();
                        }
                        else
                            spawner.enabled = false;

                    }
                    else
                    {
                        spawner.enabled = true;
                    }
                }
            }
        }

        public void UpdateQuestTrackers()
        {
            if (trackers != null)
            {
                trackers.ForEach(tracker =>
                {
                    tracker.RestoreState();
                });
            }

            var instance = vQuestSystemManager.Instance;

            secondaryQuests.ForEach(quest =>
            {
                instance.GetProxyByID(quest).UpdateQuestTrackers();
            });
        }

        public void UpdateQuestStatus(vQuestState state, bool updateChildren = true)
        {

            this.State = state;

            var instance = vQuestSystemManager.Instance;

            if (state == vQuestState.Failed)
                this.isAccepted = false;

            secondaryQuests.ForEach(
                s =>
                {
                    instance.UpdateQuestAccepted(s, this.isAccepted);
                    if (updateChildren)
                        instance.UpdateQuestState(s, state);
                });

            if (CheckPointOnStateChange)
            {
                var saveBehavior = vThirdPersonController.instance.GetComponent<Persistence.vPersistenceManager>();
                saveBehavior.Save();
            }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public string Objective
        {
            get { return objective; }
            set { objective = value; }
        }

        public bool IsAccepted
        {
            get { return isAccepted; }
            set { isAccepted = value; }
        }

        public bool IsInitiallyActive
        {
            get { return isInitiallyActive; }
            set { isInitiallyActive = true; }
        }

        public string Provider
        {
            get { return providerName; }
        }

        public string Target
        {
            get { return targetName; }
            set
            {
                targetName = value;
            }
        }

        public List<vQuestAttribute> Attributes
        {
            get { return attributes; }
            set { attributes = value; }
        }

        public List<ItemReference> Rewards
        {
            get { return rewards; }
            set { rewards = value; }
        }

        public string Tag
        {
            get { return tagObjectTag; }
            set { tagObjectTag = value; }
        }

        public int GatherItemId
        {
            get { return gatherItemId; }
            set { gatherItemId = value; }
        }

        public int KillWithItemId
        {
            get { return killWithItemId; }
            set { killWithItemId = value; }
        }

        public float ElapsedDuration
        {
            get { return currentDuration; }
        }

        public bool ResetDurationPerAction
        {
            get
            {
                return (attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.ResetDurationPerAction) != null &&
                    attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.ResetDurationPerAction).value == 0) ? false : true;
            }
        }

        public int ActionCount
        {
            get
            {
                var actionCount = attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.ActionCount);
                try
                {
                    return actionCount.value;
                }
                catch (Exception)
                {
                    return 1;
                }
            }
        }

        public int CurrentActionCount
        {
            get
            {
                return currentActionCount;
            }
        }

        public float Duration
        {

            get
            {
                var duration = attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.Duration);
                try
                {
                    return duration.value;
                }
                catch (Exception)
                {
                    return 0f;
                }
            }
        }

        public float ExperienceGained
        {
            get
            {
                var experienceAttribute = attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.ExperienceGained);
                if (experienceAttribute == null)
                    return 0f;
                else
                    return experienceAttribute.value;
            }
        }

        public bool ForceStartOnAccept
        {
            get
            {
                return (attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.ForceStartOnAccept) != null &&
                    attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.ForceStartOnAccept).value == 0) ? false : true;
            }
        }

        public bool ScriptedCountDownEnabled
        {
            get
            {
                return (attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.ScriptedCountDownEnabled) != null &&
                    attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.ScriptedCountDownEnabled).value == 0) ? false : true;
            }
        }

        public bool IsAutoCompleteEnabled
        {
            get
            {
                return (attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.AutoComplete) != null &&
                    attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.AutoComplete).value == 0) ? false : true;
            }
        }

        public bool HasImpactOnParent
        {
            get
            {
                return attributes != null && attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.HasImpactOnParent) != null
                    && attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.HasImpactOnParent).value == 1;
            }
        }

        public bool HasImpactOnChildren
        {
            get
            {
                return attributes != null && attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.HasImpactOnChildren) != null
                    && attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.HasImpactOnChildren).value == 1;
            }
        }

        public bool Parallel
        {
            get
            {
                return attributes != null && attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.Parallel) != null
                    && attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.Parallel).value == 1;
            }
        }

        public bool ReloadAtQuestProviderLocationOnDecline
        {
            get
            {
                return attributes != null && attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.ReloadAtSpecifiedLocationOnDecline) != null
                    && attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.ReloadAtSpecifiedLocationOnDecline).value == 1;
            }
        }

        public bool QuestCanBeDeclined
        {
            get
            {
                return attributes != null && attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.QuestCanBeDeclined) != null
                    && attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.QuestCanBeDeclined).value == 1;
            }
        }

        public int QuestAmount
        {
            get
            {
                var amountAttribute = attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.QuestAmount);
                try
                {
                    return amountAttribute.value;
                }
                catch (Exception)
                {
                    var itemMan = vThirdPersonController.instance.GetComponent<vItemManager>();
                    var item = itemMan.items.Find(itm => itm.id == this.GatherItemId);
                    vQuestSystemManager.Instance.UpdateAttributeValue(id, EviLA.AddOns.RPGPack.vQuestAttributes.QuestAmount, item.amount);

                    amountAttribute = attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.QuestAmount);
                    return amountAttribute.value;
                    //throw new Exception("Trying to access invalid attribute or attribute not maintained");
                }
            }
        }

        public bool DropQuestItems
        {
            get
            {
                return attributes != null && attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.DropQuestItems) != null
                    && attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.Parallel).value == 1;
            }
        }

        public bool DropQuestItemsOnChildren
        {
            get
            {
                return attributes != null && attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.DropQuestItemsOnAllChildQuests) != null
                    && attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.Parallel).value == 1;
            }
        }

        public bool TargetCannotLeaveArea
        {
            get
            {
                return (attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.TargetCannotLeaveArea) != null &&
                    attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.TargetCannotLeaveArea).value == 1) ? true : false;
            }
        }

        public bool CheckPointOnStateChange
        {
            get
            {
                return (attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.CheckpointOnStateChange) != null &&
                    attributes.GetAttributeByType(EviLA.AddOns.RPGPack.vQuestAttributes.CheckpointOnStateChange).value == 1) ? true : false;
            }
        }

        private void DropAllQuestItems()
        {

            if (DropQuestItems || state == vQuestState.Failed)
            {
                InitDropQuestItems();
            }
        }

        private void DropQuestItemsOnQuest()
        {

            if (gatherItemId != -1)
            {

                var instance = vQuestSystemManager.Instance;

                var itm = instance.ItemManager.items.Find(item => item.id == this.gatherItemId);

                if (itm != null)
                    instance.ItemManager.DropItem(itm, QuestAmount);

            }
        }

        private void InitDropQuestItems()
        {

            DropQuestItemsOnQuest();

            if (DropQuestItemsOnChildren)
            {

                if (secondaryQuests.Count > 0)
                {

                    var instance = vQuestSystemManager.Instance;
                    secondaryQuests.ForEach(
                        sqid =>
                        {

                            var proxy = instance.GetProxyByID(sqid);
                            proxy.InitDropQuestItems();
                        }
                    );

                }

            }
        }

        private void Reward()
        {

            var instance = vQuestSystemManager.Instance;

            foreach (var reward in rewards)
            {

                ItemReference refItem = new ItemReference(reward.id);
                var itemScriptableObject = instance.ItemManager.itemListData.items.Find(item => item.id == reward.id);
                refItem.amount = reward.amount;
                refItem.attributes = reward.attributes;

                instance.ItemManager.AddItem(refItem);
                //use reward here because additem causes ref item to subtract the amount ;) 
                vItemCollectionDisplay.Instance.FadeText("Rewarded : " + reward.amount.ToString() + " " + itemScriptableObject.name, 1f, 1f);
            }
        }

        private void AwardExperience()
        {
            var instance = vQuestSystemManager.Instance;
            if (ExperienceGained > 0f)
            {
                instance.LevelManager.AddExperience((int)ExperienceGained);
            }
        }


        public vQuestType Type
        {
            get; set;
        }

        public int Parent
        {
            get; set;
        }

        public List<vQuestProxy> SecondaryQuests
        {
            get
            {
                //return secondaryQuests;
                var instance = vQuestSystemManager.Instance;
                List<vQuestProxy> secondary = new List<vQuestProxy>();
                secondaryQuests.ForEach(
                    q =>
                    {
                        secondary.Add(instance.GetProxyByID(q));
                    }
                );
                return secondary;
            }
        }

        public bool UpdateQuestAttribute(vQuestAttributes attr, int value)
        {
            try
            {
                vQuestAttribute attribute = vQuestAttributeHelper.GetAttributeByType(attributes, attr);
                if (currentAttributeStat.ContainsKey(attribute))
                {
                    currentAttributeStat[attribute] = value;
                    return true;
                }
                return false;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public int GetTaggedCount()
        {

            int value = 0;
            var attribute = vQuestAttributeHelper.GetAttributeByType(attributes, vQuestAttributes.QuestAmount);

            if (currentAttributeStat.TryGetValue(attribute, out value))
                return value;
            else
                return 0;
        }

        public void SetSpawner(vQuestSystemSpawner spawner, bool spawnOnlyIfQuestIsInProgress, bool destroySpawnedOnQuestFailure)
        {

            this.spawner = spawner;
            this.spawnOnlyIfQuestIsInProgress = spawnOnlyIfQuestIsInProgress;

        }

        public bool QuestAllowsSpawning()
        {
            if (spawner == null)
                return false;
            if (!vGlobalObjectPool.Instance.IsPoolEmpty(spawner))
                return true;
            else if (spawnOnlyIfQuestIsInProgress && state == vQuestState.InProgress)
                return true;
            else if (!spawnOnlyIfQuestIsInProgress)
                return true;
            else
                return false;
        }

        public void AddSecondaryQuestProxy(vQuestProxy proxy)
        {
            secondaryQuests.Add(proxy.id);
        }

        #region Duration related methods

        public bool UpdateElapsedDuration(float current)
        {
            currentDuration = current;

            if (currentDuration > Duration)
                return false;
            else
                return true;
        }

        public void ResetDurationCountdown()
        {
            currentDuration = 0f;
        }

        #endregion

        #region Generic Quest related methods

        public void IncrementActionCount()
        {
            ++currentActionCount;
            if (ResetDurationPerAction)
                ResetDurationCountdown();
        }

        public void DecrementActionCount()
        {
            --currentActionCount;
        }

        #endregion

    }

}