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
        Debug.Log($"PlayDialogue : {index}");

        if (index >= dialogueSequence.Length)
        {
            Debug.Log("모든 대사 종료");

            if (ProjectManager.Instance != null)
            {
                ProjectManager.Instance.OnGrandmaTalkEnd();
            }
            return;
        }

        TalkManager.Instance.StartDialogue(dialogueSequence[index], () =>
        {
            Debug.Log($"대사 {index} 종료");

            PlayDialogue(index + 1);
        });
    }
}