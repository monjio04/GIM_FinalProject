using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TalkManager : MonoBehaviour
{
    public static TalkManager Instance;

    public GameObject talkUI;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;

    private DialogueData currentDialogue;
    private int currentIndex;
    private bool isTalking;
    private Action onDialogueEndCallback;

    public bool IsTalking => isTalking;
    public bool ShouldFreezePlayer => isTalking && currentDialogue != null && currentDialogue.freezePlayer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartDialogue(DialogueData dialogue, Action onEndCallback = null)
    {
        if (isTalking) StopAllCoroutines();

        currentDialogue = dialogue;
        currentIndex = 0;
        onDialogueEndCallback = onEndCallback;

        talkUI.SetActive(true);

        if (dialogue.dialogueType == DialogueType.Monologue || dialogue.dialogueType == DialogueType.Narration)
            speakerText.text = "";
        else
            speakerText.text = dialogue.speaker;

        StartCoroutine(DisplayDialogueRoutine());
    }

    IEnumerator DisplayDialogueRoutine()
    {
        isTalking = true;

        while (currentIndex < currentDialogue.lines.Length)
        {
            string line = currentDialogue.lines[currentIndex];

            if (line.Contains("//"))
            {
                string[] parts = line.Split(new string[] { "//" }, StringSplitOptions.None);
                for (int i = 0; i < parts.Length; i++)
                {
                    dialogueText.text = parts[i].Trim();
                    talkUI.SetActive(true);

                    yield return new WaitForSeconds(currentDialogue.autoDisplayTime);

                    if (i < parts.Length - 1)
                    {
                        talkUI.SetActive(false);
                        yield return new WaitForSeconds(0.5f);
                    }
                }
            }
            else
            {
                dialogueText.text = line;
                yield return new WaitForSeconds(currentDialogue.autoDisplayTime);
            }

            currentIndex++;
        }

        EndDialogue();
    }

    void EndDialogue()
    {
        isTalking = false;
        talkUI.SetActive(false);

        if (onDialogueEndCallback != null)
        {
            onDialogueEndCallback.Invoke();
            onDialogueEndCallback = null;
        }
    }
}