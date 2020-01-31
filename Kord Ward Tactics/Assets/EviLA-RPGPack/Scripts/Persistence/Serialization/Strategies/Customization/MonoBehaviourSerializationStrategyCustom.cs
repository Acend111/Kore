using UnityEngine;

using Invector;
using Invector.vCharacterController;
using EviLA.AddOns.RPGPack.Spawners;
using Invector.vItemManager;
using Invector.vCharacterController.AI;

namespace EviLA.AddOns.RPGPack.Persistence.Serialization
{
    public partial class MonoBehaviourSerializationStrategy : ISerializationStrategy
    {

        void ISerializationStrategy.HandleDeserializedInstance<T, X>(ref X monobehaviour, ref T serializedContent)
        {
            if (monobehaviour.GetComponent<vThirdPersonController>())
            {
                var strategy = Serializer.GetSerializationStrategy(SerializationStrategies.Player);
                strategy.HandleDeserializedInstance(ref monobehaviour, ref serializedContent);
            }

            else if (monobehaviour.GetComponent<vQuestSystemSpawner>())
            {
                var strategy = Serializer.GetSerializationStrategy(SerializationStrategies.QuestSystemSpawner);
                strategy.HandleDeserializedInstance(ref monobehaviour, ref serializedContent);
            }

            else if (monobehaviour.GetComponent<v_AIController>())
            {
                var strategy = Serializer.GetSerializationStrategy(SerializationStrategies.InvectorAI);
                strategy.HandleDeserializedInstance(ref monobehaviour, ref serializedContent);
            }

            else if (monobehaviour.GetComponent<vItemSeller>())
            {
                var strategy = Serializer.GetSerializationStrategy(SerializationStrategies.Vendor);
                strategy.HandleDeserializedInstance(ref monobehaviour, ref serializedContent);
            }

            else if (monobehaviour.GetComponent<vItemCollection>() || monobehaviour.GetComponent<vQuestItemCollection>())
            {
                var strategy = Serializer.GetSerializationStrategy(SerializationStrategies.ItemCollection);
                strategy.HandleDeserializedInstance(ref monobehaviour, ref serializedContent);
            }

            else if (monobehaviour.GetComponent<vSimpleDoor>())
            {
                var strategy = Serializer.GetSerializationStrategy(SerializationStrategies.InvectorSimpleDoor);
                strategy.HandleDeserializedInstance(ref monobehaviour, ref serializedContent);
            }

        }

        SerializedContent GetSerializedContentByBehaviourGameObject(MonoBehaviour mono)
        {
            SerializedContent data = new SerializedContent();

            if (mono.gameObject.GetComponent<vThirdPersonController>())
            {

                var monoPlayer = mono.GetComponent<vThirdPersonController>();
                var strategy = Serializer.GetSerializationStrategy(SerializationStrategies.Player);
                var playerContent = strategy.GetSerializableContent(monoPlayer);
                data = playerContent;
            }

            else if (mono.gameObject.GetComponent<vQuestSystemSpawner>())
            {
                var monoSpawner = mono.GetComponent<vQuestSystemSpawner>();
                var strategy = Serializer.GetSerializationStrategy(SerializationStrategies.QuestSystemSpawner);
                var spawnerContent = strategy.GetSerializableContent(monoSpawner);
                data = spawnerContent;
            }

            else if (mono.gameObject.GetComponent<v_AIController>())
            {
                var monoAI = mono.GetComponent<v_AIController>();
                var strategy = Serializer.GetSerializationStrategy(SerializationStrategies.InvectorAI);
                var spawnerContent = strategy.GetSerializableContent(monoAI);
                data = spawnerContent;
            }

            else if (mono.gameObject.GetComponent<vItemSeller>())
            {
                var vendor = mono.GetComponent<vItemSeller>();
                var strategy = Serializer.GetSerializationStrategy(SerializationStrategies.Vendor);
                var spawnerContent = strategy.GetSerializableContent(vendor);
                data = spawnerContent;
            }

            else if (mono.gameObject.GetComponent<vQuestItemCollection>() || mono.gameObject.GetComponent<vItemCollection>())
            {
                vItemCollection itemCollection = mono.GetComponent<vItemCollection>();

                if (itemCollection == null)
                    itemCollection = mono.GetComponent<vQuestItemCollection>();

                var strategy = Serializer.GetSerializationStrategy(SerializationStrategies.ItemCollection);
                var spawnerContent = strategy.GetSerializableContent(itemCollection);
                data = spawnerContent;
            }

            else if (mono.gameObject.GetComponent<vSimpleDoor>())
            {
                var door = mono.GetComponent<vSimpleDoor>();
                var strategy = Serializer.GetSerializationStrategy(SerializationStrategies.InvectorSimpleDoor);
                var spawnerContent = strategy.GetSerializableContent(door);
                data = spawnerContent;
            }

            return data;
        }
    }
}
