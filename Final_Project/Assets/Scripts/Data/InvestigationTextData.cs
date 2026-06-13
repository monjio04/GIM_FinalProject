using UnityEngine;

[CreateAssetMenu(menuName = "Investigation/Text Data")]
public class InvestigationTextData : ScriptableObject
{
    public string title;

    [TextArea(10,30)]
    public string content;
}