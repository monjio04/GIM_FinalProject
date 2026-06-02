using UnityEngine;

// 상호작용 인터페이스를 상속받습니다.
public class Item : MonoBehaviour, IInteractable
{
    public ItemData itemData;
    
    [Header("획득 시 터질 독백 데이터")]
    public DialogueData pickupDialogue; 

    public void Interact()
    {
        // 1. 작성하신 인벤토리 매니저에 아이템 추가 시도
        bool isAdded = InventoryManager.Instance.AddItem(itemData);
        
        if (isAdded)
        {
            // 2. 기획서 규칙: "방수복은 예산 문제로...", "목장갑은 사비로..." 같은 독백 출력
            if (pickupDialogue != null)
            {
                TalkManager.Instance.StartDialogue(pickupDialogue);
            }
            
            // 3. 아이템을 주웠으므로 맵(필드)에서 오브젝트 제거
            Destroy(gameObject);
        }
    }
}