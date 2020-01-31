using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using UnityEngine;

using Invector;
using Invector.vCharacterController.AI;

namespace EviLA.AddOns.RPGPack.Persistence.Serialization
{

    public class vAIControllerSerializationStrategyException : Exception
    {
        public vAIControllerSerializationStrategyException(string message) : base(message) { }
    }

    public class vAIControllerSerializationStrategy : ISerializationStrategy
    {

        public void Serialize<T>(T data, IFormatter formatter, CryptoStream stream) where T : SerializedContent
        {
            try
            {
                formatter.Serialize(stream, data);
            }
            catch (SerializationException e)
            {
                throw new vAIControllerSerializationStrategyException("Unable to Serialize Monobehavior");
            }
        }

        public void Serialize<T>(List<T> data, IFormatter formatter, CryptoStream stream) where T : SerializedContent
        {
            try
            {
                formatter.Serialize(stream, data);
            }
            catch (SerializationException e)
            {
                throw e;
            }
        }

        public void DeserializeSingle<T>(ref T data, IFormatter formatter, CryptoStream stream) where T : MonoBehaviour
        {
            vAIControllerSerializedContent content = ((SerializedContent)formatter.Deserialize(stream)) as vAIControllerSerializedContent;

            data.gameObject.SetActive(content.active);

            var _ai = data.gameObject.GetComponent<v_AIController>();

            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;

            position.x = float.Parse(content.position_x);
            position.y = float.Parse(content.position_y);
            position.z = float.Parse(content.position_z);

            rotation.x = float.Parse(content.rotation_x);
            rotation.y = float.Parse(content.rotation_y);
            rotation.z = float.Parse(content.rotation_z);
            rotation.w = float.Parse(content.rotation_w);

            data.transform.position = position;
            data.transform.rotation = rotation;

            var healthFactor = (int)(_ai.currentHealth - content.currentHealth);
            _ai.ChangeHealth(healthFactor);

        }

        public void DeserializeMultiple<T>(ref List<T> data, IFormatter formatter, CryptoStream stream) where T : MonoBehaviour
        {
            var contents = ((List<SerializedContent>)formatter.Deserialize(stream)).Cast<vAIControllerSerializedContent>().ToList();

            List<T> copied = data.vCopy<T>();

            contents.ForEach(content =>
            {

                var _go = copied.Find(obj => obj.name.Equals(content.name)).gameObject;
                var _ai = _go.GetComponent<v_AIController>();


                if (_go != null)
                {
                    _go.SetActive(content.active);

                    Vector3 position = Vector3.zero;
                    Quaternion rotation = Quaternion.identity;

                    position.x = float.Parse(content.position_x);
                    position.y = float.Parse(content.position_y);
                    position.z = float.Parse(content.position_z);

                    rotation.x = float.Parse(content.rotation_x);
                    rotation.y = float.Parse(content.rotation_y);
                    rotation.z = float.Parse(content.rotation_z);
                    rotation.w = float.Parse(content.rotation_w);

                    _go.transform.position = position;
                    _go.transform.rotation = rotation;

                    var healthFactor = (int)(_ai.currentHealth - content.currentHealth);
                    _ai.ChangeHealth(healthFactor);

                    var animator = _ai.GetComponent<Animator>();
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
            });

            data = copied.vCopy<T>();
        }


        public SerializedContent GetSerializableContent<T>(T mono) where T : MonoBehaviour
        {

            var go = mono.gameObject;

            var data = new SerializedContent();
            var finalData = new SerializedContent();

            var ai_controller = mono.GetComponent<v_AIController>();

            if (ai_controller == null)
                throw new vAIControllerSerializationStrategyException("Unabled to find ai controller for Serialization");

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

            var aiData = new vAIControllerSerializedContent(data);

            aiData.currentHealth = ai_controller.currentHealth;

            finalData = aiData;

            return finalData;
        }

        public List<SerializedContent> GetSerializableContent<T>(List<T> list) where T : MonoBehaviour
        {
            List<SerializedContent> finalDataSet = new List<SerializedContent>();

            list.ForEach(mono =>
            {
                finalDataSet.Add(GetSerializableContent(mono));
            });

            return finalDataSet;
        }

        public void HandleDeserializedInstance<T, X>(ref X monobehaviour, ref T serializedContent)
            where T : SerializedContent
            where X : MonoBehaviour
        {

            if (!serializedContent.BelongsToActiveScene)
                if (!serializedContent.GetType().Equals(typeof(PlayerSerializedContent)))
                    return;

            var _ai = monobehaviour.gameObject.GetComponent<v_AIController>();
            var content = serializedContent as vAIControllerSerializedContent;

            _ai.ChangeHealth((int)content.currentHealth);

            var animator = _ai.GetComponent<Animator>();
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
