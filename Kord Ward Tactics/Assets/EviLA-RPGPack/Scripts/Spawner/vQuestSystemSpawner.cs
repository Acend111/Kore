using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Invector;
using Invector.vCharacterController;

namespace EviLA.AddOns.RPGPack.Spawners
{

    [Serializable]
    public class vSpawnable
    {

        public GameObject prefab;
        public string instancePrefix;
        public int maxInstancesPerWave;
        public float maxSpawnTime;
        public float minSpawnTime;
        public float spawnRadius;
        public string deadAnimationState;
        [Range(0, 1)]
        public float normalizedTimeToInitiateDestroy;
        public float waitNSecondsAfterAnimToDestroy;
        public int spawnAtWave = 0;
        public int lastWave = 0;

        public List<Transform> spawnPoints;
        public Transform parentTransform;

        [NonSerialized]
        public UnityEvent OnSpawnObject;

    }

    [Serializable]
    public class vQuestSystemSpawner : MonoBehaviour
    {

        public int waveCount;
        public int waveInterval;
        public bool waitTillCurrentWaveDestroyed;

        public bool spawnOnEnterRegion;
        public bool spawnOnlyIfQuestIsInProgress;
        public bool destroySpawnedOnQuestFailure;
        public bool drawGizmos = true;

        public List<vSpawnable> prefabs = new List<vSpawnable>();

        [HideInInspector, SerializeField]
        public int currentWaveCount;
        [HideInInspector, SerializeField]
        public int currentWaveInstanceCount;

        public int questID = -1;

        [NonSerialized]
        public UnityEvent OnStartWave;
        [NonSerialized]
        public UnityEvent OnEndWave;

        private bool playerWithinArea;

        public void Awake()
        {
            currentWaveCount = 0;
            currentWaveInstanceCount = 0;
        }

        public void Start()
        {

            var instance = vQuestSystemManager.Instance;

            if (questID != -1)
                if (instance.GetParent(questID) != instance.ActiveQuest)
                    return;
                else
                    instance.AssignQuestSpawner(questID, this, spawnOnlyIfQuestIsInProgress, destroySpawnedOnQuestFailure);


            if (!spawnOnEnterRegion)
            {
                if (questID == -1 || (questID != -1 && instance.QuestAllowsSpawning(questID)))
                {
                    StartCoroutine(Spawn());
                }
            }

        }

        public void Respawn()
        {
            Start();
        }

        void OnTriggerEnter(Collider other)
        {

            var instance = vQuestSystemManager.Instance;

            playerWithinArea = true;

            if (questID != -1)
                // if (questID != vQuestSystem.Instance.ActiveQuest)
                // if (vQuestSystem.Instance.GetParent(questID) != vQuestSystem.Instance.ActiveQuest)
                //    return;
                // else
                instance.AssignQuestSpawner(questID, this, spawnOnlyIfQuestIsInProgress, destroySpawnedOnQuestFailure);

            if (other.tag == "Player" && spawnOnEnterRegion)
            {
                if (questID != -1 && !instance.QuestAllowsSpawning(questID))
                {
                    return;
                }
                if (!(currentWaveCount >= waveCount) && vGlobalObjectPool.Instance.IsPoolEmpty(this))
                    StartCoroutine(Spawn());
                else if (!vGlobalObjectPool.Instance.IsPoolEmpty(this))
                {
                    vGlobalObjectPool.Instance.SetObjectState(this, true);
                }
            }
        }

        void OnTriggerExit(Collider other)
        {

            playerWithinArea = false;


            if (other.tag == "Player")
            {

                var instance = vQuestSystemManager.Instance;

                if (instance.GetQuestState(questID) == vQuestState.Failed || instance.GetQuestState(questID) == vQuestState.NotStarted)
                {
                    ResetSpawner();
                    return;
                }
                vGlobalObjectPool.Instance.SetObjectState(this, false);
            }
        }

        public void ResetSpawner()
        {

            if (playerWithinArea)
                return;

            vGlobalObjectPool.Instance.DestroyPool(this);
            currentWaveCount = 0;

        }

        IEnumerator SpawnPrefab(vSpawnable spawnable)
        {

            for (int i = currentWaveInstanceCount; i < spawnable.maxInstancesPerWave; i++)
            {

                yield return new WaitForSeconds(UnityEngine.Random.Range(spawnable.minSpawnTime, spawnable.maxSpawnTime));
                CreatePrefabInstance(spawnable);
                ++currentWaveInstanceCount;

            }
        }

