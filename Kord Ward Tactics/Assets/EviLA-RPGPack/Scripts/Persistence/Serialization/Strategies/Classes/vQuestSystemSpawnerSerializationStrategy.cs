using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using UnityEngine;

using Invector;
using EviLA.AddOns.RPGPack.Spawners;

namespace EviLA.AddOns.RPGPack.Persistence.Serialization
{

    public class vQuestSystemSpawnerSerializationStrategyException : Exception
    {
        public vQuestSystemSpawnerSerializationStrategyException(string message) : base(message) { }
    }

    public class vQuestSystemSpawnerSerializationStrategy : ISerializationStrategy
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
                throw new MonoBehaviorSerializationStrategyException("Unable to Serialize Monobehavior");
            }
        }

        public void DeserializeSingle<T>(ref T data, IFormatter formatter, CryptoStream stream) where T : MonoBehaviour
        {
            QuestSystemSpawnerSerializedContent content = ( (SerializedContent) formatter.Deserialize(stream) ) as QuestSystemSpawnerSerializedContent;
            
            var spawner = data.GetComponent<vQuestSystemSpawner>();

            HandleDeserializedInstance(ref spawner, ref content);

        }

        public void DeserializeMultiple<T>(ref List<T> data, IFormatter formatter, CryptoStream stream) where T : MonoBehaviour
        {
            var contents = ((List<SerializedContent>)formatter.Deserialize(stream)).Cast<QuestSystemSpawnerSerializedContent>().ToList();
            List<T> copied = data.vCopy<T>();

            contents.ForEach(content => {

                var mono = copied.Find(obj => obj.name == content.name);
                HandleDeserializedInstance(ref mono, ref content);
            });

            data = copied.vCopy<T>();
        }


        public SerializedContent GetSerializableContent<T>(T mono) where T : MonoBehaviour
        {                               

            var go = mono.gameObject;

            var data = new SerializedContent();
            var finalData = new SerializedContent();
            var spawner = mono.GetComponent<vQuestSystemSpawner>();


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

            var questSpawnerData = new QuestSystemSpawnerSerializedContent(data);

            questSpawnerData.waveCount = spawner.waveCount;
            questSpawnerData.waveInterval = spawner.waveInterval;
            questSpawnerData.waitTillCurrentWaveDestroyed = spawner.waitTillCurrentWaveDestroyed;
            questSpawnerData.spawnOnEnterRegion = spawner.spawnOnEnterRegion;
            questSpawnerData.spawnOnlyIfQuestIsInProgress = spawner.spawnOnlyIfQuestIsInProgress;
            questSpawnerData.destroySpawnedOnQuestFailure = spawner.destroySpawnedOnQuestFailure;
            questSpawnerData.drawGizmos = spawner.drawGizmos;
            questSpawnerData.currentWaveCount = spawner.currentWaveCount;
            questSpawnerData.currentWaveInstanceCount = spawner.currentWaveInstanceCount;
            questSpawnerData.questID = spawner.questID;
            
            spawner.prefabs.ForEach(prefab => {

                var pooledObjectsOfSpawner = vGlobalObjectPool.Instance.GetPoolForSpawner(spawner, prefab);

                if (pooledObjectsOfSpawner != null && pooledObjectsOfSpawner.Count > 0)
                {
                    List<List<SerializedContent>> prefabPooledObjects = new List<List<SerializedContent>>();
                    

                    pooledObjectsOfSpawner.ForEach(obj => {

                        if (obj == null)
                        {
                            pooledObjectsOfSpawner.Remove(obj);
                            return;
                        }

                        var behaviours = obj.GetComponents<MonoBehaviour>().vToList();

                        if (behaviours != null && behaviours.Count > 0)
                        {
                            List<SerializedContent> serializedBehaviors = new List<SerializedContent>();

                            behaviours.ForEach(behaviour => {

                                ISerializationStrategy strategy = Serializer.GetStrategyByType(behaviour.GetType().Name);
                                serializedBehaviors.Add(strategy.GetSerializableContent(behaviour));

                            });

                            if (serializedBehaviors.Count > 0)
                            {
                                prefabPooledObjects.Add(serializedBehaviors);
                            }
                        }

                    });

                    if (prefabPooledObjects.Count > 0) {
                        questSpawnerData.pooledSpawners.Add(prefab.instancePrefix);
                        questSpawnerData.pooledObjects.Add(prefabPooledObjects);
                    }
                }

            });

            finalData = questSpawnerData;

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

            var _go = monobehaviour.gameObject;
            var spawner = _go.GetComponent<vQuestSystemSpawner>();

            if (_go != null)
            {
                var content = serializedContent as QuestSystemSpawnerSerializedContent;
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

                spawner.waveCount = content.waveCount;
                spawner.waveInterval = content.waveInterval;
                spawner.waitTillCurrentWaveDestroyed = content.waitTillCurrentWaveDestroyed;
                spawner.spawnOnEnterRegion = content.spawnOnEnterRegion;
                spawner.spawnOnlyIfQuestIsInProgress = content.spawnOnlyIfQuestIsInProgress;
                spawner.destroySpawnedOnQuestFailure = content.destroySpawnedOnQuestFailure;
                spawner.drawGizmos = content.drawGizmos;
                spawner.currentWaveCount = content.currentWaveCount;
                spawner.currentWaveInstanceCount = content.currentWaveInstanceCount;
                spawner.questID = content.questID;

                vGlobalObjectPool.Instance.DestroyPool(spawner);
                spawner.prefabs.ForEach(prefab =>
                {
                    var pooledSpawnableID = content.pooledSpawners.Find(pooledSpawnable => pooledSpawnable.Equals(prefab.instancePrefix));
                    int index = -1;

                    if (pooledSpawnableID != null)
                        index = content.pooledSpawners.FindIndex(spawnableID => spawnableID.Equals(pooledSpawnableID));

                    if (index != -1)
                    {

                        var instance = spawner.CreatePrefabInstance(prefab);
                        var components = instance.GetComponents<MonoBehaviour>().vToList();

                        content.pooledObjects[index].ForEach(serializedMonos => {

                            serializedMonos.ForEach(mono => {

                                int indexOfMono = serializedMonos.IndexOf(mono);
                                var component = components[indexOfMono];

                                ISerializationStrategy strategy = Serializer.GetStrategyByType(component.GetType().Name);

                                strategy.HandleDeserializedInstance(ref component, ref mono);

                                if (component.gameObject.transform.position.Equals(Vector3.zero) || component.gameObject.transform.rotation.Equals(Quaternion.identity))
                                {

                                    Vector3 goPos = new Vector3(float.Parse(mono.position_x), float.Parse(mono.position_y), float.Parse(mono.position_z));
                                    Quaternion goRot = new Quaternion(float.Parse(mono.rotation_x), float.Parse(mono.rotation_y), float.Parse(mono.rotation_z), float.Parse(mono.rotation_z));

                                    component.gameObject.transform.position = goPos;
                                    component.gameObject.transform.rotation = goRot;

                                }

                                components[indexOfMono] = component;

                            });

                            instance.SetActive(false);
                            vGlobalObjectPool.Instance.AddObjectToPool(spawner, prefab, instance);

                        });
                    }

                });

                if (spawner.questID != -1)
                    vQuestSystemManager.Instance.AssignQuestSpawner(spawner.questID, spawner, spawner.spawnOnlyIfQuestIsInProgress, spawner.destroySpawnedOnQuestFailure);
                spawner.Start();
            }
        }
    }
}
