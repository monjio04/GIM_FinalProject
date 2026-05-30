using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest/Quest Data")]
public class QuestData : ScriptableObject
{
    public int questID;

    public string title;

    [TextArea(2,5)]
    public string description;
}