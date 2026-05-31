using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // 현재 플레이어가 가지고 있는 아이템 리스트
    public List<ItemData> items = new List<ItemData>();
    
    // 최대 인벤토리 칸 수 (예: 24칸)
    public int maxSlotCount = 24; 

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // 게임 중에 언제든 'I' 키 입력을 감지합니다.
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (InventoryUI.Instance != null)
            {
                InventoryUI.Instance.ToggleInventory();
            }
        }
    }

    // 게임 중간에 아이템을 얻을 때 이 함수를 실행합니다.
    public bool AddItem(ItemData newItem)
    {
        if (items.Count >= maxSlotCount)
        {
            Debug.Log("인벤토리가 가득 찼습니다!");
            return false;
        }

        items.Add(newItem);

        // 아이템을 얻었으니 UI를 새로고침합니다.
        InventoryUI.Instance.UpdateInventoryUI();
        return true;
    }
}