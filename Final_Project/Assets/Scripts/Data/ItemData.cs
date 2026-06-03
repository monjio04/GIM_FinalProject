using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    
    [TextArea]
    public string description;

    public Sprite icon;
}