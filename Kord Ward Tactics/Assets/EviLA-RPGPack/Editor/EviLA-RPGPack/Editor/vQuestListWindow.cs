using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
namespace EviLA.AddOns.RPGPack
{
    public class vQuestListWindow : EditorWindow
    {
        public static vQuestListWindow Instance;
        public vQuestListData questList;
        [SerializeField]
        protected GUISkin skin;
        protected SerializedObject serializedObject;
        protected vQuest addQuest;
        protected vQuestDrawer addQuestDrawer;
        protected vQuestDrawer currentQuestDrawer;
        protected bool inAddQuest;
        protected bool openAttributeList;
        protected bool inCreateAttribute;
        protected string attributeName;
        protected int indexSelected;
        protected Vector2 scroolView;
        protected Vector2 attributesScroll;

        public static void CreateWindow(vQuestListData questList)
        {
            vQuestListWindow window = (vQuestListWindow)EditorWindow.GetWindow(typeof(vQuestListWindow), false, "QuestList Editor");
            Instance = window;
            window.questList = questList;
            window.skin = Resources.Load("skin") as GUISkin;
            Instance.Init();
        }

        public static void CreateWindow(vQuestListData questList, int firtItemSelected)
        {
            vQuestListWindow window = (vQuestListWindow)EditorWindow.GetWindow(typeof(vQuestListWindow), false, "QuestList Editor");
            Instance = window;
            window.questList = questList;
            window.skin = Resources.Load("skin") as GUISkin;
            Instance.Init(firtItemSelected);
        }

        public static void CreateWindow(vQuestListData questList, bool floatingWindow)
        {
            vQuestListWindow window = (vQuestListWindow)EditorWindow.GetWindow(typeof(vQuestListWindow), floatingWindow, "QuestList Editor");
            Instance = window;
            window.questList = questList;
            window.skin = Resources.Load("skin") as GUISkin;
        }

        protected virtual void Init()
        {
            serializedObject = new SerializedObject(questList);
            var subAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(questList));
            skin = Resources.Load("skin") as GUISkin;
            if (subAssets.Length > 1)
            {
                for (int i = subAssets.Length - 1; i >= 0; i--)
                {
                    var quest = subAssets[i] as vQuest;

                    if (quest && !questList.quests.Contains(quest))
                    {
                        quest.id = GetUniqueID();
                        questList.quests.Add(quest);
                    }
                }
                EditorUtility.SetDirty(questList);
                OrderByID();
            }
            questList.inEdition = true;
            this.Show();
        }

        protected virtual void Init(int firstQuestSelected)
        {
            Init();
            SetCurrentSelectedQuest(firstQuestSelected);
        }

