using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;

using Invector;
using Invector.vItemManager;
using Invector.vCharacterController;
using EviLA.AddOns.RPGPack.Experience;
using EviLA.AddOns.RPGPack.Experience.UI;

namespace EviLA.AddOns.RPGPack.Persistence.Serialization
{

    public class PlayerSerializationStrategyException : Exception
    {
        public PlayerSerializationStrategyException(string message) : base(message) { }
    }

    public class PlayerSerializationStrategy : ISerializationStrategy
    {

        public void Serialize<T>(T data, IFormatter formatter, CryptoStream stream) where T : SerializedContent
        {
            try
            {
                formatter.Serialize(stream, data);
            }
            catch (SerializationException e)
            {
                throw new PlayerSerializationStrategyException("Unable to Serialize Player");
            }
        }

        public void Serialize<T>(List<T> data, IFormatter formatter, CryptoStream stream) where T : SerializedContent
        {
            throw new PlayerSerializationStrategyException("Player object cannot be in a list. There can be only one instance of a player");
        }

        public void DeserializeSingle<T>(ref T data, IFormatter formatter, CryptoStream stream) where T : MonoBehaviour
        {
            PlayerSerializedContent playerContent = ((SerializedContent)formatter.Deserialize(stream)) as PlayerSerializedContent;


            var tpc = data.gameObject.GetComponent<vThirdPersonController>();
            var itemManager = data.gameObject.GetComponent<vItemManager>();

            if (itemManager == null)
            {
                itemManager = data.gameObject.AddComponent<vItemManager>();
            }


            if (itemManager != null)
            {

                itemManager.items.Clear();


                playerContent.items.ForEach(item =>
                {
                    var itemScriptable = itemManager.itemListData.items.Find(itm => itm.id == item.id);
                    itemManager.AddItem(item);
                    itemManager.AutoEquipItem(itemScriptable, item.indexArea, true);
                });

                var unEquipTheseAreas = itemManager.inventory.equipAreas.vToList();

                foreach (var equippedItem in playerContent.equippedItems)
                {
                    itemManager.inventory.equipAreas[equippedItem.areaIndex].SetEquipSlot(equippedItem.currentEquipSlotId);
                    unEquipTheseAreas.RemoveAll(area => area.name.Equals(itemManager.inventory.equipAreas[equippedItem.areaIndex].name));
                }

                foreach (var area in unEquipTheseAreas)
                {
                    itemManager.inventory.UnequipItem(area, area.currentEquipedItem);
                }

            }

            var persistenceManager = data.gameObject.GetComponent<vPersistenceManager>();
            persistenceManager.lastCheckPointInScene = playerContent.lastCheckPointPerScene;

            if (playerContent.deletedGOs != null)
            {

                var listOfGOs = GameObject.FindObjectsOfType<vCanSaveYou>().vToList();
                playerContent.deletedGOs.RemoveAll(deletedGO => !deletedGO.Contains(SceneManager.GetActiveScene().name + "_"));
                playerContent.deletedGOs.ForEach(deletedGO =>
                {
                    string go_guid = deletedGO.Split('_')[1];
                    var go = listOfGOs.Find(g => g.guid.Equals(go_guid));
                    if (go != null)
                    {
                        go.gameObject.SetActive(false);
                    }
                });

            }

            tpc.healthRecovery = playerContent.healthRecovery;
            tpc.healthRecoveryDelay = playerContent.healthRecoveryDelay;
            tpc.currentHealthRecoveryDelay = playerContent.currentHealthRecoveryDelay;

            var healthFactor = (int)(playerContent.currentHealth - playerContent.currentHealth);
            tpc.ChangeHealth(healthFactor);

            var staminaFactor = (int)(playerContent.currentStamina - playerContent.currentStamina);
            tpc.ChangeStamina(staminaFactor);

            tpc.staminaRecovery = playerContent.staminaRecovery;
            tpc.currentStaminaRecoveryDelay = playerContent.currentStaminaRecoveryDelay;


            var maxHealthFactor = (int)(playerContent.currentMaxHealth - tpc.MaxHealth);
            tpc.ChangeMaxHealth(maxHealthFactor);

            var maxStaminaFactor = (int)(playerContent.currentMaxStamina - tpc.maxStamina);
            tpc.ChangeMaxStamina(maxStaminaFactor);

            var levelManager = data.gameObject.GetComponent<vLevelManager>();

            levelManager.RequiredXPForNextLevel = playerContent.requiredXPForNextLevel;
            levelManager.RequiredXPForPreviousLevel = playerContent.requiredXPForPreviousLevel;
            levelManager.CurrentLevel = playerContent.currentLevel;
            levelManager.CurrentExperience = playerContent.currentExperience;

            levelManager.CurrentStats.Clear();

            var stats = new List<StatComponent>();

            playerContent.stats.ForEach(stat =>
            {
                var strategy = vLevelManager.GetStatHandler(stat.type);
                var trend = levelManager.StatTrends.Find(t => t.trendID.Equals(stat.trendID));
                var statC = strategy.Initialize(stat, trend);
                stats.Add(statC);
            });

            levelManager.SetStats(stats);

            var position = new Vector3(float.Parse(playerContent.position_x), float.Parse(playerContent.position_y), float.Parse(playerContent.position_z));
            var rotation = new Quaternion(float.Parse(playerContent.rotation_x), float.Parse(playerContent.rotation_y), float.Parse(playerContent.rotation_z), float.Parse(playerContent.rotation_w));

            data.gameObject.transform.position = position;
            data.gameObject.transform.rotation = rotation;


            var animator = data.gameObject.GetComponent<Animator>();
            if (animator != null)
            {
                for (int i = 0; i < animator.layerCount; i++)
                {
                    var info = playerContent.animatorInfo.Find(anim => anim.layer == i);
                    if (info != null)
                    {
                        animator.SetLayerWeight(i, info.layerWeight);
                        animator.Play(info.nameHash, i, info.currentTimeOfAnimation);
                    }
                }
            }
        }

