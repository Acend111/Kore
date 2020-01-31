﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using UnityEngine;
using Invector.vItemManager;

namespace EviLA.AddOns.RPGPack.Persistence.Serialization
{

    public class vItemCollectionSerializationStrategyException : Exception
    {
        public vItemCollectionSerializationStrategyException(string message) : base(message) { }
    }

    public class vItemCollectionSerializationStrategy : ISerializationStrategy
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
            throw new vItemCollectionSerializationStrategyException("Player object cannot be in a list. There can be only one instance of a player");
        }

        public void DeserializeSingle<T>(ref T data, IFormatter formatter, CryptoStream stream) where T : MonoBehaviour
        {
            vItemCollectionSerailizedContent content = ((SerializedContent)formatter.Deserialize(stream)) as vItemCollectionSerailizedContent;

            var position = new Vector3(float.Parse(content.position_x), float.Parse(content.position_y), float.Parse(content.position_z));
            var rotation = new Quaternion(float.Parse(content.rotation_x), float.Parse(content.rotation_y), float.Parse(content.rotation_z), float.Parse(content.rotation_w));

            data.gameObject.transform.position = position;
            data.gameObject.transform.rotation = rotation;

            var itemCollection = data.gameObject.GetComponent<vItemCollection>();
            var trigger = itemCollection.GetComponent<BoxCollider>();

            if (content.wereItemsCollected || (trigger.isTrigger && !trigger.enabled))
            {
                itemCollection.items.ForEach(
                    item =>
                    {
                        item.amount = 0;
                    }
                );

                var animation = data.gameObject.GetComponentInParent<Animation>();
                foreach (AnimationState state in animation)
                {
                    state.normalizedTime = 1.0f;
                    animation.Play();
                }
                if (trigger.isTrigger)
                    trigger.enabled = false;
            }

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
            throw new PlayerSerializationStrategyException("Player object cannot be in a list. There can be only one instance of a player");
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

            var itemCollectionData = new vItemCollectionSerailizedContent(finalData);

            //TODO: 
            var itemCollection = go.GetComponent<vItemCollection>();

            if (itemCollection != null)
            {
                itemCollectionData.wereItemsCollected = true;
                foreach (var item in itemCollection.items)
                {
                    if (item.amount > 0)
                    {
                        itemCollectionData.wereItemsCollected = false;
                        break;
                    }
                }
            }

            finalData = itemCollectionData;

            return finalData;
        }

        public List<SerializedContent> GetSerializableContent<T>(List<T> list) where T : MonoBehaviour
        {
            throw new vItemCollectionSerializationStrategyException("Item Collection object cannot be in a list. There can be only one instance of an item collection per object containing vCanSaveYou script");
        }

        public void HandleDeserializedInstance<T, X>(ref X monobehaviour, ref T serializedContent) where T : SerializedContent
                                                                                                   where X : MonoBehaviour
        {
            if (!serializedContent.BelongsToActiveScene)
                if (!serializedContent.GetType().Equals(typeof(vItemCollectionSerailizedContent)))
                    return;

            if (serializedContent is vItemCollectionSerailizedContent)
            {
                vItemCollectionSerailizedContent content = serializedContent as vItemCollectionSerailizedContent;

                var position = new Vector3(float.Parse(content.position_x), float.Parse(content.position_y), float.Parse(content.position_z));
                var rotation = new Quaternion(float.Parse(content.rotation_x), float.Parse(content.rotation_y), float.Parse(content.rotation_z), float.Parse(content.rotation_w));

                monobehaviour.gameObject.transform.position = position;
                monobehaviour.gameObject.transform.rotation = rotation;

                var itemCollection = monobehaviour.gameObject.GetComponent<vItemCollection>();
                var trigger = itemCollection.GetComponent<BoxCollider>();

                if (content.wereItemsCollected) //|| (trigger.isTrigger && !trigger.enabled))
                {
                    //if(trigger.isTrigger)
                    itemCollection.items.Clear();
                    trigger.enabled = false;
                }

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
