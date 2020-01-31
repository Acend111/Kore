using UnityEngine;
using UnityEditor;
using Invector.vItemManager;
using EviLA.AddOns.RPGPack;
using UnityEngine.Events;

#if UNITY_5_5_OR_NEWER
using UnityEngine.AI;
#endif
namespace EviLA.AddOns.RPGPack
{
    public class vQuestSystemNPCCreator : EditorWindow
    {
        GUISkin skin;
        GameObject charObj;
        GameObject actionObj;
        Animator charAnimator;
        RuntimeAnimatorController controller;
        Vector2 rect = new Vector2(500, 630);

        Editor humanoidpreview;

        vQuestListData questListData;
        vItemListData itemListData;
        vQuest quest;
        vQuestManager questManager;

        public enum CharacterType
        {
            QuestProvider,
            Seller,
            GenericQuestTarget,
            EscortQuestTarget,
            DiscoverQuestTarget
        }

        public CharacterType charType = CharacterType.QuestProvider;

        /// <summary>
        /// 3rdPersonController Menu 
        /// </summary>    
	    [MenuItem("Invector/Quests/Create NPC", false, 1)]
        public static void CreateNewCharacter()
        {
            GetWindow<vQuestSystemNPCCreator>();
        }

        bool isHuman, isValidAvatar, charExist;

