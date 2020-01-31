using UnityEngine;
using System;
using Invector;
using Invector.vItemManager;
using Invector.vCharacterController;
using Invector.vCharacterController.AI;
using EviLA.AddOns.RPGPack.Spawners;

namespace EviLA.AddOns.RPGPack.Persistence.Serialization
{
    public static partial class Serializer
    {

        public static ISerializationStrategy GetSerializationStrategy(SerializationStrategies type)
        {

            ISerializationStrategy strategy;

            switch (type)
            {
                case SerializationStrategies.InvectorAI:
                    strategy = new vAIControllerSerializationStrategy();
                    break;
                case SerializationStrategies.Player:
                    strategy = new PlayerSerializationStrategy();
                    break;
                case SerializationStrategies.QuestSystemSpawner:
                    strategy = new vQuestSystemSpawnerSerializationStrategy();
                    break;
                case SerializationStrategies.Vendor:
                    strategy = new vItemSellerSerializationStrategy();
                    break;
                case SerializationStrategies.ItemCollection:
                    strategy = new vItemCollectionSerializationStrategy();
                    break;
                case SerializationStrategies.InvectorSimpleDoor:
                    strategy = new vSimpleDoorSerializationStrategy();
                    break;
                default:
                    strategy = new MonoBehaviourSerializationStrategy();
                    break;
            }

            return strategy;
        }

        public static ISerializationStrategy GetStrategyByType(String type)
        {
            ISerializationStrategy strategy;

            switch (type)
            {
                case "v_AIController":
                    strategy = GetSerializationStrategy(SerializationStrategies.InvectorAI);
                    break;

                case "vQuestSystemSpawner":
                    strategy = GetSerializationStrategy(SerializationStrategies.QuestSystemSpawner);
                    break;

                case "vThirdPersonController":
                    strategy = GetSerializationStrategy(SerializationStrategies.Player);
                    break;

                case "vItemSeller":
                    strategy = GetSerializationStrategy(SerializationStrategies.Vendor);
                    break;

                case "vItemCollection":
                    strategy = GetSerializationStrategy(SerializationStrategies.ItemCollection);
                    break;

                case "vQuestItemCollection":
                    strategy = GetSerializationStrategy(SerializationStrategies.ItemCollection);
                    break;
                case "vSimpleDoor":
                    strategy = GetSerializationStrategy(SerializationStrategies.InvectorSimpleDoor);
                    break;


                default:
                    strategy = GetSerializationStrategy(SerializationStrategies.Default);
                    break;
            }

            return strategy;
        }

        public static ISerializationStrategy GetStrategyByBehaviour(MonoBehaviour behaviour)
        {
            ISerializationStrategy strategy;

            if (behaviour is v_AIController)
                strategy = GetSerializationStrategy(SerializationStrategies.InvectorAI);
            else if (behaviour is vQuestSystemSpawner)
                strategy = GetSerializationStrategy(SerializationStrategies.QuestSystemSpawner);
            else if (behaviour is vThirdPersonController)
                strategy = GetSerializationStrategy(SerializationStrategies.Player);
            else if (behaviour is vItemSeller)
                strategy = GetSerializationStrategy(SerializationStrategies.Vendor);
            else if (behaviour is vQuestItemCollection || behaviour is vItemCollection)
                strategy = GetSerializationStrategy(SerializationStrategies.ItemCollection);
            else if (behaviour is vSimpleDoor)
                strategy = GetSerializationStrategy(SerializationStrategies.InvectorSimpleDoor);
            else
                strategy = GetSerializationStrategy(SerializationStrategies.Default);

            return strategy;
        }

        public static ISerializationStrategy GetStrategyBySerializedContent(SerializedContent content)
        {
            ISerializationStrategy strategy;

            switch (content.GetType().Name)
            {
                case "vAIControllerSerializedContent":
                    strategy = Serializer.GetSerializationStrategy(SerializationStrategies.InvectorAI);
                    break;

                case "PlayerSerializedContent":
                    strategy = Serializer.GetSerializationStrategy(SerializationStrategies.Player);
                    break;

                case "QuestSystemSpawnerSerializedContent":
                    strategy = Serializer.GetSerializationStrategy(SerializationStrategies.QuestSystemSpawner);
                    break;

                case "vItemSellerSerializedContent":
                    strategy = Serializer.GetSerializationStrategy(SerializationStrategies.Vendor);
                    break;

                case "vItemCollectionSerializedContent":
                    strategy = Serializer.GetSerializationStrategy(SerializationStrategies.ItemCollection);
                    break;

                case "vSimpleDoorSerializedContent":
                    strategy = Serializer.GetSerializationStrategy(SerializationStrategies.InvectorSimpleDoor);
                    break;

                default:
                    strategy = Serializer.GetSerializationStrategy(SerializationStrategies.Default);
                    break;
            }

            return strategy;
        }
    }
}
