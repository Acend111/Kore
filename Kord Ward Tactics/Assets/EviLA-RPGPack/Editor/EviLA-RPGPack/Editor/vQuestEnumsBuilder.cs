using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using Invector;

namespace EviLA.AddOns.RPGPack
{
    public class vQuestEnumsBuilder
    {
        public static void RefreshQuestEnums()
        {
            string name = "vQuestEnums";
            string copyPath = "Assets/EviLA-RPGPack/Scripts/Core/vQuestEnumsBuilder/" + name + ".cs";

            vQuestEnumsList[] datas = Resources.LoadAll<vQuestEnumsList>("");
            List<string> defaultQuestTypeNames = new List<string>();
            List<string> defaultQuestAttributesNames = new List<string>();
            List<string> defaultQuestStateNames = new List<string>();
            List<string> defaultQuestTargetTypeNames = new List<string>();
            List<string> defaultQuestTriggerTypeNames = new List<string>();

            List<string> _questTypeNames = new List<string>();
            List<string> _questAttributeNames = new List<string>();
            List<string> _questStateNames = new List<string>();
            List<string> _questTargetTypeNames = new List<string>();
            List<string> _questTriggerTypeNames = new List<string>();

            #region Get all vQuestType values of current Enum
            try
            {
                _questTypeNames = Enum.GetNames(typeof(vQuestType)).vToList();

            }
            catch
            {

            }
            #endregion

            #region Get all vQuestAttributes values of current Enum
            try
            {
                _questAttributeNames = Enum.GetNames(typeof(vQuestAttributes)).vToList();
            }
            catch
            {

            }
            #endregion

            #region Get all vQuestState values of current Enum
            try
            {
                _questStateNames = Enum.GetNames(typeof(vQuestState)).vToList();
            }
            catch
            {

            }
            #endregion

            #region Get all QuestTargetTypes values of current Enum
            try
            {
                _questTargetTypeNames = Enum.GetNames(typeof(vQuestTargetType)).vToList();
            }
            catch
            {

            }
            #endregion

            #region Get all QuestTriggerTypes values of current Enum
            try
            {
                _questTriggerTypeNames = Enum.GetNames(typeof(vQuestTriggerType)).vToList();
            }
            catch
            {

            }
            #endregion

            if (datas != null)
            {
                #region Get all enum of QuestEnumList
                for (int i = 0; i < datas.Length; i++)
                {
                    if (datas[i].questTypeEnumValues != null)
                    {

                        for (int a = 0; a < datas[i].questTypeEnumValues.Count; a++)
                        {
                            if (!string.IsNullOrEmpty(datas[i].questTypeEnumValues[a]) && !defaultQuestTypeNames.Contains(datas[i].questTypeEnumValues[a]))
                            {

                                defaultQuestTypeNames.Add(datas[i].questTypeEnumValues[a]);
                            }
                        }
                    }

                    if (datas[i].questStateEnumValues != null)
                    {

                        for (int a = 0; a < datas[i].questStateEnumValues.Count; a++)
                        {
                            if (!string.IsNullOrEmpty(datas[i].questStateEnumValues[a]) && !defaultQuestStateNames.Contains(datas[i].questStateEnumValues[a]))
                            {

                                defaultQuestStateNames.Add(datas[i].questStateEnumValues[a]);
                            }
                        }
                    }

                    if (datas[i].questAttributesEnumValues != null)
                    {
                        for (int a = 0; a < datas[i].questAttributesEnumValues.Count; a++)
                        {
                            if (!string.IsNullOrEmpty(datas[i].questAttributesEnumValues[a]) && !defaultQuestAttributesNames.Contains(datas[i].questAttributesEnumValues[a]))
                            {
                                defaultQuestAttributesNames.Add(datas[i].questAttributesEnumValues[a]);
                            }
                        }
                    }

                    if (datas[i].questTargetTypeEnumValues != null)
                    {
                        for (int a = 0; a < datas[i].questTargetTypeEnumValues.Count; a++)
                        {
                            if (!string.IsNullOrEmpty(datas[i].questTargetTypeEnumValues[a]) && !defaultQuestTargetTypeNames.Contains(datas[i].questTargetTypeEnumValues[a]))
                            {
                                defaultQuestTargetTypeNames.Add(datas[i].questTargetTypeEnumValues[a]);
                            }
                        }
                    }

                    if (datas[i].questTriggerTypeEnumValues != null)
                    {
                        for (int a = 0; a < datas[i].questTriggerTypeEnumValues.Count; a++)
                        {
                            if (!string.IsNullOrEmpty(datas[i].questTriggerTypeEnumValues[a]) && !defaultQuestTriggerTypeNames.Contains(datas[i].questTriggerTypeEnumValues[a]))
                            {
                                defaultQuestTriggerTypeNames.Add(datas[i].questTriggerTypeEnumValues[a]);
                            }
                        }
                    }

                }
                #endregion

                #region Remove enum that not exist
                foreach (string value in defaultQuestTypeNames)
                {
                    if (!_questTypeNames.Contains(value))
                    {
                        bool replace = false;
                        for (int i = 0; i < _questTypeNames.Count; i++)
                        {
                            if (!defaultQuestTypeNames.Contains(_questTypeNames[i]))
                            {
                                replace = true;
                                _questTypeNames[i] = value;
                                break;
                            }
                        }
                        if (!replace)
                            _questTypeNames.Add(value);
                    }
                }

                var typesToRemove = _questTypeNames.FindAll(x => !defaultQuestTypeNames.Contains(x));
                foreach (string value in typesToRemove)
                    _questTypeNames.Remove(value);

                foreach (string value in defaultQuestStateNames)
                {
                    if (!_questStateNames.Contains(value))
                    {
                        bool replace = false;
                        for (int i = 0; i < _questStateNames.Count; i++)
                        {
                            if (!defaultQuestStateNames.Contains(_questStateNames[i]))
                            {
                                replace = true;
                                _questStateNames[i] = value;
                                break;
                            }
                        }
                        if (!replace)
                            _questStateNames.Add(value);
                    }
                }

                var statesToRemove = _questStateNames.FindAll(x => !defaultQuestStateNames.Contains(x));
                foreach (string value in statesToRemove)
                    _questStateNames.Remove(value);

                foreach (string value in defaultQuestAttributesNames)
                {
                    if (!_questAttributeNames.Contains(value))
                    {
                        bool replace = false;
                        for (int i = 0; i < _questAttributeNames.Count; i++)
                        {
                            if (!defaultQuestAttributesNames.Contains(_questAttributeNames[i]))
                            {
                                replace = true;
                                _questAttributeNames[i] = value;
                                break;
                            }
                        }
                        if (!replace)
                            _questAttributeNames.Add(value);
                    }
                }

                var attributesToRemove = _questAttributeNames.FindAll(x => !defaultQuestAttributesNames.Contains(x));
                foreach (string value in attributesToRemove)
                    _questAttributeNames.Remove(value);



                foreach (string value in defaultQuestTargetTypeNames)
                {
                    if (!_questTargetTypeNames.Contains(value))
                    {
                        bool replace = false;
                        for (int i = 0; i < _questTargetTypeNames.Count; i++)
                        {
                            if (!defaultQuestTargetTypeNames.Contains(_questTargetTypeNames[i]))
                            {
                                replace = true;
                                _questTargetTypeNames[i] = value;
                                break;
                            }
                        }
                        if (!replace)
                            _questTargetTypeNames.Add(value);
                    }
                }

                var questTargetTypesToRemove = _questTargetTypeNames.FindAll(x => !defaultQuestTargetTypeNames.Contains(x));
                foreach (string value in questTargetTypesToRemove)
                    _questTargetTypeNames.Remove(value);


                foreach (string value in defaultQuestTriggerTypeNames)
                {
                    if (!_questTriggerTypeNames.Contains(value))
                    {
                        bool replace = false;
                        for (int i = 0; i < _questTriggerTypeNames.Count; i++)
                        {
                            if (!defaultQuestTriggerTypeNames.Contains(_questTriggerTypeNames[i]))
                            {
                                replace = true;
                                _questTriggerTypeNames[i] = value;
                                break;
                            }
                        }
                        if (!replace)
                            _questTriggerTypeNames.Add(value);
                    }
                }

                var questTriggerTypesToRemove = _questTriggerTypeNames.FindAll(x => !defaultQuestTriggerTypeNames.Contains(x));
                foreach (string value in questTriggerTypesToRemove)
                    _questTriggerTypeNames.Remove(value);

                #endregion
            }
            CreateEnumClass(copyPath, _questTypeNames, _questAttributeNames, _questStateNames, _questTargetTypeNames, _questTriggerTypeNames);
        }

