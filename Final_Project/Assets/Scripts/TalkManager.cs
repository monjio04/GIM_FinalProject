using TMPro;
using UnityEngine;

public class TalkManager : MonoBehaviour
{
    public GameObject talkUI;

    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;

    public DialogueData testDialogue;

    DialogueData currentDialogue;

    int currentIndex;

    bool isTalking;

    void Start()
    {
        StartDialogue(testDialogue);
    }

    void Update()
    {
        if (!isTalking)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextLine();
        }
    }

    public void StartDialogue(DialogueData dialogue)
    {
        currentDialogue = dialogue;

        currentIndex = 0;

        isTalking = true;

        talkUI.SetActive(true);

        speakerText.text = dialogue.speaker;

        dialogueText.text =
            dialogue.lines[currentIndex];
    }

    void NextLine()
    {
        currentIndex++;

        if (currentIndex >= currentDialogue.lines.Length)
        {
            EndDialogue();
            return;
        }

        dialogueText.text =
            currentDialogue.lines[currentIndex];
    }

    void EndDialogue()
    {
        isTalking = false;

        talkUI.SetActive(false);
    }
}