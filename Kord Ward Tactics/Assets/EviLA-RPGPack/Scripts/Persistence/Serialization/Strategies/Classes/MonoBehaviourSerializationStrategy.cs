using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using UnityEngine;
using Invector;

namespace EviLA.AddOns.RPGPack.Persistence.Serialization
{
    public class MonoBehaviorSerializationStrategyException : Exception
    {
        public MonoBehaviorSerializationStrategyException(string message) : base(message) {  }
    }

    public partial class MonoBehaviourSerializationStrategy : ISerializationStrategy
    {
        
        public void Serialize<T>(T data, IFormatter formatter, CryptoStream stream) where T : SerializedContent
        {
             try
            {
                formatter.Serialize(stream, data);
            }
            catch (SerializationException e)
            {
                throw new MonoBehaviorSerializationStrategyException("Unable to Serialize Monobehavior");
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
            SerializedContent content = (SerializedContent) formatter.Deserialize(stream);

            var behaviour = data;

            if (behaviour == null)
                return;

            var _go = behaviour.gameObject;

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

                if (behaviour is vCanSaveYou && (behaviour as vCanSaveYou).ObjectHasLegacyAnimations)
                {
                    var save = behaviour as vCanSaveYou;
                    var animation = _go.GetComponentInParent<Animation>();

                    if (animation != null)
                    {
                        foreach (AnimationState state in animation)
                        {
                            var stateValues = content.animationInfo.Find(anima => anima.name.Equals(state.name));
                            if (stateValues != null)
                            {
                                state.normalizedSpeed = stateValues.normalizedSpeed;
                                state.normalizedTime = stateValues.normalizedTime;
                                if (stateValues.normalizedTime == 1.0f)
                                {
                                    save.finishedPlaying = true;
                                }
                            }
                        }

                        if(save.finishedPlaying)
                            animation.Play();
                    }
                }

                var animator = _go.GetComponent<Animator>();
                if (animator != null)
                {
                    for (int i = 0; i < animator.layerCount; i++)
                    {
                        var info = content.animatorInfo.Find(anim => anim.layer == i);
                        if( info != null ) {
                            animator.SetLayerWeight(i, info.layerWeight);
                            animator.Play(info.nameHash, i, info.currentTimeOfAnimation);
                            break;
                        }
                    }
                }

                HandleDeserialized(ref behaviour, ref content);

            }
        }

        public void DeserializeMultiple<T>(ref List<T> data, IFormatter formatter, CryptoStream stream) where T : MonoBehaviour
        {
           var contentList = formatter.Deserialize(stream);
            if (contentList.GetType().Equals(typeof(List<SerializedContent>))) {

                var contents = contentList as List<SerializedContent>;

                contents.RemoveAll(content => !content.BelongsToActiveScene && !content.GetType().Equals(typeof(PlayerSerializedContent)));

                List<T> reference = data.vCopy();
                contents.ForEach(content => {

                    var behaviour = reference.Find(obj => obj.name.Equals(content.name) && 
                                                        ( obj.transform.parent == null || ( obj.transform.parent.name.Equals(content.parentName) ) ) );
                   
                    var _go = behaviour.gameObject;

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

                        var animator = _go.GetComponent<Animator>();
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

                        

                        if (behaviour is vCanSaveYou && (behaviour as vCanSaveYou).ObjectHasLegacyAnimations )
                        {
                            var save = behaviour as vCanSaveYou;
                            var animation = _go.GetComponentInParent<Animation>();

                            if (animation != null)
                            { 
                                foreach (AnimationState state in animation)
                                {
                                    var stateValues = content.animationInfo.Find(anima => anima.name.Equals(state.name));
                                    if ( stateValues != null )
                                    {
                                        state.normalizedSpeed = stateValues.normalizedSpeed;
                                        state.normalizedTime = stateValues.normalizedTime;
                                        if (stateValues.normalizedTime == 1.0f) {
                                            save.finishedPlaying = true;
                                        }
                                    }
                                }

                                if (save.finishedPlaying)
                                    animation.Play();

                            }
                        }

                        _go.transform.position = position;
                        _go.transform.rotation = rotation;

                        HandleDeserialized(ref behaviour, ref content);

                    }

                });

                data = reference;
            }
        }

        private void HandleDeserialized<T, X>(ref X data, ref T content) where T : SerializedContent
                                                                where X : MonoBehaviour
        {
            ISerializationStrategy strategy = null;

            if (!content.BelongsToActiveScene)
                if (!content.GetType().Equals(typeof(PlayerSerializedContent)))
                    return;

            strategy = Serializer.GetStrategyBySerializedContent(content);
            strategy.HandleDeserializedInstance(ref data, ref content);
        }

        public SerializedContent GetSerializableContent<T>(T mono) where T : MonoBehaviour
        {
            var go = mono.gameObject;

            var data = new SerializedContent();

            data = GetSerializedContentByBehaviourGameObject(mono);

            data.active = go.gameObject.activeSelf;
            data.name = go.name;

            data.parentName = go.transform.parent != null ? go.transform.parent.name : "";

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

            if (mono is vCanSaveYou && (mono as vCanSaveYou).ObjectHasLegacyAnimations)
            {
                var save = mono as vCanSaveYou;
                var animation = go.GetComponentInParent<Animation>();

                foreach (AnimationState state in animation)
                {
                    data.animationInfo.Add(new AnimationStateInformation()
                    {
                        name = state.name,
                        normalizedTime = save.finishedPlaying ? 1.0f : state.normalizedTime,
                        normalizedSpeed = state.normalizedSpeed
                    });
                }
            }

            return data;
        }

        public List<SerializedContent> GetSerializableContent<T>(List<T> list) where T : MonoBehaviour
        {
            List<SerializedContent> dataSet = new List<SerializedContent>();

            list.ForEach(mono =>
            {
                dataSet.Add(GetSerializableContent(mono));
            });

            return dataSet;
        }
        
    }

}
