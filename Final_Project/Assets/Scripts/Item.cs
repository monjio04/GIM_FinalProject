using UnityEngine;

// 상호작용 인터페이스를 상속받습니다.
public class Item : MonoBehaviour, IInteractable
{
    public ItemData itemData;
    
    [Header("조사 / 획득 시 터질 독백 데이터")]
    public DialogueData pickupDialogue; 

    [Header("상호작용 안내 문구")]
    public string interactionPrompt = "[E] 장비 챙기기";

    [Header("조사 전용 설정")]
    [Tooltip("체크하면 인벤토리에 들어가지 않고, 맵에서 파괴되지도 않으며 대사만 나옵니다.")]
    public bool isInvestigationOnly = false; // ◀ [★ 신규 추가]

    public void Interact()
    {
        // 1. [★ 신규 조건] 만약 조사 전용 아이템이라면?
        if (isInvestigationOnly)
        {
            // 인벤토리에 넣지 않고, 연결된 대사(독백)만 출력하고 끝냅니다!
            if (pickupDialogue != null)
            {
                TalkManager.Instance.StartDialogue(pickupDialogue);
            }
            return; // ◀ 여기서 코드를 끝내버려서 아래의 인벤토리 추가/오브젝트 파괴 코드로 안 내려갑니다!
        }

        // 2. 일반 획득 아이템일 때 (기존 로직 그대로 유지)
        bool isAdded = InventoryManager.Instance.AddItem(itemData);
        
        if (isAdded)
        {
            if (pickupDialogue != null)
            {
                TalkManager.Instance.StartDialogue(pickupDialogue);
            }
            
            Destroy(gameObject);
        }
    }

    public string GetPromptText()
    {
        return interactionPrompt;
    }
}