        public void DeserializeMultiple<T>(ref List<T> data, IFormatter formatter, CryptoStream stream) where T : MonoBehaviour
        {
            throw new PlayerSerializationStrategyException("Player object cannot be in a list. There can be only one instance of a player");
        }


        public SerializedContent GetSerializableContent<T>(T mono) where T : MonoBehaviour
        {

            var go = mono.gameObject;

            var instance = vQuestSystemManager.Instance;

            var data = new SerializedContent();
            var finalData = new SerializedContent();

            var tpController = mono.GetComponent<vThirdPersonController>();
            var itemManager = mono.GetComponent<vItemManager>();
            var questManager = mono.GetComponent<vQuestManager>();
            var persistenceManager = mono.GetComponent<vPersistenceManager>();
            var levelManager = mono.GetComponent<vLevelManager>();

            var equippedItems = itemManager.GetAllItemInAllEquipAreas();

            data.active = go.gameObject.activeSelf;
            data.name = go.name;

            data.parentName = go.transform.parent ? go.transform.parent.gameObject.name : "";

            data.position_x = go.transform.position.x.ToString();
            data.position_y = go.transform.position.y.ToString();
            data.position_z = go.transform.position.z.ToString();

            data.rotation_x = go.transform.rotation.x.ToString();
            data.rotation_y = go.transform.rotation.y.ToString();
            data.rotation_z = go.transform.rotation.z.ToString();
            data.rotation_w = go.transform.rotation.w.ToString();

            var animator = go.GetComponent<Animator>();
            if (animator != null)
            {
                for (int i = 0; i < animator.layerCount; i++)
                {
                    AnimatorStateInformation info = new AnimatorStateInformation();
                    var stateInfo = animator.GetCurrentAnimatorStateInfo(i);

                    info.layer = i;
                    info.nameHash = stateInfo.fullPathHash;
                    info.layerWeight = animator.GetLayerWeight(i);
                    info.currentTimeOfAnimation = stateInfo.normalizedTime;

                    data.animatorInfo.Add(info);

                }
            }

            var playerData = new PlayerSerializedContent(data);

            playerData.activeSceneName = SceneManager.GetActiveScene().name;

            playerData.activeQuestID = questManager != null ? instance.ActiveQuest : -1;

            playerData.currentHealth = tpController.currentHealth;
            playerData.currentStamina = tpController.currentStamina;
            playerData.currentMaxHealth = tpController.maxHealth;
            playerData.currentMaxStamina = tpController.maxStamina;
            playerData.currentStaminaRecoveryDelay = tpController.currentStaminaRecoveryDelay;
            playerData.currentHealthRecoveryDelay = tpController.currentHealthRecoveryDelay;

            if (levelManager != null)
            {

                levelManager.CurrentStats.ForEach(stat =>
                {
                    var serializedStat = new StatComponentSerialized();
                    serializedStat.type = stat.type;
                    serializedStat.value = stat.value;
                    serializedStat.trendID = stat.trendID;
                    serializedStat.isBool = stat.isBool;
                    serializedStat.isNumeric = stat.isNumeric;
                    serializedStat.isPercentage = stat.isPercentage;
                    playerData.stats.Add(serializedStat);
                });

            }

            playerData.lastCheckPointPerScene = persistenceManager.lastCheckPointInScene;

            playerData.deletedGOs = persistenceManager.DeletedGameObjects;

            if (itemManager != null)
            {
                itemManager.items.ForEach(item =>
                {

                    ItemReference reference = new ItemReference(item.id);

                    reference.amount = item.amount;
                    reference.attributes = item.attributes.CopyAsNew();

                    var equipAreas = itemManager.inventory.equipAreas;

                    var equipItem = equippedItems.Find(equippedItem => equippedItem.id == item.id);
                    if (equipItem != null)
                    {
                        if (equipItem.isInEquipArea)
                            for (int i = 0; i < equipAreas.Length; i++)
                            {
                                try
                                {
                                    if (itemManager.ItemIsInSpecificEquipArea(equipItem.id, i))
                                    {
                                        reference.indexArea = i;

                                        if (itemManager.ItemIsInSomeEquipPont(equipItem.id))
                                            reference.autoEquip = true;
                                        else
                                            reference.autoEquip = false;

                                        if (equipAreas[i].currentEquipedItem != null && equipAreas[i].currentEquipedItem.id == equipItem.id)
                                            playerData.equippedItems.Add(new PlayerSerializedContent.EquipAreaContent()
                                            {
                                                areaIndex = i,
                                                currentEquipItemId = equipItem.id,
                                                currentEquipSlotId = equipAreas[i].indexOfEquipedItem
                                            });
                                    }
                                }
                                catch (Exception e)
                                {
                                    //move on
                                }
                            }
                    }

                    playerData.items.Add(reference);

                });
            }

            if (questManager != null)
            {
                playerData.quests = instance.Proxies;
            }

            if (levelManager != null)
            {
                playerData.requiredXPForNextLevel = levelManager.RequiredXPForNextLevel;
                playerData.requiredXPForPreviousLevel = levelManager.RequiredXPForPreviousLevel;
                playerData.currentLevel = levelManager.CurrentLevel;
                playerData.currentExperience = levelManager.CurrentExperience;
            }
            finalData = playerData;

            return finalData;
        }

