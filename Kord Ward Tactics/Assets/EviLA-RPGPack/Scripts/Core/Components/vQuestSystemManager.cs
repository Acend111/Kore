using UnityEngine;
using System.Collections.Generic;
using System;
using Invector.vCharacterController;
using Invector;
using Invector.vItemManager;
using EviLA.AddOns.RPGPack.Spawners;
using EviLA.AddOns.RPGPack.UI;
using EviLA.AddOns.RPGPack.Experience;

namespace EviLA.AddOns.RPGPack
{
    [Serializable]
    public class OnActiveQuestChanged : UnityEngine.Events.UnityEvent<int, bool, bool, bool> { }

    [Serializable]
    public partial class vQuestSystemManager
    {
        [SerializeField]
        private int activeQuestID = -1;

        [SerializeField]
        private List<int> inProgressQuests = new List<int>();

        [NonSerialized]
        private vQuestListData questList;

        [SerializeField]
        private List<vQuestProxy> _questProxies = new List<vQuestProxy>();

        [NonSerialized]
        private vItemManager itemManager;

        [NonSerialized]
        private vQuestManager questManager;

        [NonSerialized]
        private vLevelManager levelManager;

        [SerializeField]
        private static vQuestSystemManager instance;// = new vQuestSystem();

        public static vQuestSystemManager Instance
        {
            get
            {

                if (instance == null)
                {
                    instance = new vQuestSystemManager();
                }

                return instance;

            }
        }

        vQuestSystemManager()
        {
            UpdateProxies();
            HookUpQuestStateListeners();
            HookUpActiveQuestResetListeners();

        }

        [SerializeField]
        private Transform reloadCheckPoint;

        public Transform CurrentReloadCheckPoint
        {
            get { return reloadCheckPoint; }
            set { reloadCheckPoint = value; }
        }

        public OnActiveQuestChanged onActiveQuestChanged = new OnActiveQuestChanged();

        public vItemManager ItemManager
        {
            get
            {
                if (itemManager == null)
                {
                    try
                    {

                        if (itemManager == null)
                            itemManager = vThirdPersonController.instance.GetComponent<vItemManager>();
                    }

                    catch (Exception e)
                    {
                        return null;
                    }
                }
                return itemManager;
            }
        }

