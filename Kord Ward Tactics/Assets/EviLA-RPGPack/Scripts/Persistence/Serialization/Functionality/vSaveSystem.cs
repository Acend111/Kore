
using UnityEngine;
using UnityEngine.SceneManagement;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

using Invector;
using Invector.vCamera;
using Invector.vItemManager;
using Invector.vCharacterController;

using EviLA.AddOns.RPGPack;
using EviLA.AddOns.RPGPack.MenuSystem;
using EviLA.AddOns.RPGPack.Experience.UI;
using EviLA.AddOns.RPGPack.Persistence.Serialization;

namespace EviLA.AddOns.RPGPack.Persistence
{

    public class vSaveSystem
    {

        private static vSaveSystem instance = new vSaveSystem();
        public bool loadingPlayer = false;
        public bool loadingObjects = false;

        private static bool firstTimeLoadComplete = false;

        public static bool IsLoading
        {
            get
            {
                return instance.loadingPlayer || instance.loadingObjects;
            }
        }

        vSaveSystem()
        {

        }

        public static vSaveSystem Instance
        {
            get
            {
                return instance;
            }
        }

        public static void LoadMainMenu()
        {
            //Parenting destroy on load objects is the best way to destroy them somewhere down the line. 
            //GC.Collect is called at level loader to do any garbage collection in any case to flush any 
            //held memory. 
            var go = new GameObject();
            go.hideFlags = HideFlags.HideInHierarchy;
            var list = GameObject.FindObjectsOfType<vDestroyAtTitleScreen>().vToList();
            list.ForEach(obj =>
            {
                obj.transform.parent = go.transform;
            });

            var pause = vThirdPersonController.instance.GetComponent<vPauseAction>();
            pause.PauseUnpause();

            // if loading from in game pause menu, detach persistence events before loading
            if (vThirdPersonController.instance != null)
            {
                var persistence = vThirdPersonController.instance.GetComponent<vPersistenceManager>();
                if (persistence != null)
                {
                    persistence.DetachFromScene();
                }
            }


            ResetInitialLoad();


            SceneManager.LoadScene(vQuestSystemLevelLoader.instance.mainMenuScene, LoadSceneMode.Single);
        }

        public static void ResetInitialLoad()
        {
            firstTimeLoadComplete = false;
        }

        public IEnumerator StartLoading(List<MonoBehaviour> markedToSave, bool noItemManager = false, bool noQuestManager = false)
        {
            yield return new WaitUntil(() => vThirdPersonController.instance != null);

            var itemManager = vThirdPersonController.instance.GetComponent<vItemManager>();
            var questManager = vThirdPersonController.instance.GetComponent<vQuestManager>();

            if (!noItemManager)
                yield return new WaitUntil(() => itemManager != null && itemManager.items.Count >= itemManager.startItems.Count);
            if (!noQuestManager)
                yield return new WaitUntil(() => questManager != null && questManager.quests.Count >= questManager.startQuests.Count);

            Load(markedToSave);

        }

