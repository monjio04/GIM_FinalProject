using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;

    [TextArea(2, 5)]
    public string description;

    public Sprite icon;
}