        public GameObject CreatePrefabInstance(vSpawnable spawnable)
        {

            Transform spawnPoint = spawnable.spawnPoints.Count > 0 ? spawnable.spawnPoints[UnityEngine.Random.Range(0, spawnable.spawnPoints.Count)] : transform;

            Vector3 position = spawnPoint.position;
            position.x = position.x + UnityEngine.Random.Range(0f, spawnable.spawnRadius);
            position.z = position.z + UnityEngine.Random.Range(0f, spawnable.spawnRadius);

            RaycastHit hitUp, hitDown;

            Ray rayUp = new Ray(position, Vector3.up);
            //Ray rayDown = new Ray(position , Vector3.down);

            LayerMask ignoreGroundAndRaycast = LayerMask.GetMask(LayerMask.LayerToName(vThirdPersonController.instance.groundLayer.value), "Ignore Raycast", "Transparent FX");

            if (Physics.Raycast(rayUp, out hitUp, Mathf.Infinity, ignoreGroundAndRaycast.value))
            {
                if (hitUp.collider != null)
                {
                    position.y = hitUp.point.y;
                    position = new Vector3(position.x, position.y, position.z);
                }

            }

            /*else if (Physics.Raycast(rayDown, out hitDown, Mathf.Infinity, ignoreGroundAndRaycast.value))
            {
                if (hitDown.collider != null)
                {
                    position.y = hitDown.point.y;
                    position = new Vector3(position.x, position.y, position.z);
                }
            }*/

            var overlapping = Physics.OverlapSphere(position, spawnable.spawnRadius, ignoreGroundAndRaycast.value).vToList();
            var overlappingObject = overlapping.Find(collider => !collider.isTrigger);

            if (overlappingObject != null)
            {
                int loopCount = 0, maxloopCount = 5;
                do
                {
                    if (loopCount == maxloopCount)
                        break;

                    if (Physics.Raycast(rayUp, out hitUp, Mathf.Infinity, ignoreGroundAndRaycast.value))
                    {
                        if (hitUp.collider != null)
                        {
                            position.y = hitUp.point.y;
                            position = new Vector3(position.x, position.y, position.z);
                        }

                    }
                    /*else if (Physics.Raycast(rayDown, out hitDown, Mathf.Infinity, ignoreGroundAndRaycast.value))
                    {
                        if (hitDown.collider != null)
                        {
                            position.y = hitDown.point.y;
                            position = new Vector3(position.x, position.y, position.z);
                        }
                    }*/

                    position.x = position.x + UnityEngine.Random.Range(0f, spawnable.spawnRadius);
                    position.z = position.z + UnityEngine.Random.Range(0f, spawnable.spawnRadius);
                    overlapping = Physics.OverlapSphere(position, spawnable.spawnRadius, ignoreGroundAndRaycast.value).vToList();
                    overlappingObject = overlapping.Find(collider => !collider.isTrigger);

                    ++loopCount;

                } while (overlappingObject != null);

            }

            var spawned = Instantiate(spawnable.prefab, position, spawnPoint.rotation, spawnable.parentTransform);

            spawned.tag = spawnable.prefab.tag;
            spawned.layer = spawnable.prefab.layer;

            var guid = Guid.NewGuid().ToString();

            spawned.name = spawned.name.Replace("(Clone)", "");
            spawned.name = spawnable.instancePrefix + "_" + spawned.name + "_" + guid;

            var character = spawned.GetComponent<vCharacter>();
            if (character != null)
            {

                character.onDead.AddListener((obj) =>
                {
                    StartCoroutine(vGlobalObjectPool.Instance.RemoveObjectFromPool(this, spawnable, spawned));
                });

            }

            spawned.SetActive(true);
            vGlobalObjectPool.Instance.AddObjectToPool(this, spawnable, spawned);

            return spawned;
        }

        public IEnumerator Spawn()
        {

            for (int i = currentWaveCount; i < waveCount; i++)
            {


                foreach (var spawnable in prefabs)
                {
                    if ((spawnable.spawnAtWave == 0 || spawnable.spawnAtWave <= (i + 1)) && (i != spawnable.lastWave || spawnable.lastWave == 0))
                        StartCoroutine(SpawnPrefab(spawnable));
                }

                yield return new WaitUntil(
                    () => { return !vGlobalObjectPool.Instance.IsPoolEmpty(this); }
                );

                ++currentWaveCount;

                if (waitTillCurrentWaveDestroyed)
                {
                    yield return new WaitUntil(() => vGlobalObjectPool.Instance.IsPoolEmpty(this));
                }

                currentWaveInstanceCount = 0;

                yield return new WaitForSeconds(waveInterval);
            }

        }
    }