        static void CreateEnumClass(string copyPath, List<string> questTypes = null, List<string> questAttributes = null, List<string> questStateNames = null, List<string> questTargetTypes = null, List<string> questTriggerTypes = null)
        {
            if (File.Exists(copyPath)) File.Delete(copyPath);
            using (StreamWriter outfile = new StreamWriter(copyPath))
            {
                outfile.WriteLine("namespace EviLA.AddOns.RPGPack {");
                outfile.WriteLine("     public enum vQuestType {");
                if (questTypes != null)
                    for (int i = 0; i < questTypes.Count; i++)
                    {
                        outfile.WriteLine("       " + questTypes[i] + "=" + i + (i == questTypes.Count - 1 ? "" : ","));
                    }
                outfile.WriteLine("     }");

                outfile.WriteLine("     public enum vQuestState {");
                if (questStateNames != null)
                    for (int i = 0; i < questStateNames.Count; i++)
                    {
                        outfile.WriteLine("       " + questStateNames[i] + "=" + i + (i == questStateNames.Count - 1 ? "" : ","));
                    }

                outfile.WriteLine("     }");

                outfile.WriteLine("     public enum vQuestAttributes {");
                if (questAttributes != null)
                    for (int i = 0; i < questAttributes.Count; i++)
                    {
                        outfile.WriteLine("       " + questAttributes[i] + "=" + i + (i == questAttributes.Count - 1 ? "" : ","));
                    }
                outfile.WriteLine("     }");

                outfile.WriteLine("     public enum vQuestTargetType {");
                if (questTargetTypes != null)
                    for (int i = 0; i < questTargetTypes.Count; i++)
                    {
                        outfile.WriteLine("       " + questTargetTypes[i] + "=" + i + (i == questTargetTypes.Count - 1 ? "" : ","));
                    }
                outfile.WriteLine("     }");


                outfile.WriteLine("     public enum vQuestTriggerType {");
                if (questTriggerTypes != null)
                    for (int i = 0; i < questTriggerTypes.Count; i++)
                    {
                        outfile.WriteLine("       " + questTriggerTypes[i] + "=" + i + (i == questTriggerTypes.Count - 1 ? "" : ","));
                    }
                outfile.WriteLine("     }");

                outfile.WriteLine("}");
            }
            AssetDatabase.Refresh();

        }
    }
}