        protected virtual void OnGUI()
        {
            if (!skin) skin = Resources.Load("skin") as GUISkin;
            GUI.skin = skin;

            this.minSize = rect;
            this.titleContent = new GUIContent("Character", null, "NPC Creator for Quest System");

            GUILayout.BeginVertical("NPC Creator for Quest System", "window");
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUILayout.BeginVertical("box");

            charType = (CharacterType)EditorGUILayout.EnumPopup("NPC Type", charType);

            if (!charObj)
                EditorGUILayout.HelpBox("Make sure to select the FBX model and not a Prefab already with components attached!", MessageType.Error);
            else if (!charExist && isHuman)
                EditorGUILayout.HelpBox("Missing a Animator Component", MessageType.Error);
            else if (!isHuman)
                EditorGUILayout.HelpBox("This is not a Humanoid", MessageType.Warning);
            else if (!isValidAvatar)
                EditorGUILayout.HelpBox(charObj.name + " is a invalid Humanoid", MessageType.Info);

            charObj = EditorGUILayout.ObjectField("FBX Model", charObj, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;

            if (charObj)
                charAnimator = charObj.GetComponent<Animator>();
            charExist = charAnimator != null;
            isHuman = charExist ? charAnimator.isHuman : false;
            isValidAvatar = charExist ? charAnimator.avatar.isValid : false;

            if (GUI.changed && charObj != null && charObj.GetComponent<IQuestTarget>() == null && charObj.GetComponentInChildren<IQuestTarget>() == null)
                humanoidpreview = Editor.CreateEditor(charObj);

            if (charObj != null && charObj.GetComponent<IQuestTarget>() != null)
            {
                EditorGUILayout.HelpBox("This gameObject already contains a " + charType.ToString() + " script", MessageType.Error);
            }

            if (charObj != null && charExist && charAnimator.runtimeAnimatorController == null)
                controller = EditorGUILayout.ObjectField("Animator Controller: ", controller, typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;

            if (charObj != null && charExist && charAnimator.runtimeAnimatorController != null)
                controller = charAnimator.runtimeAnimatorController;


            if (charType == CharacterType.QuestProvider || charType == CharacterType.Seller)
            {
                actionObj = EditorGUILayout.ObjectField("Action Text", actionObj, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
                questListData = EditorGUILayout.ObjectField("Quest List", questListData, typeof(vQuestListData), true, GUILayout.ExpandWidth(true)) as vQuestListData;
                if (charType == CharacterType.Seller)
                    itemListData = EditorGUILayout.ObjectField("Item List", itemListData, typeof(vItemListData), true, GUILayout.ExpandWidth(true)) as vItemListData;
            }
            else if (charType == CharacterType.GenericQuestTarget || charType == CharacterType.DiscoverQuestTarget
                        || charType == CharacterType.EscortQuestTarget)
            {
                quest = EditorGUILayout.ObjectField("Quest", quest, typeof(vQuest), true, GUILayout.ExpandWidth(true)) as vQuest;
                questManager = EditorGUILayout.ObjectField("Quest Manager", questManager, typeof(vQuestManager), true, GUILayout.ExpandWidth(true)) as vQuestManager;
            }

            GUILayout.EndVertical();



            if (CanCreate())
            {
                if (isHuman && isValidAvatar)
                    DrawHumanoidPreview();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (isHuman && controller != null)
                {
                    if (GUILayout.Button("Create"))
                        Create();
                }
                else if (!isHuman)
                {
                    if (GUILayout.Button("Create"))
                        Create();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        protected virtual bool CanCreate()
        {
            if (isHuman)
                return isValidAvatar && charObj != null && charObj.GetComponent<IQuestTarget>() == null && charObj.GetComponentInChildren<IQuestTarget>() == null;
            else
            {
                return charObj != null && charObj.GetComponent<IQuestTarget>() == null && charObj.GetComponentInChildren<IQuestTarget>() == null;
            }
        }

        /// <summary>
        /// Draw the Preview window
        /// </summary>
        void DrawHumanoidPreview()
        {
            GUILayout.FlexibleSpace();

            if (humanoidpreview != null && isHuman)
            {
                humanoidpreview.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(100, 400), "window");
            }
        }

        /// <summary>
        /// Created the AI Controller
        /// </summary>
        protected virtual void Create()
        {
            // base for the character
            var _3rdPerson = GameObject.Instantiate(charObj, Vector3.zero, charObj.transform.rotation) as GameObject; //Quaternion.identity) as GameObject;
            var _object = new GameObject();
            _object.transform.position = Vector3.zero;
            _object.transform.rotation = _3rdPerson.transform.rotation;//Quaternion.identity;
            _object.transform.parent = _3rdPerson.transform;

            if (!_3rdPerson)
                return;
            if (!_object)
                return;

            if (charType == CharacterType.QuestProvider)
            {
                _object.name = _3rdPerson.name;
                _object.tag = "Action";
                var provider = _object.AddComponent<vQuestProvider>();
                provider.autoAction = false;

                var o_layer = LayerMask.NameToLayer("Triggers");
                _object.layer = o_layer;
                foreach (Transform t in _object.transform.GetComponentsInChildren<Transform>())
                    t.gameObject.layer = o_layer;

                if (questListData)
                {
                    provider.questListData = questListData;
                }

            }
            else if (charType == CharacterType.Seller)
            {

                _object.name = _3rdPerson.name;
                _object.tag = "Action";
                var seller = _object.AddComponent<vItemSeller>();
                seller.autoAction = false;

                if (questListData)
                {
                    seller.questListData = questListData;
                }

                if (itemListData)
                {
                    seller.itemListData = itemListData;
                }

                var o_layer = LayerMask.NameToLayer("Triggers");
                _object.layer = o_layer;
                foreach (Transform t in _object.transform.GetComponentsInChildren<Transform>())
                    t.gameObject.layer = o_layer;
            }

            else
            {

                var p_layer = LayerMask.NameToLayer("Default");
                _object.name = _3rdPerson.name;
                _object.tag = "Untagged";

                switch (charType)
                {
                    case CharacterType.GenericQuestTarget:
                        _object.AddComponent<vQuestTarget>();
                        break;

                    case CharacterType.DiscoverQuestTarget:
                        _object.AddComponent<vDiscoverQuestTarget>();
                        break;

                    case CharacterType.EscortQuestTarget:
                        _object.AddComponent<vEscortQuestTarget>();
                        break;
                }


                if (quest)
                {
                    var target = _object.GetComponent<vQuestTarget>();
                    target.quest = quest;
                }

                var o_layer = LayerMask.NameToLayer("Triggers");

                if (charType == CharacterType.DiscoverQuestTarget)
                {
                    _object.layer = o_layer;
                    Destroy(_object.transform.parent.gameObject.GetComponent<Collider>());
                }
                else
                    _object.layer = p_layer;

                foreach (Transform t in _object.transform.GetComponentsInChildren<Transform>())
                    t.gameObject.layer = _object.layer;
            }

            // rigidbody settings
            // add only if there's no rigid body
            if (isHuman && _3rdPerson.GetComponent<Rigidbody>() == null)
            {
                var rigidbody = _3rdPerson.AddComponent<Rigidbody>();
                rigidbody.useGravity = true;
                rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                rigidbody.mass = 50;
            }

            // add only if there's no capsule collider
            // capsule collider settings
            if (_3rdPerson.GetComponent<CapsuleCollider>() == null && isHuman)
            {
                var collider = _3rdPerson.AddComponent<CapsuleCollider>();
                collider.height = ColliderHeight(_3rdPerson.GetComponent<Animator>());
                collider.center = new Vector3(0, (float)System.Math.Round(collider.height * 0.5f, 2), 0);
                collider.radius = (float)System.Math.Round(collider.height * 0.15f, 2);
            }

            else if (!isHuman)
            {
                var collider = _3rdPerson.AddComponent<BoxCollider>();
                Vector3 size = new Vector3();
                size.y = ColliderHeight(_3rdPerson.GetComponent<Animator>());

                if (_3rdPerson.transform.rotation == Quaternion.identity)
                    collider.center = new Vector3(0, (float)System.Math.Round(size.y * 0.5f, 2), 0);
                else
                    collider.center = Vector3.zero;

                size.z = size.x = ((float)System.Math.Round(size.y * 0.15f, 2)) * 4;
                collider.size = size;
            }

            if (charType != CharacterType.EscortQuestTarget
                && charType != CharacterType.GenericQuestTarget)
            {

                var object_collider = _object.AddComponent<BoxCollider>();
                Vector3 boxSize = new Vector3();
                boxSize.y = ColliderHeight(_3rdPerson.GetComponent<Animator>());

                if (_3rdPerson.transform.rotation == Quaternion.identity)
                    object_collider.center = new Vector3(0, (float)System.Math.Round(boxSize.y * 0.5f, 2), 0);
                else
                    object_collider.center = Vector3.zero;

                boxSize.z = boxSize.x = ((float)System.Math.Round(boxSize.y * 0.15f, 2)) * 5;
                object_collider.size = boxSize;
                object_collider.isTrigger = true;

            }

            if (controller)
                _3rdPerson.GetComponent<Animator>().runtimeAnimatorController = controller;

            if (actionObj != null && (charType == CharacterType.QuestProvider || charType == CharacterType.Seller))
            {
                var _actionText = GameObject.Instantiate(actionObj, Vector3.zero, Quaternion.identity, _object.transform);
                var provider = _object.GetComponent<vQuestProvider>();
                var seller = _object.GetComponent<vItemSeller>();

                if (provider)
                {

                    provider.OnPlayerEnter = new UnityEvent();
                    provider.OnPlayerExit = new UnityEvent();

                    UnityAction<bool> callback = new UnityAction<bool>(_actionText.SetActive);

                    UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(provider.OnPlayerEnter, callback, true);
                    UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(provider.OnPlayerExit, callback, false);


                }

                if (seller)
                {

                    seller.OnPlayerEnter = new UnityEvent();
                    seller.OnPlayerExit = new UnityEvent();

                    UnityAction<bool> callback = new UnityAction<bool>(_actionText.SetActive);

                    UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(seller.OnPlayerEnter, callback, true);
                    UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(seller.OnPlayerExit, callback, false);

                }

                float textPositionY = 1f;

                if (_3rdPerson.GetComponent<MeshFilter>() != null)
                    textPositionY = _3rdPerson.GetComponent<MeshFilter>().mesh.bounds.extents.y;
                else if (isHuman && _3rdPerson.GetComponent<Animator>() != null)
                    textPositionY = ColliderHeight(_3rdPerson.GetComponent<Animator>());

                _actionText.transform.position = new Vector3(0f, textPositionY, 0f);
                _actionText.SetActive(false);
                _actionText.name = _actionText.name.Replace("(Clone)", string.Empty);
            }

            _3rdPerson.name = _3rdPerson.name.Replace("(Clone)", string.Empty);
            _object.name = _object.name.Replace("(Clone)", string.Empty);


            this.Close();

        }

        /// <summary>
        /// Capsule Collider height based on the Character height
        /// </summary>
        /// <param name="animator">animator humanoid</param>
        /// <returns></returns>
        protected virtual float ColliderHeight(Animator animator)
        {
            if (isHuman)
            {
                var foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                return (float)System.Math.Round(Vector3.Distance(foot.position, hips.position) * 2f, 2);
            }

            return 2f;
        }

    }

}