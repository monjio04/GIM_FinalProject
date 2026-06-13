using UnityEngine;

public class NPC_Rescue : MonoBehaviour, IInteractable
{
    [Header("구출 시 출력할 대사")]
    public DialogueData rescueDialogue;

    [Header("상호작용 안내 문구")]
    public string interactionPrompt = "[E] 구출하기";

    public void Interact()
    {
        if (rescueDialogue != null) TalkManager.Instance.StartDialogue(rescueDialogue);

        // 퀘스트 완료 처리는 ProjectManager가 타이밍에 맞게 알아서 하도록 넘깁니다!
        if (ProjectManager.Instance != null) ProjectManager.Instance.OnNpcRescued();

        Destroy(gameObject);
    }
    public string GetPromptText() { return interactionPrompt; }
}