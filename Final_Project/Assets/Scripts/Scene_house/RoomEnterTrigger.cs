using UnityEngine;

public class RoomEnterTrigger : MonoBehaviour
{
    [Header("안방 도착 시 띄울 대본")]
    public DialogueData roomEnterDialogue;

    private void OnTriggerEnter(Collider other)
    {
        // 덫을 밟은 게 플레이어라면
        if (other.CompareTag("Player"))
        {
            // 안방 도착 대사 띄우기
            if (roomEnterDialogue != null)
            {
                TalkManager.Instance.StartDialogue(roomEnterDialogue);
            }

            // 대사가 두 번 뜨지 않게 덫을 바로 지워줍니다.
            gameObject.SetActive(false); 
        }
    }
}