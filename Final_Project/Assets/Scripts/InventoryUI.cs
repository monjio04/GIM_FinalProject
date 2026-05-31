using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("Right Description Panel")]
    public Image itemImage;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemDescription;

    [Header("Inventory Setup")]
    public Transform slotParent;      // 'Items' 오브젝트 연결
    public GameObject inventoryBaseUI; // 실제 눈에 보이는 인벤토리 창 패널 오브젝트 연결
    
    // 하이어라키에 이미 배치된 슬롯들을 담아둘 배열
    private InventorySlot[] slots; 

    private void Awake()
    {
        Instance = this;

        // 게임 시작 시 꺼져있는 자식 슬롯들까지 (true) 옵션으로 전부 안전하게 찾아옵니다.
        slots = slotParent.GetComponentsInChildren<InventorySlot>(true);
    }

    void Start()
    {
        // 1. 우측 설명창을 깨끗하게 비웁니다.
        ClearDescription();

        // 2. 현재 인벤토리 아이템 상황을 슬롯들에 반영합니다.
        UpdateInventoryUI();
        
        // 3. 게임이 켜지자마자 인벤토리 창을 강제로 닫고 마우스를 가둡니다.
        CloseInventory();
    }

    // 아이템을 얻거나 잃었을 때 호출되어 화면을 갱신하는 함수
    public void UpdateInventoryUI()
    {
        // 슬롯을 찾지 못했다면 에러 방지를 위해 리턴
        if (slots == null) return;

        List<ItemData> currentItems = InventoryManager.Instance.items;

        for (int i = 0; i < slots.Length; i++)
        {
            // 가지고 있는 아이템 데이터가 배치된 슬롯 순서보다 작으면 데이터를 넣어줌
            if (i < currentItems.Count)
            {
                slots[i].SetItem(currentItems[i]);
            }
            // 아이템이 없는 남은 빈 슬롯들은 비워둠
            else
            {
                slots[i].ClearSlot();
            }
        }
    }

    // 우측 설명창 초기화
    public void ClearDescription()
    {
        itemImage.sprite = null;
        itemImage.enabled = false;
        itemName.text = "";
        itemDescription.text = "";
    }

    // 슬롯 클릭 시 우측 창에 아이템 정보를 띄우는 함수
    public void ShowItem(ItemData item)
    {
        if (item == null) return;

        itemImage.enabled = true;
        itemImage.sprite = item.icon;
        itemName.text = item.itemName;
        itemDescription.text = item.description;
    }

    // 인벤토리를 완전히 닫는 내부 함수 (Start 및 필요할 때 호출)
    private void CloseInventory()
    {
        inventoryBaseUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 'I' 키를 누를 때마다 호출되는 토글 함수
    public void ToggleInventory()
    {
        // 현재 활성화 상태의 반대로 뒤집기 (켜져있으면 끄고, 꺼져있으면 켜기)
        bool isActive = !inventoryBaseUI.activeSelf;
        inventoryBaseUI.SetActive(isActive);

        if (isActive)
        {
            // 인벤토리가 열릴 때: 최신 아이템 획득 상태를 새로고침하고 마우스 커서를 풀어줌
            UpdateInventoryUI();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // 인벤토리가 닫힐 때: 다시 화면 중앙에 가두고 숨김
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}