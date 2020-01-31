using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using Invector;
using UnityEngine;

namespace EviLA.AddOns.RPGPack.Persistence.Serialization
{

    public class vSimpleDoorSerializationStrategyException : Exception
    {
        public vSimpleDoorSerializationStrategyException(string message) : base(message) { }
    }

    public class vSimpleDoorSerializationStrategy : ISerializationStrategy
    {

        public void Serialize<T>(T data, IFormatter formatter, CryptoStream stream) where T : SerializedContent
        {
            try
            {
                formatter.Serialize(stream, data);
            }
            catch (SerializationException e)
            {
                throw new vSimpleDoorSerializationStrategyException("Unable to Serialize door object");
            }
        }

        public void Serialize<T>(List<T> data, IFormatter formatter, CryptoStream stream) where T : SerializedContent
        {
            throw new vSimpleDoorSerializationStrategyException("This method won't be invoked through this class");
        }

        public void DeserializeSingle<T>(ref T data, IFormatter formatter, CryptoStream stream) where T : MonoBehaviour
        {
            vSimpleDoorSerializedContent content = ((SerializedContent)formatter.Deserialize(stream)) as vSimpleDoorSerializedContent;

            var position = new Vector3(float.Parse(content.position_x), float.Parse(content.position_y), float.Parse(content.position_z));
            var rotation = new Quaternion(float.Parse(content.rotation_x), float.Parse(content.rotation_y), float.Parse(content.rotation_z), float.Parse(content.rotation_w));

            data.gameObject.transform.position = position;
            data.gameObject.transform.rotation = rotation;

            var door = data.gameObject.GetComponent<vSimpleDoor>();

            door.SetAutoClose(content.autoClose);
            door.SetAutoOpen(content.autoOpen);

            var animator = data.gameObject.GetComponent<Animator>();
            if (animator != null)
            {
                for (int i = 0; i < animator.layerCount; i++)
                {
                    var info = content.animatorInfo.Find(anim => anim.layer == i);
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
            throw new vSimpleDoorSerializationStrategyException("This method won't be invoked through this class");
        }


        public SerializedContent GetSerializableContent<T>(T mono) where T : MonoBehaviour
        {

            var go = mono.gameObject;

            var finalData = new SerializedContent();

            finalData.active = go.gameObject.activeSelf;
            finalData.name = go.name;

            finalData.parentName = go.transform.parent ? go.transform.parent.gameObject.name : "";

            finalData.position_x = go.transform.position.x.ToString();
            finalData.position_y = go.transform.position.y.ToString();
            finalData.position_z = go.transform.position.z.ToString();

            finalData.rotation_x = go.transform.rotation.x.ToString();
            finalData.rotation_y = go.transform.rotation.y.ToString();
            finalData.rotation_z = go.transform.rotation.z.ToString();
            finalData.rotation_w = go.transform.rotation.w.ToString();

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

                    finalData.animatorInfo.Add(info);

                }
            }

            var doorContent = new vSimpleDoorSerializedContent(finalData);
            var door = go.GetComponent<vSimpleDoor>();

            doorContent.autoOpen = door.autoOpen;
            doorContent.autoClose = door.autoClose;

            finalData = doorContent;

            return finalData;
        }

        public List<SerializedContent> GetSerializableContent<T>(List<T> list) where T : MonoBehaviour
        {
            throw new vSimpleDoorSerializationStrategyException("This method won't be invoked through this class");
        }

        public void HandleDeserializedInstance<T, X>(ref X monobehaviour, ref T serializedContent) where T : SerializedContent
                                                                                                   where X : MonoBehaviour
        {
            if (!serializedContent.BelongsToActiveScene)
                if (!serializedContent.GetType().Equals(typeof(vSimpleDoorSerializedContent)))
                    return;

            if (serializedContent is vSimpleDoorSerializedContent)
            {
                vSimpleDoorSerializedContent content = serializedContent as vSimpleDoorSerializedContent;

                var position = new Vector3(float.Parse(content.position_x), float.Parse(content.position_y), float.Parse(content.position_z));
                var rotation = new Quaternion(float.Parse(content.rotation_x), float.Parse(content.rotation_y), float.Parse(content.rotation_z), float.Parse(content.rotation_w));

                monobehaviour.gameObject.transform.position = position;
                monobehaviour.gameObject.transform.rotation = rotation;

                var door = monobehaviour.gameObject.GetComponent<vSimpleDoor>();

                door.SetAutoClose(content.autoClose);
                door.SetAutoOpen(content.autoOpen);


                var animator = monobehaviour.gameObject.GetComponent<Animator>();
                if (animator != null)
                {
                    for (int i = 0; i < animator.layerCount; i++)
                    {
                        var info = content.animatorInfo.Find(anim => anim.layer == i);
                        if (info != null)
                        {
                            animator.SetLayerWeight(i, info.layerWeight);
                            animator.Play(info.nameHash, i, info.currentTimeOfAnimation);
                        }
                    }
                }
            }
        }
    }
}
