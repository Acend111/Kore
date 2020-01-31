namespace EviLA.AddOns.RPGPack {
     public enum vQuestType {
       Gather=0,
       Discover=1,
       Assassinate=2,
       Multiple=3,
       MultipleAssassinate=4,
       Escort=5,
       Generic=6
     }
     public enum vQuestState {
       NotStarted=0,
       InProgress=1,
       Completed=2,
       Failed=3,
       NotAcceptedButComplete=4,
       PendingReward=5
     }
     public enum vQuestAttributes {
       Duration=0,
       QuestAmount=1,
       AutoComplete=2,
       Parallel=3,
       KillWithSpecificWeapon=4,
       HasImpactOnParent=5,
       HasImpactOnChildren=6,
       TargetCannotLeaveArea=7,
       TagCount=8,
       DropQuestItems=9,
       DropQuestItemsOnAllChildQuests=10,
       CheckpointOnStateChange=11,
       ReloadAtSpecifiedLocationOnDecline=12,
       QuestCanBeDeclined=13,
       ScriptedCountDownEnabled=14,
       ForceStartOnAccept=15,
       ExperienceGained=16,
       ActionCount=17,
       ResetDurationPerAction=18
     }
     public enum vQuestTargetType {
       vQuestTarget=0,
       vItemSeller=1,
       vQuestProvider=2
     }
     public enum vQuestTriggerType {
       AcceptOnly=0,
       Failure=1,
       Complete=2,
       AcceptAndCountDown=3
     }
}
