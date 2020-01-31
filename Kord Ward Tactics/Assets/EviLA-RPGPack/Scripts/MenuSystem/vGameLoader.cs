using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EviLA.AddOns.RPGPack.Persistence;
using EviLA.AddOns.RPGPack.Util;
using Invector;

namespace EviLA.AddOns.RPGPack.MenuSystem
{

    [Serializable]
    public class vGameLoader : vMonoBehaviour
    {

        public GameObject content;
        public GameObject checkpointPanel;
        public vSaveSlot gamePrefab;
        public Image DisplayGameSlotImage;
        public Text noSavedGamesText;

        private List<Texture2D> thumbnails = new List<Texture2D>();

        private int tw = 400, th = 400;


        public void OnEnable()
        {
            noSavedGamesText.gameObject.SetActive(false);

            LoadThumbnails();

            string directory = Application.persistentDataPath + "/Data/";

            var filesArray = Directory.GetDirectories(directory);
            Array.Sort(filesArray, new AlphanumComparatorFast());
            var files = filesArray.vToList();
            files.RemoveAll(file => file.ToLower().Contains("thumb"));



            files.ForEach(file =>
            {
                var slot = GameObject.Instantiate(gamePrefab, content.transform);
                slot.gameID = files.IndexOf(file);
                var button = slot.GetComponent<Button>();

                button.onClick.AddListener(() =>
                {
                    var saveSlot = button.GetComponent<vSaveSlot>();
                    vQuestSystemLevelLoader.instance.UpdateGameID(saveSlot.gameID);
                    transform.gameObject.SetActive(false);
                    checkpointPanel.SetActive(true);
                });

                slot.gameID = files.IndexOf(file);
                var text = slot.GetComponentInChildren<Text>();
                text.text = "Game " + (slot.gameID + 1);
                ShowThumbnail(slot);
            });
        }

        public void OnDisable()
        {
            noSavedGamesText.gameObject.SetActive(false);

            thumbnails.Clear();

            foreach (Transform child in content.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public void LoadThumbnails()
        {
            try
            {

                var filesArray = Directory.GetFiles(Application.persistentDataPath + "/Data/Thumb/", "*_0_*.tmw");
                Array.Sort(filesArray, new AlphanumComparatorFast());
                var files = filesArray.vToList();

                files.ForEach(file =>
                {
                    byte[] bytes = File.ReadAllBytes(file);
                    Texture2D texture = new Texture2D(tw, th, TextureFormat.RGB24, false);
                    texture.filterMode = FilterMode.Trilinear;
                    texture.LoadImage(bytes);
                    thumbnails.Add(texture);
                });

            }
            catch (DirectoryNotFoundException e)
            {
                noSavedGamesText.gameObject.SetActive(true);
            }
        }

        public void ShowThumbnail(vSaveSlot slot)
        {
            var image = slot.GetComponent<Image>();
            var texture = thumbnails[slot.gameID];
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, tw, th), new Vector2(0.5f, 0.0f), 1.0f);
            image.sprite = sprite;
        }
    }
}