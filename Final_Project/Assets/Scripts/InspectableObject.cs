using UnityEngine;

public class InspectableObject : MonoBehaviour, IInteractable
{
    [Header("조사 시 터질 독백 데이터")]
    public DialogueData inspectDialogue;

    [Header("상호작용 안내 문구")]
    public string interactionPrompt = "[E] 조사하기";

    public void Interact()
    {
        // 이미 대화 중이 아닐 때만 조사 독백 시작
        if (inspectDialogue != null && !TalkManager.Instance.IsTalking)
        {
            TalkManager.Instance.StartDialogue(inspectDialogue);
        }
    }

    public string GetPromptText()
    {
        return interactionPrompt;
    }
}