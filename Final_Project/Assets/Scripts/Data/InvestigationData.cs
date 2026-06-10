using UnityEngine;

[CreateAssetMenu(fileName = "New Investigation", menuName = "Investigation/Data")]
public class InvestigationData : ScriptableObject
{
    public string title;

    [TextArea(5, 20)]
    public string content;

    [Header("조사 이미지")]
    public Sprite image;
}