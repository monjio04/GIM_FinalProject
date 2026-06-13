using UnityEngine;

public class InvestigationImageObject : MonoBehaviour, IInteractable
{
    public InvestigationData data;

    public string prompt = "[E] 조사하기";

    private bool inspected = false;

    public void Interact()
    {
        InvestigationUI.Instance.ShowImage(data);

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