        public vQuestManager QuestManager
        {
            get
            {
                if (questManager == null)
                {
                    try
                    {
                        questManager = vThirdPersonController.instance.GetComponent<vQuestManager>();
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                }
                return questManager;
            }
        }

        public vLevelManager LevelManager
        {
            get
            {
                if (levelManager == null)
                {
                    try
                    {
                        levelManager = vThirdPersonController.instance.GetComponent<vLevelManager>();
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                }
                return levelManager;
            }
        }

        void HookUpQuestStateListeners()
        {
            onActiveQuestChanged.AddListener((int id, bool showStatus, bool accepted, bool activeQuestChanged) =>
            {
                int actualQuest = -1;

                int parent = instance.GetParent(id);

                if (parent == -1)
                    actualQuest = id;
                else
                    actualQuest = parent;

                if (activeQuestChanged != false)
                {

                    var timed = _questProxies.FindAll(q => q.Duration > 0f && q.State == vQuestState.InProgress);
                    timed.ForEach(q =>
                    {
                        if (q.Parent != -1)
                        {
                            if (actualQuest != q.Parent)
                                activeQuestChanged = false;
                        }
                        else
                        {
                            if (actualQuest != q.Id)
                                activeQuestChanged = false;
                        }
                    });
                }

                if (activeQuestChanged)
                {
                    if (actualQuest != -1 && actualQuest != activeQuestID)
                    {
                        vQuestStatusDisplay.Instance.Reset();
                    }

                    activeQuestID = actualQuest;
                }

                if (activeQuestID == -1)
                    return;

                var proxy = instance.GetProxyByID(activeQuestID);
                var actualProxy = (activeQuestID == actualQuest) ? proxy : instance.GetProxyByID(actualQuest);

                var secondary = instance.GetSecondaryQuests(activeQuestID);

                bool updated = true;

                if (secondary.Count > 0)
                {

                    secondary.ForEach(s =>
                    {
                        if (instance.HasImpactOnParent(s))
                            if (instance.GetQuestState(s) == vQuestState.Failed)
                                updated = false;
                    });

                }
                else
                {

                    if (instance.GetQuestState(activeQuestID) == vQuestState.Failed)
                        updated = false;
                }


                if (actualQuest == activeQuestID)
                {
                    if (proxy.State != vQuestState.Failed && (secondary != null && secondary.Count > 0))
                    {
                        secondary.ForEach(secondaryQuest =>
                        {
                            var secondaryQuestState = instance.GetQuestState(secondaryQuest);

                            if (secondaryQuestState == vQuestState.InProgress || secondaryQuestState == vQuestState.PendingReward)
                            {
                                var secondaryProxy = instance.GetProxyByID(secondaryQuest);

                                if (secondaryProxy.SecondaryQuests.Count > 0)
                                    onActiveQuestChanged.Invoke(secondaryQuest, true, false, activeQuestChanged);

                                if (secondaryQuestState == vQuestState.PendingReward)
                                {
                                    string message = " to " + GetTargetName(secondaryQuest) + ".";
                                    if (GetTargetName(activeQuestID).Equals(GetProviderName(secondaryQuest)))
                                        message = "Return" + message;
                                    else
                                        message = "Go" + message;

                                    vQuestStatusDisplay.Instance.ShowStayingText(secondaryQuest, QuestDisplayComponent.Objective, message);
                                    return;
                                }

                                if (secondaryQuestState == vQuestState.InProgress)
                                {
                                    inProgressQuests.Add(activeQuestID);

                                    if (secondaryProxy.Type == vQuestType.Gather && secondaryProxy.QuestAmount > 1)
                                    {
                                        var gatherItem = Instance.QuestManager.itemManager.items.Find(i => i.id == secondaryProxy.GatherItemId);
                                        int amount = 0;

                                        if (gatherItem != null)
                                        {
                                            amount = gatherItem.amount;
                                        }

                                        vQuestStatusDisplay.Instance.ShowStayingText(secondaryQuest, QuestDisplayComponent.Objective, secondaryProxy.Name + " : " + secondaryProxy.Objective + " " + amount + "/" + secondaryProxy.QuestAmount);

                                    }
                                    else if (secondaryProxy.Type == vQuestType.Generic && secondaryProxy.ActionCount > 1)
                                    {

                                        var actionCount = secondaryProxy.ActionCount;
                                        var currentCompletedCount = secondaryProxy.CurrentActionCount;
                                        vQuestStatusDisplay.Instance.ShowStayingText(secondaryQuest, QuestDisplayComponent.Objective, secondaryProxy.Name + " : " + secondaryProxy.Objective + " " + currentCompletedCount + "/" + actionCount);

                                    }
                                    else
                                    {
                                        vQuestStatusDisplay.Instance.ShowStayingText(secondaryQuest, QuestDisplayComponent.Objective, secondaryProxy.Name + " : " + secondaryProxy.Objective);
                                    }
                                }
                            }
                        });
                    }
                    else
                    {
                        if (proxy.State == vQuestState.InProgress)
                        {
                            inProgressQuests.Add(activeQuestID);

                            if (proxy.Type == vQuestType.Gather && proxy.QuestAmount > 1)
                            {
                                var gatherItem = Instance.QuestManager.itemManager.items.Find(i => i.id == proxy.GatherItemId);
                                int amount = 0;

                                if (gatherItem != null)
                                {
                                    amount = gatherItem.amount;
                                }

                                vQuestStatusDisplay.Instance.ShowStayingText(activeQuestID, QuestDisplayComponent.Objective, proxy.Name + " : " + proxy.Objective + " " + amount + "/" + proxy.QuestAmount);

                            }
                            else if (proxy.Type == vQuestType.Generic && proxy.ActionCount > 1)
                            {

                                var actionCount = proxy.ActionCount;
                                var currentCompletedCount = proxy.CurrentActionCount;
                                vQuestStatusDisplay.Instance.ShowStayingText(activeQuestID, QuestDisplayComponent.Objective, proxy.Name + " : " + proxy.Objective + " " + currentCompletedCount + "/" + actionCount);

                            }
                            else
                            {
                                vQuestStatusDisplay.Instance.ShowStayingText(activeQuestID, QuestDisplayComponent.Objective, proxy.Name + " : " + proxy.Objective);
                            }
                        }
                    }
                }

                if (accepted)
                {
                    vQuestStatusDisplay.Instance.ShowFadingText(activeQuestID, proxy.Name + " : Accepted ", 3f, 1f);
                    return;
                }

                var appendText = updated ? "Updated" : "Failed";

                if (activeQuestID == actualQuest)
                {
                    switch (Instance.GetQuestState(activeQuestID))
                    {

                        case vQuestState.PendingReward:
                            string message = " to " + GetTargetName(activeQuestID) + ".";
                            if (GetTargetName(activeQuestID).Equals(GetProviderName(activeQuestID)))
                                message = "Return" + message;
                            else
                                message = "Go" + message;

                            vQuestStatusDisplay.Instance.ShowStayingText(activeQuestID, QuestDisplayComponent.Objective, message);
                            vQuestStatusDisplay.Instance.ShowFadingText(activeQuestID, proxy.Name + " " + appendText, 3f, 1f);
                            break;

                        case vQuestState.Completed:
                            vQuestStatusDisplay.Instance.ShowFadingText(activeQuestID, proxy.Name + " : Complete ", 3f, 1f);
                            break;

                        default:
                            vQuestStatusDisplay.Instance.ShowFadingText(activeQuestID, proxy.Name + " : " + appendText, 3f, 1f);
                            break;
                    }
                }

                else if (showStatus)
                {
                    vQuestStatusDisplay.Instance.ShowFadingText(actualQuest, actualProxy.Name + " : " + appendText, 3f, 1f);
                }

                //if (activeQuestChanged && ( instance.GetQuestState(activeQuestID) != vQuestState.NotStarted ) )
                //    vQuestStatusDisplay.Instance.ShowFadingText(activeQuestID, proxy.Name + " : Updated ", 3f, 1f);

            });
        }

        void HookUpActiveQuestResetListeners()
        {
            onActiveQuestChanged.AddListener((int id, bool showStatus, bool accepted, bool activeQuestChanged) =>
            {
                if (Instance.GetQuestState(id) == vQuestState.Failed || Instance.GetQuestState(id) == vQuestState.NotStarted)
                {
                    UpdateQuestTrackers(id);

                    if (activeQuestID == id)
                    {
                        if (Instance.GetProxyByID(id).ReloadAtQuestProviderLocationOnDecline)
                        {
                            var quest = questManager.questListData.quests.Find(q => q.id == activeQuestID);

                            if (quest.reloadLocationOnDecline == null)
                                throw new Exception("Unable to find reload location");

                            vThirdPersonController.instance.lockMovement = true;
                            vThirdPersonController.instance.gameObject.transform.position = quest.reloadLocationOnDecline.position;
                            vThirdPersonController.instance.lockMovement = false;

                        }

                        var next = questManager.goToNextQuestInListOnFailure ? _questProxies.Find(q => q.State == vQuestState.InProgress && q.Parent == -1)
                                         : null;

                        if (next != null)
                        {
                            onActiveQuestChanged.Invoke(next.Id, true, false, true);
                            activeQuestID = next.Id;
                        }
                        else
                        {
                            activeQuestID = -1;
                        }

                    }
                }
            });
        }



        public bool ExternalSaveEnabled = true;

        public void SetInitiallyActiveQuest()
        {
            var initial = _questProxies.Find(q => q.IsInitiallyActive);
            if (initial != null && initial.State == vQuestState.NotStarted)
            {
                initial.IsInitiallyActive = false;
                QuestManager.AcceptQuest(questList.quests.Find(q => q.id == initial.Id));
                onActiveQuestChanged.Invoke(initial.Id, true, true, true);
            }
        }

        public void AssignQuestSpawner(int id, vQuestSystemSpawner spawner, bool spawnOnlyIfQuestIsInProgress, bool destroySpawnedOnQuestFailure)
        {
            _questProxies.Find(q => q.Id == id).SetSpawner(spawner, spawnOnlyIfQuestIsInProgress, destroySpawnedOnQuestFailure);
        }

        public int ActiveQuest
        {
            get
            {
                return activeQuestID;
            }
        }

        public void UpdateQuestState(int id, vQuestState state)
        {

            var quest = _questProxies.Find(q => id == q.Id);
            int index = _questProxies.FindIndex(q => q.Id == id);
            _questProxies[index].UpdateQuestStatus(state, false);

        }

        private void UpdateQuestStateRecursively(int id, vQuestState state)
        {
            var quest = _questProxies.Find(q => id == q.Id);
            int index = _questProxies.FindIndex(q => q.Id == id);
            _questProxies[index].UpdateQuestStatus(state, true);
        }

        public vQuestProxy GetProxyByID(int id)
        {
            return _questProxies.Find(qp => qp.Id == id);
        }


        public void UpdateQuest(int id, vQuestProvider fromProvider = null, IQuestTarget fromTarget = null)
        {
            var quest = _questProxies.Find(q => q.Id == id);
            if (quest != null)
            {
                var isAccepted = instance.IsQuestAccepted(quest.Id);
                var state = instance.GetQuestState(quest.Id);
                var providerName = instance.GetProviderName(quest.Id);
                var targetName = instance.GetTargetName(quest.Id);
                var isAutoComplete = instance.IsAutoCompleteEnabled(quest.Id);
                var isParallel = instance.IsParallelQuest(quest.Id);
                var hasImpactOnChildren = instance.HasImpactOnChildren(quest.Id);
                var hasImapctOnParent = instance.HasImpactOnParent(quest.Id);
                var parent = instance.GetParent(quest.Id);
                var type = instance.GetQuestType(quest.Id);
                var gatherItemId = instance.GetGatherItemId(quest.Id);
                var killWithItemId = instance.GetKillItemId(quest.Id);
                var actionCount = instance.GetActionCount(quest.Id);
                var currentActionCount = instance.GetCompletedActionCount(quest.Id);

                #region Process Multiple Quest Type
                if (type == vQuestType.Multiple)
                {

                    //Get the list of tasks / secondary quests
                    List<int> secondary = instance.GetSecondaryQuests(quest.Id);

                    //Update Pending Reward Quests to Completed
                    secondary.FindAll(q => instance.GetQuestState(q) == vQuestState.PendingReward)
                                .ForEach(q =>
                                {
                                    var q_state = instance.GetQuestState(q);
                                    var q_isAccepted = instance.IsQuestAccepted(q);
                                    var q_targetName = instance.GetTargetName(q);
                                    var q_providerName = instance.GetProviderName(q);

                                    if (q_isAccepted)
                                    {

                                        if (fromTarget != null && q_targetName == fromTarget.getTargetName() ||
                                            (fromTarget == null && fromProvider != null && fromProvider.getTargetName().Equals(q_providerName)))
                                        {
                                            instance.UpdateQuestStateRecursively(q, vQuestState.Completed);
                                        }

                                    }

                                    if (state != vQuestState.Failed)
                                    {
                                        //ShowQuestStatus(q);
                                    }
                                });

                    /*
                    If at least one secondary quest having "has impact onparent" attribute has failed, set the parent quest as failed
                    and exit
                    */


                    bool proceed = true;

                    if (secondary.FindAll(q => instance.GetQuestState(q) == vQuestState.Failed && instance.HasImpactOnParent(q)).Count > 0)
                    {
                        if (instance.HasImpactOnParent(quest.Id))
                        {
                            instance.UpdateQuestStateRecursively(quest.Id, vQuestState.Failed);
                        }
                        else
                        {
                            instance.UpdateQuestState(quest.Id, vQuestState.Failed);
                        }

                        //ShowQuestStatus(quest.Id);

                        proceed = false;
                    }

                    /*Else check if all of the secondary quests have been completed
                       Quests can be completed either after accepting the quest, in which the state of the quest will be vQuestState.Completed, 
                       or they can be completed before it was given by a provider/giver when the player is exploring the world.
                       In this case the state of the quest is vQuestState.NotAcceptedButComplete
                    */

                    else if (proceed && secondary.FindAll(q => instance.GetQuestState(q) == vQuestState.Completed ||
                                                                instance.GetQuestState(q) == vQuestState.NotAcceptedButComplete).Count == secondary.Count)
                    {
                        // if the secondary quests are complete
                        secondary.FindAll(q => (instance.GetQuestState(q) == vQuestState.Completed ||
                                                  instance.GetQuestState(q) == vQuestState.NotAcceptedButComplete)).ForEach(
                                q =>
                                {

                                    if (!instance.HasImpactOnParent(q))
                                    {
                                        proceed = true;
                                    }
                                });
                        //but the parent quest is not accepted, update the main quest and sub quests to NotAcceptedButComplete quest status
                        //else, update the parent quest and subquests to Complete quest status.
                        if (proceed)
                        {
                            if (!isAccepted)
                            {
                                instance.UpdateQuestStateRecursively(quest.Id, vQuestState.NotAcceptedButComplete);
                            }
                            else
                            {
                                if (!isAutoComplete)
                                {
                                    if (fromProvider != null && fromProvider.name.Equals(providerName))
                                    {
                                        instance.UpdateQuestStateRecursively(quest.Id, vQuestState.Completed);
                                    }
                                    else if (fromTarget == null && targetName == "" && fromProvider != null && fromProvider.name.Equals(providerName)) //&& !quest.HasQuestTargetEntity
                                    {
                                        instance.UpdateQuestStateRecursively(quest.Id, vQuestState.Completed);
                                    }
                                    else if (targetName != "")
                                    {
                                        if (targetName.Equals(fromTarget.getTargetName()))
                                        {
                                            instance.UpdateQuestStateRecursively(quest.Id, vQuestState.Completed);
                                        }
                                        else
                                        {
                                            instance.UpdateQuestState(quest.Id, vQuestState.PendingReward);
                                        }
                                    }

                                    else
                                    {
                                        instance.UpdateQuestState(quest.Id, vQuestState.PendingReward);
                                    }

                                    if (state != vQuestState.Failed)
                                    {
                                        //ShowQuestStatus(quest.Id);
                                    }
                                }
                                else
                                {
                                    instance.UpdateQuestStateRecursively(quest.Id, vQuestState.Completed);

                                }

                                //if (vQuestSystem.Instance.GetQuestState(quest.Id) == vQuestState.Completed ||
                                //    vQuestSystem.Instance.GetQuestState(quest.Id) == vQuestState.PendingReward)
                                //onActiveQuestChanged.Invoke(quest.Id, true,false);
                            }
                        }
                    }

                    // 11/01/2017 - Tharindu - Moved from top of the stack to this in order to impact cases first
                    state = instance.GetQuestState(quest.Id);

                    if (state == vQuestState.Failed)
                    {
                        if (hasImpactOnChildren || (parent != -1 && instance.HasImpactOnChildren(parent)))
                        {
                            secondary.ForEach(q =>
                                {
                                    instance.UpdateQuestState(q, state);
                                    UpdateQuest(q);
                                    instance.UpdateQuestState(q, vQuestState.NotStarted);
                                });

                        }

                        instance.UpdateQuestState(id, vQuestState.NotStarted);
                        state = instance.GetQuestState(quest.Id);

                        return;
                    }

                    // If the quest is not accepted, exit method
                    if (!isAccepted || state == vQuestState.Failed)
                        return;
                    /* If accepted, check if there are secondary quests that have been Completed although they haven't been accepted from a Quest Provider/Giver,
                       and set their status to Complete. 
                       Display the status of these quests
                    */
                    else
                    {
                        secondary.FindAll(q => instance.GetQuestState(q) == vQuestState.NotAcceptedButComplete)
                            .ForEach(q =>
                            {
                                instance.UpdateQuestStateRecursively(q, vQuestState.Completed);
                                if (state != vQuestState.Failed)
                                {
                                    //ShowQuestStatus(q);
                                }
                            });
                    }

                    /* Look for secondary quests that need to run in parallel and update their quest status */
                    var parallelList = secondary.FindAll(q => instance.GetQuestState(q) == vQuestState.NotStarted
                                                                                           && instance.IsParallelQuest(q));

                    foreach (var parallelQuest in parallelList)
                    {

                        if (isAccepted)
                        {
                            instance.UpdateQuestAccepted(parallelQuest, isAccepted);
                            instance.UpdateQuestState(parallelQuest, vQuestState.InProgress);

                        }

                        UpdateQuest(parallelQuest, fromProvider);
                    }

                    /* Since the parallel quests have been sorted out, Choose the sequential ones, and process them*/
                    // Get the next quest in the secondary quest/task list that has not been started yet (and also sequential / not parallel)
                    if (secondary.FindAll(q => instance.GetQuestState(q) == vQuestState.InProgress && !instance.IsParallelQuest(q)).Count == 0)
                    {
                        var secondaryArray = secondary.FindAll(q => instance.GetQuestState(q) == vQuestState.NotStarted && !parallelList.Contains(q)
                            ).vToArray<int>();

                        if (secondaryArray.Length == 0)
                        {
                            if (parent != -1)
                                UpdateQuest(parent, fromProvider);
                            else
                                //ShowQuestStatus(id);
                                return;
                        }

                        var notStarted = secondaryArray[0];
                        var index = secondary.IndexOf(notStarted);

                        if (isAccepted)
                        {
                            instance.UpdateQuestAccepted(notStarted, isAccepted);
                            if (instance.GetQuestType(notStarted) == vQuestType.Multiple)
                            {
                                instance.UpdateQuestState(notStarted, vQuestState.InProgress);
                            }

                            UpdateQuest(notStarted, fromProvider);
                        }
                    }

                }
                #endregion 

                #region Discover Quest Type
                else if (type == vQuestType.Discover)
                {
                    if (fromTarget != null)
                    {
                        if (state == vQuestState.InProgress)
                        {
                            instance.UpdateQuestState(quest.Id, vQuestState.Completed);
                            //onActiveQuestChanged.Invoke(quest.Id, true, false);
                        }
                    }
                }
                #endregion

                #region Escort Quest Type
                else if (type == vQuestType.Escort)
                {
                    if (fromTarget != null)
                    {
                        if (state == vQuestState.InProgress)
                        {
                            int questAmount = instance.GetQuestAmount(quest.Id);
                            int tagCount = instance.GetTaggedCount(quest.Id);
                            if (instance.GetTaggedCount(quest.Id) == tagCount)
                            {
                                instance.UpdateQuestState(quest.Id, vQuestState.Completed);
                                //onActiveQuestChanged.Invoke(quest.Id, true, false);
                            }
                            else
                            {
                                vHUDController.instance.ShowText(tagCount + "/" + questAmount + " " + quest.Tag);
                            }
                        }
                    }
                }

                #endregion

                #region Gather Quest Type
                else if (type == vQuestType.Gather && isAccepted)
                {
                    var item = instance.QuestManager.itemManager.items.Find(i => i.id == gatherItemId);
                    if (item != null)
                    {
                        if (instance.GetQuestAmount(quest.Id) == item.amount)
                        {
                            instance.UpdateQuestState(quest.Id, vQuestState.Completed);
                            // onActiveQuestChanged.Invoke(quest.Id, true, false);
                        }
                        else
                        {
                            instance.UpdateQuestState(quest.Id, vQuestState.InProgress);

                            if (instance.IsQuestAccepted(quest.Id))
                            {
                                vHUDController.instance.ShowText(item.amount + "/"
                                                                 + instance.GetQuestAmount(quest.Id) + " "
                                                                 + item.name);
                            }
                        }

                    }
                }
                #endregion

                #region Assassinate Quest Type
                else if (type == vQuestType.Assassinate)
                {
                    if (killWithItemId != -1)
                    {
                        var killItem = instance.ItemManager.items.Find(item => itemManager.ItemIsInSomeEquipArea(killWithItemId));
                        if (killItem == null)
                            instance.UpdateQuestState(quest.Id, vQuestState.Failed);
                        else
                        {
                            if (killItem.isInEquipArea)
                            {
                                if (instance.IsQuestAccepted(quest.Id))
                                {
                                    if (!isAutoComplete)
                                        instance.UpdateQuestState(quest.Id, vQuestState.PendingReward);
                                    else
                                        instance.UpdateQuestState(quest.Id, vQuestState.Completed);
                                }
                                else
                                    instance.UpdateQuestState(quest.Id, vQuestState.NotAcceptedButComplete);
                            }
                            else
                            {
                                instance.UpdateQuestState(quest.Id, vQuestState.Failed);
                            }
                        }
                    }

                    if (instance.GetQuestState(quest.Id) == vQuestState.NotAcceptedButComplete && instance.IsQuestAccepted(quest.Id))
                    {

                        if (!isAutoComplete)
                            instance.UpdateQuestState(quest.Id, vQuestState.PendingReward);
                        else
                            instance.UpdateQuestState(quest.Id, vQuestState.Completed);

                        //onActiveQuestChanged.Invoke(quest.Id, true, false);
                    }

                    if (fromTarget == null)
                    {
                        if (instance.GetQuestState(quest.Id) == vQuestState.Completed ||
                            instance.GetQuestState(quest.Id) == vQuestState.PendingReward)
                        {
                            //onActiveQuestChanged.Invoke(quest.Id, true, false);
                        }
                        else if (state != vQuestState.Failed)
                            instance.UpdateQuestState(quest.Id, vQuestState.InProgress);
                    }
                    else if (fromTarget.getTargetName().Equals(instance.GetTargetName(id)))
                    {
                        if (instance.GetQuestState(quest.Id) == vQuestState.PendingReward)
                            instance.UpdateQuestState(quest.Id, vQuestState.Completed);
                    }
                    else
                    {
                        if (instance.GetQuestState(quest.Id) == vQuestState.InProgress)
                        {
                            if (!isAutoComplete)
                                instance.UpdateQuestState(quest.Id, vQuestState.PendingReward);
                            else
                                instance.UpdateQuestState(quest.Id, vQuestState.Completed);
                        }
                        else if (instance.GetQuestState(quest.Id) == vQuestState.Failed)
                        {
                            instance.UpdateQuestState(quest.Id, vQuestState.Failed);
                        }
                        else
                        {
                            instance.UpdateQuestState(quest.Id, vQuestState.NotAcceptedButComplete);
                        }
                    }
                }
                else if (type == vQuestType.MultipleAssassinate)
                {

                }
                else if (type == vQuestType.Generic)
                {
                    if (state == vQuestState.PendingReward)
                    {
                        instance.UpdateQuestState(id, vQuestState.Completed);
                    }
                    else if (state == vQuestState.InProgress)
                    {

                        bool actionsComplete = true;

                        if (actionCount > 0)
                        {
                            actionsComplete = false;
                            if (IncrementActionCount(id) == actionCount)
                                actionsComplete = true;
                        }

                        if (actionsComplete)
                        {
                            if (isAutoComplete)
                                instance.UpdateQuestState(id, vQuestState.Completed);
                            else
                                instance.UpdateQuestState(id, vQuestState.PendingReward);
                        }

                    }
                    else if (state == vQuestState.NotStarted || !isAccepted)
                    {
                        instance.UpdateQuestState(id, vQuestState.NotAcceptedButComplete);
                    }

                }

                #endregion

                if ((instance.GetQuestState(quest.Id) != vQuestState.InProgress && instance.GetQuestState(quest.Id) != vQuestState.NotStarted))
                {
                    if (parent != -1)
                    {
                        if (instance.GetQuestState(parent) != vQuestState.Failed)
                        {
                            UpdateQuest(parent, fromProvider, fromTarget);
                            return;
                        }
                    }
                }
            }
        }

        public void ForceStartQuests()
        {
            var instance = vQuestSystemManager.Instance;
            if (instance.ActiveQuest != -1)
            {

                instance.onActiveQuestChanged.Invoke(instance.ActiveQuest, true, false, true);

                var timed = instance.Proxies.FindAll(q => instance.GetDuration(q.Id) > 0f && q.State == vQuestState.InProgress
                   && q.ElapsedDuration > 0f);
                timed.ForEach(t =>
                {
                    instance.QuestManager.StartCountDown(t.Id);
                });
            }
        }

        public void UpdateQuestFromActionTrigger(vQuestTriggerType triggerType, int id)
        {
            if (triggerType == vQuestTriggerType.Complete)
            {
                UpdateQuest(id);
            }
        }

        public void UpdateQuestAccepted(int id, bool accepted)
        {
            var quest = _questProxies.Find(q => id == q.Id);
            int index = _questProxies.FindIndex(q => q.Id == id);
            _questProxies[index].IsAccepted = accepted;
        }

        public bool UpdateAttributeValue(int id, vQuestAttributes attribute, int value)
        {
            return _questProxies.Find(q => q.Id == id).UpdateQuestAttribute(attribute, value);
        }

        public vQuestType GetQuestType(int id)
        {
            return _questProxies.Find(q => q.Id == id).Type;
        }

        public int GetParent(int id)
        {
            return _questProxies.Find(q => q.Id == id).Parent;
        }

        public List<int> GetSecondaryQuests(int id, bool sort = false)
        {
            List<int> sIds = new List<int>();
            if (id == -1)
                return sIds;
            var secondaryList = _questProxies.Find(q => q.Id == id).SecondaryQuests;

            if (sort)
                SortProxies(ref secondaryList);

            secondaryList.ForEach(
                s =>
                {
                    sIds.Add(s.Id);
                });
            return sIds;
        }

        public string GetTag(int id)
        {
            return _questProxies.Find(q => q.Id == id).Tag;
        }

        public int GetGatherItemId(int id)
        {
            return _questProxies.Find(q => q.Id == id).GatherItemId;
        }

        public int GetKillItemId(int id)
        {
            return _questProxies.Find(q => q.Id == id).KillWithItemId;
        }

        public int GetTaggedCount(int id)
        {
            return _questProxies.Find(q => q.Id == id).GetTaggedCount();
        }

        public vQuestState GetQuestState(int id)
        {
            return _questProxies.Find(q => q.Id == id).State;
        }

        public string GetProviderName(int id)
        {
            return _questProxies.Find(q => q.Id == id).Provider;
        }

        public string GetTargetName(int id)
        {
            return _questProxies.Find(q => q.Id == id).Target;
        }

        public bool IsQuestAccepted(int id)
        {
            return _questProxies.Find(q => q.Id == id).IsAccepted;
        }

        public bool IsAutoCompleteEnabled(int id)
        {
            return _questProxies.Find(q => q.Id == id).IsAutoCompleteEnabled;
        }

        public bool HasImpactOnParent(int id)
        {
            return _questProxies.Find(q => q.Id == id).HasImpactOnParent;
        }

        public bool HasImpactOnChildren(int id)
        {
            return _questProxies.Find(q => q.Id == id).HasImpactOnChildren;
        }

        public bool IsParallelQuest(int id)
        {
            return _questProxies.Find(q => q.Id == id).Parallel;
        }

        public int GetQuestAmount(int id)
        {
            return _questProxies.Find(q => q.Id == id).QuestAmount;
        }

        public bool TargetsCannotLeaveArea(int id)
        {
            return _questProxies.Find(q => q.Id == id).TargetCannotLeaveArea;
        }

        public bool QuestAllowsSpawning(int id)
        {
            return _questProxies.Find(q => q.Id == id).QuestAllowsSpawning();
        }

        public bool QuestCanBeDeclined(int id)
        {
            return _questProxies.Find(q => q.Id == id).QuestCanBeDeclined;
        }

        public bool UpdateElapsedDuration(int id, float timeElapsed)
        {
            return _questProxies.Find(q => q.Id == id).UpdateElapsedDuration(timeElapsed);
        }

        public float GetElapsedDuration(int id)
        {
            return _questProxies.Find(q => q.Id == id).ElapsedDuration;
        }

        public bool ResetDurationPerAction(int id)
        {
            return _questProxies.Find(q => q.Id == id).ResetDurationPerAction;
        }

        public int GetActionCount(int id)
        {
            return _questProxies.Find(q => q.Id == id).ActionCount;
        }

        public int GetCompletedActionCount(int id)
        {
            return _questProxies.Find(q => q.Id == id).CurrentActionCount;
        }

        public int IncrementActionCount(int id)
        {
            _questProxies.Find(q => q.Id == id).IncrementActionCount();
            return GetCompletedActionCount(id);
        }

        public void DecrementActionCount(int id)
        {
            _questProxies.Find(q => q.Id == id).DecrementActionCount();
        }

        public bool IsTimedQuestInProgress()
        {
            var timed = _questProxies.FindAll(q => q.State == vQuestState.InProgress && q.Duration != 0f && q.ElapsedDuration > 0f);
            return timed != null && timed.Count > 0;
        }

        public bool AreTimedQuestsInQuestLog()
        {
            var timed = _questProxies.FindAll(q => q.State == vQuestState.InProgress && q.Duration != 0f);
            return timed != null && timed.Count > 0;
        }

        public float GetDuration(int id)
        {
            return _questProxies.Find(q => q.Id == id).Duration;
        }

        public bool IsScriptedCountDownEnabled(int id)
        {
            return _questProxies.Find(q => q.Id == id).ScriptedCountDownEnabled;
        }

        public bool IsForceStartOnAccept(int id)
        {
            return _questProxies.Find(q => q.Id == id).ForceStartOnAccept;
        }

        internal void UpdateQuestTrackers(int id)
        {
            _questProxies.Find(q => q.Id == id).UpdateQuestTrackers();
        }


        internal void ResetProxies(List<vQuestProxy> proxies)
        {
            _questProxies = proxies;
            SortProxies();
        }

        public List<vQuestProxy> Proxies
        {
            get
            {
                return _questProxies.vToArray<vQuestProxy>().vToList<vQuestProxy>();
            }
        }

        private void UpdateProxies()
        {

            if (_questProxies.Count == 0)
            {
                questList = QuestManager.questListData;
            }

            questList.quests.ForEach(quest =>
            {
                CreateProxy(quest);
            });

            SortProxies();


        }

        private void SortProxies()
        {
            _questProxies.Sort((vQuestProxy a, vQuestProxy b) =>
            {
                if (a.Id < b.Id)
                    return -1;
                else if (a.Id > b.Id)
                    return 1;
                else
                    return 0;
            });
        }

        private void SortProxies(ref List<vQuestProxy> proxies)
        {
            proxies.Sort((vQuestProxy a, vQuestProxy b) =>
            {
                if (a.Id < b.Id)
                    return -1;
                else if (a.Id > b.Id)
                    return 1;
                else
                    return 0;
            });
        }

        private vQuestProxy CreateProxy(vQuest quest)
        {

            var _proxy = _questProxies.Find(proxy => proxy.Id == quest.id);
            if (_proxy != null)
                return _proxy;

            var questProxy = new vQuestProxy(quest.id, quest.type, quest.provider != null ? quest.provider.name : "", quest.state, quest.isAccepted, quest.isInitiallyActive);
            questProxy.Name = quest.name;
            questProxy.Description = quest.description;
            questProxy.Objective = quest.objective;
            questProxy.Target = quest.QuestTarget != null ? quest.QuestTarget.getTargetName() : "";
            questProxy.Attributes = (List<vQuestAttribute>)quest.attributes.CopyAsNew();
            questProxy.Tag = quest.tagObjectTag;
            questProxy.Type = quest.type;

            //In some cases, when they secondary quest list data is directly removed by dereferencing the list, the parent reference of the child object is not removed. 
            //Currently a temporary fix has been placed here to check if the parent still contains this secondary quest.
            //Ideally this needs to be done at the design time of the scriptable object

            //questProxy.Parent = quest.parent != null && quest.parent.SecondaryQuests.Find( q => q.id == quest.id ) != null ? quest.parent.id : -1;

            //Should a situation arise where the parent is still present in the child quest even though the secondary quest list is dereferenced, use the above live of code
            // and comment below line

            questProxy.Parent = quest.parent != null ? quest.parent.id : -1;

            questProxy.GatherItemId = quest.gatherItem != null ? quest.gatherItem.id : -1;
            questProxy.KillWithItemId = quest.killWithThis != null ? quest.killWithThis.id : -1;
            questProxy.Rewards = quest.Rewards.vCopy();

            if (quest.SecondaryQuests.Count > 0)
            {
                quest.SecondaryQuests.ForEach(q =>
                {
                    questProxy.AddSecondaryQuestProxy(CreateProxy(questList.quests.Find(q2 => q2.id == q.id)));
                });
                _questProxies.Add(questProxy);
            }
            else
            {
                _questProxies.Add(questProxy);
            }

            return questProxy;

        }
    }
}

