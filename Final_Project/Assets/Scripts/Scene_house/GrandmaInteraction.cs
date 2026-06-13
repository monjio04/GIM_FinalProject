using UnityEngine;

public class GrandmaInteraction : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        // ProjectManager의 나머지 대화 시퀀스 호출
        ProjectManager.Instance.OnTalkToGrandma();
    }

    public string GetPromptText()
    {
        return "[E] 할머니와 대화하기";
    }
}