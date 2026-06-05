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
 
    [Header("타자기 설정 (신규)")]
    [Tooltip("글자가 찍히는 속도입니다. 수치가 작을수록 빨라집니다.")]
    public float typeSpeed = 0.08f; 

    private DialogueData currentDialogue;
    private int currentIndex;
    private bool isTalking;
    private Action onDialogueEndCallback;

    public bool IsTalking => isTalking;
    public bool ShouldFreezePlayer => isTalking && currentDialogue != null && currentDialogue.freezePlayer;

    // ★ 딱 하나만 존재해야 하는 Awake 함수
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartDialogue(DialogueData dialogue, Action onEndCallback = null)
    {
        StopAllCoroutines();

        currentDialogue = dialogue;
        currentIndex = 0;
        onDialogueEndCallback = onEndCallback;

        isTalking = true; 

        talkUI.SetActive(true);
        talkUI.transform.SetAsLastSibling(); 

        if (dialogue.dialogueType == DialogueType.Monologue || dialogue.dialogueType == DialogueType.Narration)
        {
            speakerText.text = "";
        }
        else
        {
            speakerText.text = $"[{dialogue.speaker}]"; 
        }

        StartCoroutine(DisplayDialogueRoutine());
    }

    IEnumerator DisplayDialogueRoutine()
    {
        while (currentIndex < currentDialogue.lines.Length)
        {
            string line = currentDialogue.lines[currentIndex];

            if (line.Contains("//"))
            {
                string[] parts = line.Split(new string[] { "//" }, StringSplitOptions.None);
                for (int i = 0; i < parts.Length; i++)
                {
                    talkUI.SetActive(true);

                    yield return StartCoroutine(TypeTextRoutine(parts[i].Trim()));

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
                yield return StartCoroutine(TypeTextRoutine(line));
                yield return new WaitForSeconds(currentDialogue.autoDisplayTime);
            }

            currentIndex++;
        }

        EndDialogue();
    }

    IEnumerator TypeTextRoutine(string line)
    {
        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0; 

        int totalCharacters = line.Length;

        for (int i = 0; i <= totalCharacters; i++)
        {
            dialogueText.maxVisibleCharacters = i; 
            yield return new WaitForSeconds(typeSpeed);
        }
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