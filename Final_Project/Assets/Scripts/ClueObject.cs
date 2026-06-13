using UnityEngine;

public class ClueObject : MonoBehaviour, IInteractable
{
    public DialogueData clueDialogue;
    private bool isInvestigated = false;

    public void Interact()
    {
        if (isInvestigated) return; 
        isInvestigated = true;

        if (clueDialogue != null) TalkManager.Instance.StartDialogue(clueDialogue);
        if (ProjectManager.Instance != null) ProjectManager.Instance.OnClueFound();
    }
    public string GetPromptText() { return isInvestigated ? "" : "[E] 조사하기"; }
}