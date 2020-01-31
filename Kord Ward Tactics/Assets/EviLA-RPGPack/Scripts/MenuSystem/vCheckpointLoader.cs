using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Invector;
using Invector.vCharacterController;
using EviLA.AddOns.RPGPack.Persistence;
using EviLA.AddOns.RPGPack.Util;

namespace EviLA.AddOns.RPGPack.MenuSystem
{

    [Serializable]
    public class vCheckpointLoader : vMonoBehaviour
    {

        public GameObject content;
        public vCheckPointSlot gamePrefab;
        public Image checkpointImage;

        private List<Texture2D> thumbnails = new List<Texture2D>();

        private int tw = 400, th = 400;

        public void OnEnable()
        {

            var gameID = vQuestSystemLevelLoader.instance.CurrentGameID;

            string directory = Application.persistentDataPath + "/Data/" + gameID + "/";

            var filesArray = Directory.GetDirectories(directory);
            Array.Sort(filesArray, new AlphanumComparatorFast());
            var files = filesArray.vToList();


            files.ForEach(file =>
            {

                var fileInfo = new FileInfo(file);

                var slot = GameObject.Instantiate(gamePrefab, content.transform);
                slot.checkpointID = files.IndexOf(file);

                var button = slot.GetComponent<Button>();
                var text = slot.GetComponentInChildren<Text>();
                text.text = fileInfo.CreationTimeUtc.ToString();

                var mainMenu = button.GetComponentInParent<vMainMenu>();

                button.onClick.AddListener(() =>
                {

                    // if loading from in game pause menu, detach persistence events before loading
                    if (vThirdPersonController.instance != null)
                    {
                        var persistence = vThirdPersonController.instance.GetComponent<vPersistenceManager>();
                        if (persistence != null)
                        {
                            persistence.DetachFromScene();
                        }
                    }

                    var checkpoint = button.GetComponent<vCheckPointSlot>();
                    vQuestSystemLevelLoader.instance.UpdateCheckPointID(checkpoint.checkpointID);
                    if (mainMenu != null)
                        mainMenu.ResumeGame();
                    vQuestSystemLevelLoader.instance.LoadGame(true);
                });

            });

            LoadThumbnails();
        }

        public void LoadThumbnails()
        {

            var dirsArray = Directory.GetDirectories(Application.persistentDataPath + "/Data/" + vQuestSystemLevelLoader.instance.CurrentGameID + "/");
            Array.Sort(dirsArray, new AlphanumComparatorFast());
            var dirs = dirsArray.vToList();

            dirs.ForEach(dir =>
            {
                var files = Directory.GetFiles(dir + "/SaveData/Thumb/", "*.tmw");
                if (files != null && files.Length > 0)
                {
                    var file = files[0];
                    byte[] bytes = File.ReadAllBytes(file);
                    Texture2D texture = new Texture2D(tw, th, TextureFormat.RGB24, false);
                    texture.filterMode = FilterMode.Trilinear;
                    texture.LoadImage(bytes);
                    thumbnails.Add(texture);
                }
            });
        }

        public void ShowThumbnail(vCheckPointSlot slot)
        {
            var texture = thumbnails[slot.checkpointID];
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, tw, th), new Vector2(0.5f, 0.0f), 1.0f);
            checkpointImage.sprite = sprite;
            var color = checkpointImage.color;
            color.a = 255;
            checkpointImage.color = color;
        }

        public void HideThumbnail()
        {
            var color = checkpointImage.color;
            color.a = 0;
            checkpointImage.color = color;
        }

        public void OnDisable()
        {
            foreach (Transform child in content.transform)
            {
                Destroy(child.gameObject);
            }

            thumbnails.Clear();

        }
    }
}