    [Serializable]
    public class vGlobalObjectPool
    {

        [SerializeField]
        private Dictionary<vQuestSystemSpawner, Dictionary<vSpawnable, List<GameObject>>> spawnedPool = new Dictionary<vQuestSystemSpawner, Dictionary<vSpawnable, List<GameObject>>>();

        static vGlobalObjectPool instance = new vGlobalObjectPool();

        vGlobalObjectPool()
        {

        }

        public void LoadPool(Dictionary<vQuestSystemSpawner, Dictionary<vSpawnable, List<GameObject>>> pool)
        {
            DestroyAll();
            spawnedPool = pool;
        }

        public Dictionary<vQuestSystemSpawner, Dictionary<vSpawnable, List<GameObject>>> ObjectPool
        {
            get
            {
                return spawnedPool;
            }
        }

        public bool DestroyAll()
        {
            spawnedPool.Clear();
            return true;
        }

        public static vGlobalObjectPool Instance
        {
            get
            {
                return instance;
            }
        }

        public bool SetObjectState(vQuestSystemSpawner spawner, bool state)
        {
            try
            {
                foreach (var prefab in spawner.prefabs)
                {
                    var pooledItems = GetPoolForSpawner(spawner, prefab);
                    foreach (var obj in pooledItems)
                    {
                        if (obj.GetComponent<vMarkedForDeletion>() != null)
                        {
                            RemoveObjectFromPool(spawner, prefab, obj);
                            continue;
                        }
                        obj.SetActive(state);
                        obj.SetActiveChildren(state);

                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DestroyPool(vQuestSystemSpawner spawner)
        {
            try
            {
                foreach (var prefab in spawner.prefabs)
                {
                    var spawnedItems = GetPoolForSpawner(spawner, prefab);
                    foreach (var item in spawnedItems)
                    {
                        GameObject.Destroy(item);
                    }
                    spawnedItems.Clear();
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public List<GameObject> GetPoolForSpawner(vQuestSystemSpawner spawner, vSpawnable prefab)
        {

            Dictionary<vSpawnable, List<GameObject>> prefabDictionary;
            List<GameObject> spawnedItems;

            if (!spawnedPool.ContainsKey(spawner))
            {
                var prefabs = new Dictionary<vSpawnable, List<GameObject>>();
                spawnedPool.Add(spawner, prefabs);
                prefabDictionary = prefabs;
            }
            else
            {
                spawnedPool.TryGetValue(spawner, out prefabDictionary);
            }

            if (prefabDictionary.ContainsKey(prefab))
            {
                prefabDictionary.TryGetValue(prefab, out spawnedItems);
                return spawnedItems;
            }
            else
            {
                spawnedItems = new List<GameObject>();
                prefabDictionary.Add(prefab, spawnedItems);
                return spawnedItems;
            }


        }

        public bool IsPoolEmpty(vQuestSystemSpawner spawner)
        {

            foreach (var prefab in spawner.prefabs)
            {
                var spawnerPool = GetPoolForSpawner(spawner, prefab);
                if (spawnerPool.Count > 0)
                {
                    return false;
                }
            }
            return true;
        }

        public void AddObjectToPool(vQuestSystemSpawner spawner, vSpawnable prefab, GameObject obj)
        {

            var spawnerPool = GetPoolForSpawner(spawner, prefab);
            spawnerPool.Add(obj);
        }

        public IEnumerator RemoveObjectFromPool(vQuestSystemSpawner spawner, vSpawnable prefab, GameObject obj)
        {

            var animator = obj.GetComponent<Animator>();
            obj.AddComponent<vMarkedForDeletion>();

            var seconds = prefab.waitNSecondsAfterAnimToDestroy;

            yield return new WaitUntil(() =>
            {
                if (animator == null)
                    return true;

                var stateinfo = animator.GetCurrentAnimatorStateInfo(5);
                return stateinfo.IsName(prefab.deadAnimationState) && stateinfo.normalizedTime >= prefab.normalizedTimeToInitiateDestroy && !animator.IsInTransition(5);
            });

            yield return new WaitForSeconds(seconds);

            var spawnerPool = GetPoolForSpawner(spawner, prefab);
            spawnerPool.Remove(obj);
            GameObject.Destroy(obj);
            spawnerPool.RemoveAll(o => o == null);
            yield break;
        }
    }
}