        public List<SerializedContent> GetSerializableContent<T>(List<T> list) where T : MonoBehaviour
        {
            throw new PlayerSerializationStrategyException("Player object cannot be in a list. There can be only one instance of a player");
        }

        public void HandleDeserializedInstance<T, X>(ref X monobehaviour, ref T serializedContent) where T : SerializedContent
                                                                                                   where X : MonoBehaviour
        {
            if (!serializedContent.BelongsToActiveScene)
                if (!serializedContent.GetType().Equals(typeof(PlayerSerializedContent)))
                    return;

            if (serializedContent is PlayerSerializedContent)
            {
                var player = vThirdPersonController.instance;
                var itemManager = vThirdPersonController.instance.GetComponent<vItemManager>();
                var questManager = vThirdPersonController.instance.GetComponent<vQuestManager>();
                var persistenceManager = vThirdPersonController.instance.GetComponent<vPersistenceManager>();
                var levelManager = vThirdPersonController.instance.GetComponent<vLevelManager>();

                var playerContent = serializedContent as PlayerSerializedContent;

                player.healthRecovery = playerContent.healthRecovery;
                player.healthRecoveryDelay = playerContent.healthRecoveryDelay;
                player.currentHealthRecoveryDelay = playerContent.currentHealthRecoveryDelay;

                var healthFactor = (int)(playerContent.currentHealth - player.currentHealth);
                player.ChangeHealth(healthFactor);

                var staminaFactor = (int)(playerContent.currentStamina - player.currentStamina);
                player.ChangeStamina(staminaFactor);

                player.staminaRecovery = playerContent.staminaRecovery;
                player.currentStaminaRecoveryDelay = playerContent.currentStaminaRecoveryDelay;


                var maxHealthFactor = (int)(playerContent.currentMaxHealth - player.MaxHealth);
                player.ChangeMaxHealth(maxHealthFactor);

                var maxStaminaFactor = (int)(playerContent.currentMaxStamina - player.maxStamina);
                player.ChangeMaxStamina(maxStaminaFactor);

                persistenceManager.lastCheckPointInScene = playerContent.lastCheckPointPerScene;

                if (playerContent.deletedGOs != null)
                {

                    var listOfGOs = GameObject.FindObjectsOfType<vCanSaveYou>().vToList();
                    playerContent.deletedGOs.RemoveAll(deletedGO => !deletedGO.Contains(SceneManager.GetActiveScene().name + "_"));
                    playerContent.deletedGOs.ForEach(deletedGO =>
                    {
                        string go_guid = deletedGO.Split('_')[1];
                        var go = listOfGOs.Find(g => g.guid.Equals(go_guid));
                        if (go != null)
                        {
                            go.gameObject.SetActive(false);
                        }
                    });

                }

                var lastCheckPointCurrentScene = persistenceManager.lastCheckPointInScene.Find(chkpt => chkpt.scene.Equals(SceneManager.GetActiveScene().name));
                if (lastCheckPointCurrentScene != null)
                {
                    var checkpoint = persistenceManager.sceneCheckPoints.Find(chkpt => chkpt.name.Equals(lastCheckPointCurrentScene.checkpointName));
                    if (checkpoint)
                    {
                        player.transform.position = checkpoint.gameObject.transform.position;
                        player.transform.rotation = checkpoint.gameObject.transform.rotation;
                    }
                }
                else
                {
                    var loadLevel = GameObject.FindObjectOfType<vLoadLevel>();
                    if (loadLevel != null)
                    {
                        var spawnPoint = GameObject.Find(loadLevel.spawnPointName);
                        if (spawnPoint)
                        {
                            player.transform.position = spawnPoint.transform.position;
                            player.transform.rotation = spawnPoint.transform.rotation;
                        }
                    }
                    else
                    {
                        player.transform.position = new Vector3(float.Parse(playerContent.position_x), float.Parse(playerContent.position_y), float.Parse(playerContent.position_z));
                        player.transform.rotation = new Quaternion(float.Parse(playerContent.rotation_x), float.Parse(playerContent.rotation_y), float.Parse(playerContent.rotation_z), float.Parse(playerContent.rotation_w));
                    }
                }

                var animator = player.GetComponent<Animator>();
                if (animator != null)
                {
                    for (int i = 0; i < animator.layerCount; i++)
                    {
                        var info = playerContent.animatorInfo.Find(anim => anim.layer == i);
                        if (info != null)
                        {
                            animator.SetLayerWeight(i, info.layerWeight);
                            animator.Play(info.nameHash, i, info.currentTimeOfAnimation);
                        }
                    }
                }

                if (levelManager != null)
                {

                    levelManager.RequiredXPForPreviousLevel = playerContent.requiredXPForPreviousLevel;
                    levelManager.RequiredXPForNextLevel = playerContent.requiredXPForNextLevel;
                    levelManager.CurrentLevel = playerContent.currentLevel;
                    levelManager.CurrentExperience = playerContent.currentExperience;

                    vExperienceHUD.instance.UpdateXPSlider();

                    levelManager.CurrentStats.Clear();

                    var stats = new List<StatComponent>();

                    playerContent.stats.ForEach(stat =>
                    {
                        var strategy = vLevelManager.GetStatHandler(stat.type);
                        var trend = levelManager.StatTrends.Find(t => t.trendID.Equals(stat.trendID));
                        var statC = strategy.Initialize(stat, trend);
                        stats.Add(statC);
                    });

                    levelManager.SetStats(stats);

                }

                if (itemManager != null)
                {

                    itemManager.items.Clear();

                    playerContent.items.ForEach(item =>
                    {
                        var itemScriptable = itemManager.itemListData.items.Find(itm => itm.id == item.id);
                        itemManager.AddItem(item);
                        itemManager.AutoEquipItem(itemScriptable, item.indexArea, true);
                    });

                    var unEquipTheseAreas = itemManager.inventory.equipAreas.vToList();

                    foreach (var equippedItem in playerContent.equippedItems)
                    {
                        itemManager.inventory.equipAreas[equippedItem.areaIndex].SetEquipSlot(equippedItem.currentEquipSlotId);
                        unEquipTheseAreas.RemoveAll(area => area.name.Equals(itemManager.inventory.equipAreas[equippedItem.areaIndex].name));
                    }

                    foreach (var area in unEquipTheseAreas)
                    {
                        itemManager.inventory.UnequipItem(area, area.currentEquipedItem);
                    }
                }


                if (questManager)
                {
                    var instance = vQuestSystemManager.Instance;

                    if (playerContent.quests.Count > 0)
                    {

                        var list = playerContent.quests;

                        instance.ResetProxies(list);
                        instance.QuestManager.startQuests.ForEach(quest =>
                        {
                            if (instance.GetQuestState(quest.id) == vQuestState.Completed)
                                list.RemoveAll(q => q.Id == quest.id);
                        });

                        list.ForEach(q =>
                        {
                            var quest = instance.QuestManager.questListData.quests.Find(i => i.id == q.Id);
                            if (quest != null)
                            {
                                var qm = vThirdPersonController.instance.GetComponent<vQuestManager>();
                                qm.AddQuest(
                                    new QuestReference(q.Id, quest.provider, quest.QuestTarget));
                            }
                        });
                    }

                    if (playerContent.activeQuestID != -1)
                    {

                        instance.onActiveQuestChanged.Invoke(playerContent.activeQuestID, true, false, true);

                        var timed = playerContent.quests.FindAll(q => instance.GetDuration(q.Id) > 0f && q.State == vQuestState.InProgress
                           && q.ElapsedDuration > 0f);
                        timed.ForEach(t =>
                        {
                            instance.QuestManager.StartCountDown(t.Id);
                        });
                    }

                }
            }
        }
    }
}
