using Invector.vItemManager;
using EviLA.AddOns.RPGPack.Experience;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EviLA.AddOns.RPGPack.Persistence.Serialization
{
    [Serializable]
    public class SerializedContent
    {
        public string guid;
        public string scene;
        public string name;
        public string parentName;
        public bool active;

        public string position_x;
        public string position_y;
        public string position_z;

        public string rotation_x;
        public string rotation_y;
        public string rotation_z;
        public string rotation_w;

        public List<AnimatorStateInformation> animatorInfo = new List<AnimatorStateInformation>();
        public List<AnimationStateInformation> animationInfo = new List<AnimationStateInformation>();
        public List<AudioStateInformation> audioInfo = new List<AudioStateInformation>();

        public SerializedContent()
        {
            scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }

        public bool BelongsToActiveScene
        {
            get { return this.scene.Equals(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name); }
        }
    }

    //vQuestSystemSpawner
    [Serializable]
    public class QuestSystemSpawnerSerializedContent : SerializedContent
    {
        public int waveCount;
        public int waveInterval;
        public bool waitTillCurrentWaveDestroyed;

        public bool spawnOnEnterRegion;
        public bool spawnOnlyIfQuestIsInProgress;
        public bool destroySpawnedOnQuestFailure;
        public bool drawGizmos;

        public int maxInstanceCount;
        public int currentWaveCount;
        public int currentWaveInstanceCount;

        public int questID;

        public List<List<List<SerializedContent>>> pooledObjects = new List<List<List<SerializedContent>>>();
        public List<string> pooledSpawners = new List<string>();

        public QuestSystemSpawnerSerializedContent() : base()
        {

        }

        public QuestSystemSpawnerSerializedContent(SerializedContent data)
        {
            name = data.name;
            parentName = data.parentName;
            active = data.active;

            position_x = data.position_x;
            position_y = data.position_y;
            position_z = data.position_z;

            rotation_x = data.rotation_x;
            rotation_y = data.rotation_y;
            rotation_z = data.rotation_z;
            rotation_w = data.rotation_w;

            animatorInfo = data.animatorInfo;
            animationInfo = data.animationInfo;
        }
    }

    [Serializable]
    public class vAIControllerSerializedContent : SerializedContent
    {
        public float currentHealth;
        public float currentStamina;

        public vAIControllerSerializedContent() : base()
        {

        }

        public vAIControllerSerializedContent(SerializedContent data)
        {
            name = data.name;
            parentName = data.parentName;
            active = data.active;

            position_x = data.position_x;
            position_y = data.position_y;
            position_z = data.position_z;

            rotation_x = data.rotation_x;
            rotation_y = data.rotation_y;
            rotation_z = data.rotation_z;
            rotation_w = data.rotation_w;

            animatorInfo = data.animatorInfo;
            animationInfo = data.animationInfo;
        }
    }


    [Serializable]
    public class PlayerSerializedContent : SerializedContent
    {

        [Serializable]
        public class EquipAreaContent
        {
            public int areaIndex;
            public int currentEquipItemId;
            public int currentEquipSlotId;
        }
        public int activeQuestID;
        public string activeSceneName;

        public float currentHealth;
        public float currentStamina;

        public float currentMaxHealth;
        public float currentMaxStamina;

        public int currentLevel;
        public double currentExperience;
        public double requiredXPForNextLevel;
        public double requiredXPForPreviousLevel;

        public float healthRecovery;
        public float currentHealthRecoveryDelay;
        public float healthRecoveryDelay;

        public float staminaRecovery;
        public float currentStaminaRecoveryDelay;

        public List<ItemReference> items = new List<ItemReference>();
        public Dictionary<int, int> equipItemInequipArea = new Dictionary<int, int>();
        public List<EquipAreaContent> equippedItems = new List<EquipAreaContent>();
        public List<vQuestProxy> quests = new List<vQuestProxy>();
        public List<LastCheckPoint> lastCheckPointPerScene = new List<LastCheckPoint>();
        public List<StatComponentSerialized> stats = new List<StatComponentSerialized>();
        public List<string> deletedGOs = new List<string>();


        public PlayerSerializedContent() : base()
        {

        }

        public PlayerSerializedContent(SerializedContent data)
        {
            name = data.name;
            parentName = data.parentName;
            active = data.active;

            position_x = data.position_x;
            position_y = data.position_y;
            position_z = data.position_z;

            rotation_x = data.rotation_x;
            rotation_y = data.rotation_y;
            rotation_z = data.rotation_z;
            rotation_w = data.rotation_w;

            animatorInfo = data.animatorInfo;
            animationInfo = data.animationInfo;
        }
    }

    [Serializable]
    public class vItemSellerSerializedContent : SerializedContent
    {
        public List<ItemReference> vendorItems = new List<ItemReference>();

        public vItemSellerSerializedContent() : base()
        {

        }

        public vItemSellerSerializedContent(SerializedContent data)
        {
            name = data.name;
            parentName = data.parentName;
            active = data.active;

            position_x = data.position_x;
            position_y = data.position_y;
            position_z = data.position_z;

            rotation_x = data.rotation_x;
            rotation_y = data.rotation_y;
            rotation_z = data.rotation_z;
            rotation_w = data.rotation_w;

            animatorInfo = data.animatorInfo;
            animationInfo = data.animationInfo;
        }
    }

    [Serializable]
    public class vItemCollectionSerailizedContent : SerializedContent
    {
        public bool wereItemsCollected;
        public AnimatorStateInformation animation;

        public vItemCollectionSerailizedContent() : base()
        {

        }

        public vItemCollectionSerailizedContent(SerializedContent data)
        {
            name = data.name;
            parentName = data.parentName;
            active = data.active;

            position_x = data.position_x;
            position_y = data.position_y;
            position_z = data.position_z;

            rotation_x = data.rotation_x;
            rotation_y = data.rotation_y;
            rotation_z = data.rotation_z;
            rotation_w = data.rotation_w;

            animatorInfo = data.animatorInfo;
            animationInfo = data.animationInfo;
        }
    }

    [Serializable]
    public class vSimpleDoorSerializedContent : SerializedContent
    {
        public bool autoOpen;
        public bool autoClose;

        public vSimpleDoorSerializedContent(SerializedContent data)
        {
            name = data.name;
            parentName = data.parentName;
            active = data.active;

            position_x = data.position_x;
            position_y = data.position_y;
            position_z = data.position_z;

            rotation_x = data.rotation_x;
            rotation_y = data.rotation_y;
            rotation_z = data.rotation_z;
            rotation_w = data.rotation_w;

            animatorInfo = data.animatorInfo;
            animationInfo = data.animationInfo;
        }

    }


}