        public static void LoadGame(bool loadMostRecent = false)
        {
            try
            {
                var gameID = vQuestSystemLevelLoader.instance.CurrentGameID;
                var slotID = vQuestSystemLevelLoader.instance.CurrentCheckPointID;

                string pathPlayer = Application.persistentDataPath + "/Data/" + gameID + "/" + slotID + "/SaveData/Player/SavePlayer.tmw";

                BinaryFormatter formatter = new BinaryFormatter();
                FileStream filestream;

                if (File.Exists(pathPlayer))
                {
                    filestream = new FileStream(pathPlayer, FileMode.Open, FileAccess.Read);

                    using (CryptoStream cryptoStream = QuestSystemSerializationHelper.GetCryptoStream(filestream, CryptoStreamMode.Read))
                    {
                        var playerData = Serializer.DeserializeContentOnly(formatter, cryptoStream);
                        filestream.Close();

                        playerData.ForEach(player =>
                        {
                            //This list will always only have one value
                            var playerdes = player as PlayerSerializedContent;
                            SceneManager.LoadSceneAsync(playerdes.activeSceneName, LoadSceneMode.Single);
                        });
                    }
                }
                else
                {
                    SceneManager.LoadSceneAsync(vQuestSystemLevelLoader.instance.startScene, LoadSceneMode.Single);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        public IEnumerator Save(List<MonoBehaviour> behaviors)
        {

            lock (this)
            {
                yield return new WaitForEndOfFrame();
                SaveBehaviours(behaviors);
            }
        }

        void SaveBehaviours(List<MonoBehaviour> behaviours)
        {
            var parentPath = Application.persistentDataPath + "/Data/";

            if (!File.Exists(parentPath))
            {
                var parent = new DirectoryInfo(parentPath);
                parent.Create();
            }

            var gameID = vQuestSystemLevelLoader.instance.CurrentGameID;

            var slotID = 0;

            if (gameID != -1)
            {
                try
                {
                    var files = Directory.GetDirectories(parentPath + gameID + "/");
                    if (files != null)
                    {
                        var list = files.vToList();
                        list.RemoveAll(file => file.ToLower().Contains("thumb"));
                        slotID = list.Count;
                    }
                }
                catch (Exception e)
                {
                    var files = Directory.GetDirectories(parentPath);
                    if (files != null && files.vToList().Count != 0)
                        throw new Exception("Error loading game : " + e.Message);
                }
            }
            else
            {
                var list = Directory.GetDirectories(parentPath).vToList();
                list.RemoveAll(file => file.ToLower().Contains("thumb"));
                gameID = list.Count;
            }

            vQuestSystemLevelLoader.instance.UpdateGameID(gameID);
            vQuestSystemLevelLoader.instance.UpdateCheckPointID(slotID);

            behaviours.RemoveAll(behaviour => behaviour == null);

            var playerBehaviours = new List<MonoBehaviour>();

            playerBehaviours.Add(vThirdPersonController.instance);

            behaviours.RemoveAll(behaviour => behaviour.GetComponent<vThirdPersonController>() != null);

            string pathObjects = parentPath + gameID + "/" + slotID + "/SaveData/Levels/" + SceneManager.GetActiveScene().name + ".tmw";
            string pathPlayer = parentPath + gameID + "/" + slotID + "/SaveData/Player/SavePlayer.tmw";

            string pathImgCheckpoint = parentPath + gameID + "/" + slotID + "/SaveData/Thumb/" + DateTime.UtcNow.ToFileTime() + ".tmw";

            string pathImgGame = parentPath + "/Thumb/" + gameID + "_0_" + "_.tmw";


            BinaryFormatter formatter = new BinaryFormatter();


            if (!File.Exists(pathPlayer))
            {
                FileInfo playerFile = new System.IO.FileInfo(pathPlayer);
                playerFile.Directory.Create();
            }
            else
            {
                File.Delete(pathPlayer);
            }

            using (CryptoStream cryptoStreamPlayer = QuestSystemSerializationHelper.GetCryptoStream(new FileStream(pathPlayer, FileMode.Create, FileAccess.Write), CryptoStreamMode.Write))
            {
                Serializer.Serialize(playerBehaviours, formatter, cryptoStreamPlayer);
                cryptoStreamPlayer.Close();
            }


            if (behaviours.Count == 0)
            {
                return;
            }


            if (!File.Exists(pathObjects))
            {

                FileInfo dataFile = new System.IO.FileInfo(pathObjects);
                dataFile.Directory.Create();

                if (slotID > 0)
                {

                    string pathPrevSlotLevels = parentPath + gameID + "/" + (slotID - 1) + "/SaveData/Levels/";
                    string pathNewSlotLevels = parentPath + gameID + "/" + slotID + "/SaveData/Levels/";
                    CopySlotToNewSlot(pathPrevSlotLevels, pathNewSlotLevels);
                }
            }
            else
            {
                File.Delete(pathObjects);
            }
            using (CryptoStream cryptoStream = QuestSystemSerializationHelper.GetCryptoStream(new FileStream(pathObjects, FileMode.Create, FileAccess.Write), CryptoStreamMode.Write))
            {
                Serializer.Serialize(behaviours, formatter, cryptoStream);
                cryptoStream.Close();
            }

            if (!File.Exists(pathImgCheckpoint))
            {
                FileInfo thumbnailPath = new System.IO.FileInfo(pathImgCheckpoint);
                thumbnailPath.Directory.Create();
            }

            if (!File.Exists(pathImgGame))
            {
                FileInfo thumbnailPath = new System.IO.FileInfo(pathImgGame);
                thumbnailPath.Directory.Create();
            }

            this.Screenshot(pathImgCheckpoint);
            this.Screenshot(pathImgGame);


        }

        void Load(List<MonoBehaviour> behaviours)
        {
            if (vQuestSystemLevelLoader.instance == null)
            {
                loadingPlayer = false;
                loadingObjects = false;
                var player = vThirdPersonController.instance;
                if (player != null)
                    player.GetComponent<vPersistenceManager>().JustLoaded = false;
                return;
            }

            var gameID = vQuestSystemLevelLoader.instance.CurrentGameID;
            var slotID = vQuestSystemLevelLoader.instance.CurrentCheckPointID;

            string pathObjects = Application.persistentDataPath + "/Data/" + gameID + "/" + slotID + "/SaveData/Levels/" + SceneManager.GetActiveScene().name + ".tmw";
            string pathPlayer = Application.persistentDataPath + "/Data/" + gameID + "/" + slotID + "/SaveData/Player/SavePlayer.tmw";
            behaviours.RemoveAll(behaviour => behaviour == null);

            var playerBehaviours = new List<MonoBehaviour>();

            playerBehaviours.Add(vThirdPersonController.instance);
            behaviours.RemoveAll(behaviour => behaviour.GetComponent<vThirdPersonController>() != null);

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream filestream;
            //
            //			if (firstTimeLoadComplete) {
            //				playerBehaviours.RemoveAll (behaviour => behaviour.GetType () == typeof(vLevelManager) ||
            //				behaviour.GetType () == typeof(vItemManager));
            //			}

            if (File.Exists(pathPlayer) && !firstTimeLoadComplete)
            {
                filestream = new FileStream(pathPlayer, FileMode.Open, FileAccess.Read);
                using (CryptoStream cryptoStream = QuestSystemSerializationHelper.GetCryptoStream(filestream, CryptoStreamMode.Read))
                {
                    Serializer.DeserializeInto(ref playerBehaviours, formatter, cryptoStream, SerializationMode.List);
                    filestream.Close();
                }
            }

            if (behaviours.Count > 0)
                if (File.Exists(pathObjects))
                {
                    filestream = new FileStream(pathObjects, FileMode.Open, FileAccess.Read);
                    using (CryptoStream cryptoStream = QuestSystemSerializationHelper.GetCryptoStream(filestream, CryptoStreamMode.Read))
                    {
                        Serializer.DeserializeInto(ref behaviours, formatter, cryptoStream, SerializationMode.List);
                        filestream.Close();
                    }
                }

            loadingPlayer = false;
            loadingObjects = false;

            firstTimeLoadComplete = true;

            ForceStartPlayer();
        }

        void ForceStartPlayer()
        {
            //v1.2.2 / v2.3.2 No longer required to forcefully instantiate player or experience slider
            //vThirdPersonController.instance.Init();
            //vThirdPersonCamera.instance.Init();
            //vExperienceHUD.instance.UpdateXPSlider();

            vQuestSystemManager.Instance.ForceStartQuests();
            var persistence = vThirdPersonController.instance.GetComponent<vPersistenceManager>();
            persistence.onPlayerLoaded.Invoke();

        }


        public void Screenshot(string path)
        {
            Camera currentCamera = Camera.main;
            int tw = 400; //thumb width
            int th = 400; //thumb height
            RenderTexture rt = new RenderTexture(tw, th, 24, RenderTextureFormat.ARGB32);
            rt.antiAliasing = 4;

            currentCamera.targetTexture = rt;

            currentCamera.Render();//

            //Create the blank texture container
            Texture2D thumb = new Texture2D(tw, th, TextureFormat.RGB24, false);

            //Assign rt as the main render texture, so everything is drawn at the higher resolution
            RenderTexture.active = rt;

            //Read the current render into the texture container, thumb
            thumb.ReadPixels(new Rect(0, 0, tw, th), 0, 0, false);

            byte[] bytes = thumb.EncodeToPNG(); // 90 for jpeg
            GameObject.Destroy(thumb);

            // For testing purposes, also write to a file in the project folder
            File.WriteAllBytes(path, bytes);

            //--Clean up--
            RenderTexture.active = null;

            currentCamera.targetTexture = null;

            rt.DiscardContents();
        }

        public void CopySlotToNewSlot(string source, string dest)
        {
            foreach (string dir in Directory.GetDirectories(source, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dir.Replace(source, dest));

            foreach (string file in Directory.GetFiles(source, "*.*",
                SearchOption.AllDirectories))
                File.Copy(file, file.Replace(source, dest), true);

        }
    }
}
