using UnityEngine;

public class InvestigationTextObject : MonoBehaviour, IInteractable
{
    public InvestigationTextData data;

    [Header("위령비 여부")]
    public bool isMemorial = false;

    public string prompt = "[E] 조사하기";

    private bool inspected = false;

    public void Interact()
    {
        if (isMemorial)
        {
            InvestigationUI.Instance.ShowMemorial();
        }
        else
        {
            InvestigationUI.Instance.ShowText(data);
        }

        if (!inspected)
        {
            inspected = true;
            InvestigationManager.Instance.RegisterInspection();
        }
    }

    public string GetPromptText()
    {
        return prompt;
    }
}