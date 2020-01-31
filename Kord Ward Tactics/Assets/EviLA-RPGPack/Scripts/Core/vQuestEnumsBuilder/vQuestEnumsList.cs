using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class vQuestEnumsList : ScriptableObject
{
    [SerializeField]
    public List<string> questTypeEnumValues = new List<string>();
    [SerializeField]
    public List<string> questAttributesEnumValues = new List<string>();
    [SerializeField]
    public List<string> questStateEnumValues = new List<string>();
    [SerializeField]
    public List<string> questTargetTypeEnumValues = new List<string>();
	[SerializeField]
	public List<string> questTriggerTypeEnumValues = new List<string>();

}
