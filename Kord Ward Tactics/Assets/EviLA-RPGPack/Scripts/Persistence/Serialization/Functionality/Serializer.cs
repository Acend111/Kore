using UnityEngine;
using System;
using System.Security.Cryptography;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace EviLA.AddOns.RPGPack.Persistence.Serialization
{
    public enum SerializationStrategies
    {
        Default,
        Player,
        Vendor,
        InvectorAI,
        QuestSystemSpawner,
        ItemCollection,
        InvectorSimpleDoor
    }

    public enum SerializationMode
    {
        Single,
        List
    }

    public static partial class Serializer
    {

        public static void Serialize<T>(List<T> toBeSerialized,IFormatter formatter, CryptoStream writer) where T : MonoBehaviour
        {

            toBeSerialized.RemoveAll(mono => mono == null || mono.gameObject == null);

            ISerializationStrategy strategy = GetStrategyByType(typeof(T).Name);

            List<SerializedContent> content = strategy.GetSerializableContent(toBeSerialized);
            strategy.Serialize(content, formatter, writer);

        }

        public static void Serialize<T>(T toBeSerialized,IFormatter formatter, CryptoStream stream) where T : MonoBehaviour
        {
            if ( toBeSerialized == null )
                return;

            ISerializationStrategy strategy = GetStrategyByBehaviour(toBeSerialized);

            SerializedContent content = strategy.GetSerializableContent(toBeSerialized);

            strategy.Serialize(content, formatter, stream);

        }

        public static void DeserializeInto<T>(ref List<T> DeserializeInto, IFormatter formatter, CryptoStream stream, SerializationMode mode) where T : MonoBehaviour {

            DeserializeInto.RemoveAll(deserialze => deserialze == null || deserialze.gameObject == null);

            if (DeserializeInto.Count == 0)
                return;

            ISerializationStrategy strategy = GetStrategyByType(typeof(T).Name);


            switch (mode)
            {
                case SerializationMode.List:
                    strategy.DeserializeMultiple(ref DeserializeInto,formatter, stream);
                    break;
                default:
                    throw new Exception("Quest System Exception - Invalid Serialization Mode");
            }

        }

        public static void DeserializeInto<T>(ref T DeserializeInto, IFormatter formatter, CryptoStream stream, SerializationMode mode) where T : MonoBehaviour {

            if (DeserializeInto == null || DeserializeInto.gameObject == null)
                return;

            ISerializationStrategy strategy = GetStrategyByType(typeof(T).Name);

            switch (mode)
            {
                case SerializationMode.Single:
                    strategy.DeserializeSingle(ref DeserializeInto, formatter, stream);
                    break;
                default:
                    throw new Exception("Quest System Exception - Invalid Serialization Mode");
            }

        }

        public static List<SerializedContent> DeserializeContentOnly(IFormatter formatter, CryptoStream stream)
        {
            var content = new System.Collections.Generic.List<SerializedContent>();
            content = formatter.Deserialize(stream) as List<SerializedContent>;
            return content;
        }

    }
}
