using UnityEngine;
using System; 

public class NPC_Talk : MonoBehaviour, IInteractable
{
    [Header("연속으로 띄울 대사들을 순서대로 넣으세요")]
    public DialogueData[] dialogueSequence;

    [Header("상호작용 안내 문구")]
    public string interactionPrompt = "[E] 대화하기";

    public void Interact()
    {
        if (dialogueSequence != null && dialogueSequence.Length > 0)
        {
            PlayDialogue(0); 
        }
    }

    public string GetPromptText()
    {
        return interactionPrompt;
    }

    private void PlayDialogue(int index)
    {
        // 준비된 대사를 다 틀었으면
        if (index >= dialogueSequence.Length) 
        {
            // ★ 신규 추가: ProjectManager에게 대화가 모두 끝났다고 보고합니다!
            if (ProjectManager.Instance != null)
            {
                ProjectManager.Instance.OnGrandmaTalkEnd();
            }
            return;
        }

        TalkManager.Instance.StartDialogue(dialogueSequence[index], () => 
        {
            PlayDialogue(index + 1);
        });
    }
}