        public virtual void OnGUI()
        {
            if (skin) GUI.skin = skin;

            scroolView = GUILayout.BeginScrollView(scroolView, GUILayout.ExpandWidth(true));

            GUILayout.BeginVertical("Quest List", "window");
            GUILayout.Space(30);
            GUILayout.BeginVertical("box");

            GUI.enabled = !Application.isPlaying;
            questList = EditorGUILayout.ObjectField("QuestListData", questList, typeof(vQuestListData), false) as vQuestListData;

            if (serializedObject == null && questList != null)
            {
                serializedObject = new SerializedObject(questList);
            }
            else if (questList == null)
            {
                GUILayout.EndVertical();
                return;
            }

            serializedObject.Update();

            if (!inAddQuest && GUILayout.Button("Create New Quest"))
            {
                addQuest = ScriptableObject.CreateInstance<vQuest>();
                addQuest.name = "New Quest";

                currentQuestDrawer = null;
                inAddQuest = true;
            }
            if (inAddQuest)
            {
                DrawAddQuest();
            }
            if (GUILayout.Button("Open QuestEnums Editor"))
            {
                vQuestEnumsWindow.CreateWindow();
            }
            GUILayout.Space(10);
            GUILayout.EndVertical();

            GUILayout.Box(questList.quests.Count.ToString("00") + " Quests");
            for (int i = 0; i < questList.quests.Count; i++)
            {
                if (questList.quests[i] != null)
                {
                    Color color = GUI.color;
                    GUI.color = currentQuestDrawer != null && currentQuestDrawer.quest == questList.quests[i] ? Color.green : color;
                    GUILayout.BeginVertical("box");
                    GUI.color = color;
                    GUILayout.BeginHorizontal();
                    var texture = questList.quests[i].iconTexture;
                    var name = " ID " + questList.quests[i].id.ToString("00") + "\n - " + questList.quests[i].name + "\n - " + questList.quests[i].type.ToString();
                    var content = new GUIContent(name, texture, currentQuestDrawer != null && currentQuestDrawer.quest == questList.quests[i] ? "Click to Close" : "Click to Open");
                    GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
                    GUI.skin.box.alignment = TextAnchor.UpperLeft;
                    GUI.skin.box.fontStyle = FontStyle.Italic;

                    GUI.skin.box.fontSize = 11;

                    if (GUILayout.Button(content, "box", GUILayout.Height(50), GUILayout.MinWidth(50)))
                    {
                        GUI.FocusControl("clearFocus");
                        scroolView.y = 1 + i * 60;
                        currentQuestDrawer = currentQuestDrawer != null ? currentQuestDrawer.quest == questList.quests[i] ? null : new vQuestDrawer(questList.quests[i]) : new vQuestDrawer(questList.quests[i]);
                    }
                    EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

                    GUI.skin.box = boxStyle;
                    var duplicateImage = Resources.Load("duplicate") as Texture;
                    if (GUILayout.Button(new GUIContent("", duplicateImage, "Duplicate Quest"), GUILayout.MaxWidth(45), GUILayout.Height(45)))
                    {
                        if (EditorUtility.DisplayDialog("Duplicate the " + questList.quests[i].name,
                        "Are you sure you want to duplicate this quest? ", "Duplicate", "Cancel"))
                        {
                            DuplicateItem(questList.quests[i]);
                            GUILayout.EndHorizontal();
                            Repaint();
                            break;
                        }
                    }
                    if (GUILayout.Button(new GUIContent("x", "Delete Quest"), GUILayout.MaxWidth(20), GUILayout.Height(45)))
                    {

                        if (EditorUtility.DisplayDialog("Delete the " + questList.quests[i].name,
                        "Are you sure you want to delete this quest? ", "Delete", "Cancel"))
                        {

                            var item = questList.quests[i];
                            questList.quests.RemoveAt(i);
                            DestroyImmediate(item, true);
                            OrderByID();
                            AssetDatabase.SaveAssets();
                            serializedObject.ApplyModifiedProperties();
                            EditorUtility.SetDirty(questList);
                            GUILayout.EndHorizontal();
                            Repaint();
                            break;
                        }
                    }

                    GUILayout.EndHorizontal();

                    GUI.color = color;
                    if (currentQuestDrawer != null && currentQuestDrawer.quest == questList.quests[i] && questList.quests.Contains(currentQuestDrawer.quest))
                    {
                        currentQuestDrawer.DrawQuest(ref questList.quests, false);
                    }

                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndVertical();


            GUILayout.EndScrollView();

            if (GUI.changed || serializedObject.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(questList);
            }
        }

        public static void SetCurrentSelectedQuest(int index)
        {
            if (Instance != null && Instance.questList != null && Instance.questList.quests != null && Instance.questList.quests.Count > 0 && index < Instance.questList.quests.Count)
            {
                Instance.currentQuestDrawer = Instance.currentQuestDrawer != null ? Instance.currentQuestDrawer.quest == Instance.questList.quests[index] ? null : new vQuestDrawer(Instance.questList.quests[index]) : new vQuestDrawer(Instance.questList.quests[index]);
                Instance.scroolView.y = 1 + index * 60;
                Instance.Repaint();
            }

        }

        protected virtual void OnDestroy()
        {
            if (questList)
            {
                questList.inEdition = false;
            }
        }

        protected virtual void DrawAddQuest()
        {
            GUILayout.BeginVertical("box");

            if (addQuest != null)
            {
                if (addQuest == null || addQuestDrawer == null || addQuestDrawer.quest == null || addQuestDrawer.quest != addQuest)
                    addQuestDrawer = new vQuestDrawer(addQuest);
                bool isValid = true;
                if (addQuestDrawer != null)
                {
                    GUILayout.Box("Create Quest Window");
                    addQuestDrawer.DrawQuest(ref questList.quests, false, true);
                }

                if (string.IsNullOrEmpty(addQuest.name))
                {
                    isValid = false;
                    EditorGUILayout.HelpBox("This quest name cant be null or empty,please type a name", MessageType.Error);
                }

                if (questList.quests.FindAll(item => item.name.Equals(addQuestDrawer.quest.name)).Count > 0)
                {
                    isValid = false;
                    EditorGUILayout.HelpBox("This quest name already exists", MessageType.Error);
                }
                GUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(false));

                if (isValid && GUILayout.Button("Create"))
                {
                    AssetDatabase.AddObjectToAsset(addQuest, AssetDatabase.GetAssetPath(questList));
                    addQuest.hideFlags = HideFlags.HideInHierarchy;
                    addQuest.id = GetUniqueID();
                    questList.quests.Add(addQuest);
                    OrderByID();
                    addQuest = null;
                    inAddQuest = false;
                    addQuestDrawer = null;
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(questList);
                    AssetDatabase.SaveAssets();
                }

                if (GUILayout.Button("Cancel"))
                {
                    addQuest = null;
                    inAddQuest = false;
                    addQuestDrawer = null;
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(questList);
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.HelpBox("Error", MessageType.Error);
            }

            GUILayout.EndVertical();

        }

        protected virtual void DuplicateItem(vQuest targetQuest)
        {
            addQuest = Instantiate(targetQuest);
            AssetDatabase.AddObjectToAsset(addQuest, AssetDatabase.GetAssetPath(questList));
            addQuest.hideFlags = HideFlags.HideInHierarchy;
            addQuest.id = GetUniqueID();
            questList.quests.Add(addQuest);
            OrderByID();
            addQuest = null;
            inAddQuest = false;
            addQuestDrawer = null;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(questList);
            AssetDatabase.SaveAssets();
        }

        protected virtual int GetUniqueID(int value = 0)
        {
            var result = value;


            for (int i = 0; i < questList.quests.Count + 1; i++)
            {
                var item = questList.quests.Find(t => t.id == i);
                if (item == null)
                {
                    result = i;
                    break;
                }

            }

            return result;
        }

        protected virtual void OrderByID()
        {
            if (questList && questList.quests != null && questList.quests.Count > 0)
            {
                var list = questList.quests.OrderBy(i => i.id).ToList();
                questList.quests = list;
            }

        }
    }
}
