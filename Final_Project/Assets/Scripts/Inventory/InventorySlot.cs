using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image icon; // 자식 오브젝트인 icon의 Image 컴포넌트 연결

    private ItemData itemData;

    // 슬롯에 아이템 데이터 채우기
    public void SetItem(ItemData item)
    {
        // ★ 방어 코드 추가: icon이 연결되어 있는지 확인
        if (icon == null)
        {
            Debug.LogError($"{gameObject.name} 슬롯에 Icon 이미지가 할당되지 않았습니다!");
            return;
        }

        itemData = item;
        icon.sprite = item.icon;
        icon.enabled = true;
    }
    // 슬롯 비우기 (아이템이 없을 때)
    public void ClearSlot()
    {
        itemData = null;
        icon.sprite = null;
        icon.enabled = false; // 아이콘 이미지 끄기
    }

    public void OnClick()
    {
        // 빈 슬롯을 눌렀을 때는 아무것도 하지 않음
        if (itemData == null) return; 

        // 아이템이 있을 때만 우측 UI에 정보 표시
        InventoryUI.Instance.ShowItem(itemData);
    }
}