
using UnityEngine;
using System.Security.Cryptography;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace EviLA.AddOns.RPGPack.Persistence.Serialization
{
    public interface ISerializationStrategy
    {
        SerializedContent GetSerializableContent<T>(T mono) where T : MonoBehaviour;
        List<SerializedContent> GetSerializableContent<T>(List<T> mono) where T : MonoBehaviour;

        void DeserializeSingle<T>(ref T data, IFormatter formatter, CryptoStream stream) where T : MonoBehaviour;
        void DeserializeMultiple<T>(ref List<T> data, IFormatter formatter, CryptoStream stream) where T : MonoBehaviour;

        void HandleDeserializedInstance<T,X>(ref X monobehaviour, ref T serializedContent) where T : SerializedContent
                                                                                           where X : MonoBehaviour;

        void Serialize<T>(T data, IFormatter formatter, CryptoStream stream) where T : SerializedContent;
        void Serialize<T>(List<T> data, IFormatter formatter, CryptoStream stream) where T : SerializedContent;
    }
}
